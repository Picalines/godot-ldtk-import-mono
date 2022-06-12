#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LDtkFieldAssigner
    {
        public record Context
        {
            public int? GridSize { get; init; }
            public LDtkEntityReferenceAssigner? ReferenceAssigner { get; init; }
        }

        private record CheckContext : Context
        {
            public Node? TargetNode { get; init; }
            public string? MemberName { get; init; }
            public int? ArrayIndex { get; init; }
        }

        private record TargetFieldInfo(string EditorName, MemberInfo TargetMember);

        private static readonly Dictionary<Type, IEnumerable<TargetFieldInfo>> _TargetFields = new();

        private static readonly HashSet<Type> _NumberTypes = new()
        {
            typeof(int),
            typeof(byte),
            typeof(short),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public static TileMap? CurrentTileMap { get; set; }

        public static void Initialize()
        {
            ScanAssemblyForTargetFields();
        }

        public static void Assign(Node node, IReadOnlyDictionary<string, object> values, Context context)
        {
            var nodeScriptType = GetNodeScriptType(node);

            if (nodeScriptType is null || !_TargetFields.TryGetValue(nodeScriptType, out var targetFields))
            {
                return;
            }

            foreach (var targetField in targetFields)
            {
                if (!values.TryGetValue(targetField.EditorName, out var fieldValue))
                {
                    GD.PushWarning($"{nodeScriptType}: missing LDtk field named '{targetField.EditorName}'");
                    continue;
                }

                var targetFieldType = targetField.TargetMember switch
                {
                    FieldInfo { FieldType: var type } => type,
                    PropertyInfo { PropertyType: var type } => type,
                    _ => throw new NotImplementedException(),
                };

                var checkContext = new CheckContext()
                {
                    GridSize = context.GridSize,
                    ReferenceAssigner = context.ReferenceAssigner,
                    TargetNode = node,
                    MemberName = targetField.TargetMember.Name
                };

                if (!CheckFieldType(targetFieldType, ref fieldValue, checkContext))
                {
                    continue;
                }

                node.Set(targetField.TargetMember.Name, fieldValue);
            }
        }

        public static void Assign(Node targetNode, IEnumerable<LevelJson.FieldInstance> fieldInstances, Context context)
        {
            Assign(targetNode, fieldInstances.ToDictionary(field => field.Identifier, field => field.Value), context);
        }

        private static bool CheckFieldType(Type targetType, ref object? fieldValue, CheckContext context)
        {
            switch (fieldValue)
            {
                case null when targetType.IsClass || Nullable.GetUnderlyingType(targetType) is not null:
                case bool or string when targetType == fieldValue.GetType():
                case { } when _NumberTypes.Contains(fieldValue.GetType()) && _NumberTypes.Contains(targetType):
                    return true;

                case string colorString when targetType == typeof(Color):
                {
                    fieldValue = new Color(colorString);
                    return true;
                }

                case string enumValueName when targetType.IsEnum:
                {
                    if (!Enum.IsDefined(targetType, enumValueName))
                    {
                        GD.PushError($"LDtk field error: value '{enumValueName}' is not defined in {targetType} enum (member {context.MemberName})");
                        return false;
                    }

                    fieldValue = Enum.Parse(targetType, enumValueName);
                    return true;
                }

                case object?[] array when targetType.IsArray:
                {
                    var elementType = targetType.GetElementType();

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!CheckFieldType(elementType, ref array[i], context with { ArrayIndex = i }))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                case object?[] array when targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>):
                {
                    var elementType = targetType.GenericTypeArguments[0];

                    var checkedList = (Activator.CreateInstance(targetType, new object[] { array.Length }) as IList)!;

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!CheckFieldType(elementType, ref array[i], context with { ArrayIndex = i }))
                        {
                            return false;
                        }

                        checkedList.Add(array[i]);
                    }

                    fieldValue = checkedList;
                    return true;
                }

                case Dictionary<string, object> point when targetType == typeof(Vector2):
                {
                    var editorPoint = new Vector2() { x = Convert.ToSingle(point["cx"]), y = Convert.ToSingle(point["cy"]) };

                    var gridSizeV = Vector2.One * (context.GridSize ?? 1);
                    editorPoint *= gridSizeV;
                    editorPoint += gridSizeV / 2;

                    fieldValue = editorPoint;
                    return true;
                }

                case Dictionary<string, object> entityRef when typeof(Node).IsAssignableFrom(targetType):
                {
                    if (context.ReferenceAssigner is not { } referenceAssigner)
                    {
                        GD.PushError($"LDtk entity reference is ignored in tiles");
                        return false;
                    }

                    var targetMember = context switch
                    {
                        { MemberName: { } member, ArrayIndex: { } index } => $"{member}/{index}",
                        { MemberName: { } member } => member,
                        _ => throw new NotImplementedException(),
                    };

                    referenceAssigner.RegisterReference(context.TargetNode!, (string)entityRef["entityIid"], targetMember);
                    return true;
                }

                default:
                {
                    GD.PushError($"LDtk field type error: C# script expected value of type {targetType} for member {context.MemberName}, but received {fieldValue?.GetType().ToString() ?? "null"}");
                    return false;
                }
            }
        }

        private static Type? GetNodeScriptType(Node scene)
        {
            if (scene.GetScript() is not CSharpScript { ResourcePath: var scriptPath })
            {
                return null;
            }

            var scriptClassName = scriptPath.BaseName().GetFile();

            try
            {
                return Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Where(type => type.Name == scriptClassName)
                    .SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                GD.PushWarning($"{scriptPath}: C# class {scriptClassName} not found");
                return null;
            }
        }

        private static void ScanAssemblyForTargetFields()
        {
            _TargetFields.Clear();

            var validTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.Assembly.GetName().Name != "GodotSharp" && !type.IsDefined(typeof(CompilerGeneratedAttribute)))
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.IsSubclassOf(typeof(Node)));

            foreach (var validType in validTypes)
            {
                RegisterTargetFields(validType);
            }
        }

        private static void RegisterTargetFields(Type type)
        {
            var members = type.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                (member, _) => IsTargetMember(member),
                null);

            if (!members.Any())
            {
                return;
            }

            static TargetFieldInfo CreateFieldInfo(MemberInfo member)
            {
                var infoAttribute = member.GetCustomAttribute<LDtkFieldAttribute>();
                return new(infoAttribute.LDtkFieldName, member);
            }

            _TargetFields[type] = members.Select(CreateFieldInfo).ToList();
        }

        private static bool IsTargetMember(MemberInfo member)
        {
            if (!member.IsDefined(typeof(LDtkFieldAttribute)) || member.IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                return false;
            }

            if (member is PropertyInfo property)
            {
                if (!(property is { CanRead: true, CanWrite: true } && property.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)))
                {
                    throw new InvalidOperationException($"{nameof(LDtkFieldAttribute)} can be used only on auto properties or fields ({member.DeclaringType}.{member.Name})");
                }
            }

            if (!member.IsDefined(typeof(ExportAttribute)))
            {
                throw new InvalidOperationException($"{nameof(ExportAttribute)} is required when {nameof(LDtkFieldAttribute)} is used");
            }

            return true;
        }
    }
}

#endif