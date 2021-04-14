using Godot;

namespace LDtkImport.Utils
{
    public static class NodeExtensions
    {
        public static void SetOwnerRecursive(this Node node, Node owner)
        {
            node.Owner = owner;

            foreach (Node child in node.GetChildren())
            {
                child.SetOwnerRecursive(owner);
            }
        }
    }
}
