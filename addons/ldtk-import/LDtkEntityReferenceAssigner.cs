using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkEntityReferenceAssigner : Node
    {
        public const string EntitiesGroupName = "LDtkEntities";

        public const string InstanceIdMetaKey = "LDtkEntityInstanceId";

        private static readonly Regex _ReferenceRegex = new($@"(?<id>.+)/(?<member>.+)");

        [Export]
        private readonly Dictionary<string, List<string>> _References = new();

#if TOOLS
        public void RegisterReference(Node owner, string targetId, string targetMember)
        {
            var ownerName = owner.Name;

            if (!_References.TryGetValue(ownerName, out var refList))
            {
                refList = new();
                _References[ownerName] = refList;
            }

            refList.Add($"{targetId}/{targetMember}");
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

            var entities = GetTree().GetNodesInGroup(EntitiesGroupName).OfType<Node>();

            foreach (var pair in _References)
            {
                var node = GetNode($"../{pair.Key}");

                foreach (var reference in pair.Value)
                {
                    var match = _ReferenceRegex.Match(reference);
                    var targetId = match.Groups["id"]!.Value;
                    var targetMember = match.Groups["member"]!.Value;

                    var target = entities.First(entity => (string)entity.GetMeta(InstanceIdMetaKey) == targetId);
                    node.Set(targetMember, target);
                }
            }
        }
    }
}
