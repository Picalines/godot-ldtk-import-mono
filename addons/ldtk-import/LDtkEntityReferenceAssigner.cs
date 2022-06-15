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

        private IEnumerable<Node>? _Entities = null;

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

            PropertyListChangedNotify();
        }

        public bool IsUsed => _References.Any();
#endif

        public override void _Ready()
        {
            base._Ready();

            if (Engine.EditorHint)
            {
                return;
            }

            foreach (var pair in _References)
            {
                var node = GetNode($"../{pair.Key}");

                AssignReferences(node, pair.Value);
            }

            QueueFree();
        }

        private void AssignReferences(Node node, List<string> references)
        {
            foreach (var reference in references)
            {
                var match = _ReferenceRegex.Match(reference);
                var targetMember = match.Groups["member"]!.Value;
                var targetIdsString = match.Groups["ids"]!.Value;

                bool isArray = targetIdsString.Contains(',');
                var targetIds = targetIdsString.Split(',');

                object? targetMemberValue;

                if (!isArray)
                {
                    targetMemberValue = FindEntity(targetIds[0]);
                }
                else
                {
                    IList<Node?> targets = new List<Node?>();

                    foreach (var targetId in targetIds.Where(id => id.Length > 0))
                    {
                        targets.Add(FindEntity(targetId));
                    }

                    if (node.Get(targetMember) is Array)
                    {
                        targets = targets.ToArray();
                    }

                    targetMemberValue = targets;
                }

                node.Set(targetMember, targetMemberValue);
            }
        }

        private Node? FindEntity(string id)
        {
            _Entities ??= GetTree().GetNodesInGroup(LDtkConstants.GroupNames.Entities).OfType<Node>();

            return _Entities.FirstOrDefault(entity => (string)entity.GetMeta(LDtkConstants.MetaKeys.InstanceId) == id);
        }
    }
}
