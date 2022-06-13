using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkEntityReferenceAssigner : Node
    {
        private static readonly Regex _ReferenceRegex = new(@"(?<member>.+)::(?<ids>.+)");

        [Export]
        private readonly Dictionary<string, List<string>> _References = new();

#if TOOLS
        public void RegisterReference(Node owner, string targetId, string targetMember, bool addToArray)
        {
            var ownerName = owner.Name;

            if (!_References.TryGetValue(ownerName, out var refList))
            {
                refList = new();
                _References[ownerName] = refList;
            }

            if (!addToArray)
            {
                refList.Add($"{targetMember}::{targetId}");
            }
            else
            {
                var existingRef = refList.FirstOrDefault(reference => reference.StartsWith(targetMember + "::")) ?? $"{targetMember}::";
                refList.Remove(existingRef);

                existingRef += targetId + ",";

                refList.Add(existingRef);
            }
        }

        public void Serialize()
        {
            Set(nameof(_References), _References);
        }
#endif

        public override void _Ready()
        {
            base._Ready();

            if (Engine.EditorHint)
            {
                return;
            }

            var entities = GetTree().GetNodesInGroup(LDtkEditorPlugin.GroupNames.Entities).OfType<Node>();

            Node? FindEntity(string id)
            {
                return entities.FirstOrDefault(entity => (string)entity.GetMeta(LDtkEditorPlugin.MetaKeys.InstanceId) == id);
            }

            foreach (var pair in _References)
            {
                var node = GetNode($"../{pair.Key}");

                foreach (var reference in pair.Value)
                {
                    var match = _ReferenceRegex.Match(reference);
                    var targetMember = match.Groups["member"]!.Value;
                    var targetIdsString = match.Groups["ids"]!.Value;

                    bool isArray = targetIdsString.Contains(',');
                    var targetIds = targetIdsString.Split(',');

                    if (!isArray)
                    {
                        node.Set(targetMember, FindEntity(targetIds[0]));
                    }
                    else
                    {
                        var targets = new List<Node?>() as IList<Node?>;

                        foreach (var targetId in targetIds.Where(id => id.Length > 0))
                        {
                            targets.Add(FindEntity(targetId));
                        }

                        if (node.Get(targetMember) is Array)
                        {
                            targets = targets.ToArray();
                        }

                        node.Set(targetMember, targets);
                    }
                }
            }
        }
    }
}
