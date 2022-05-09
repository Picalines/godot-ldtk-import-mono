#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Picalines.Godot.LDtkImport.Importers
{
    // TODO: type checking
    // TODO: enums

    internal static class LDtkFieldAssigner
    {
        private record LDtkFieldInfo(string EditorName, MemberInfo TargetMember);

        private static readonly Dictionary<Type, IEnumerable<LDtkFieldInfo>> _TargetFields = new();

        public static void Initialize()
        {
            ScanAssemblyForTargetFields();
        }

        public static void Assign(Node entityNode, IReadOnlyDictionary<string, object> values)
        {
            var entityType = GetSceneType(entityNode);

            if (entityType is null || !_TargetFields.TryGetValue(entityType, out var targetFields))
            {
                return;
            }

            foreach (var targetField in targetFields)
            {
                if (!values.TryGetValue(targetField.EditorName, out var fieldValue))
                {
                    GD.PushWarning($"{entityType}: missing LDtk field named '{targetField.EditorName}'");
                    continue;
                }

                entityNode.Set(targetField.TargetMember.Name, fieldValue);
            }
        }

        public static void Assign(Node entityNode, LevelJson.EntityInstance entityJson)
        {
            Assign(entityNode, entityJson.FieldInstances.ToDictionary(field => field.Identifier, field => field.Value));
        }

        private static Type? GetSceneType(Node scene)
        {
            if (scene.GetScript() is not Script { ResourcePath: var scriptPath })
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
                RegisterLDtkFields(validType);
            }
        }

        private static void RegisterLDtkFields(Type type)
        {
            var members = type.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                (member, _) => IsTargetMember(member),
                null);

            if (members.Any())
            {
                _TargetFields[type] = members.Select(member => new LDtkFieldInfo(
                    member.GetCustomAttribute<LDtkFieldAttribute>().LDtkFieldName,
                    member
                )).ToList();
            }
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