using Engine.JsonConverters;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Nodes;

[JsonConverter(typeof(NodeConverter))]
public class Node
{
    private Dictionary<string, Node> _children;

    public Node()
    {
        _children = new();
    }

    [JsonConstructor]
    public Node(Dictionary<string, Node> children)
    {
        _children = children;
    }

    public virtual void Update(Node parent, int frameNumber, InputState inputState)
    {
        foreach (var node in _children.Values)
        {
            node.Update(this, frameNumber, inputState);
        }
    }

    public virtual void Draw(Node parent, Camera camera)
    {
        foreach (var node in _children.Values)
        {
            node.Draw(this, camera);
        }
    }

    public virtual Dictionary<string, Node> GetChildren() => _children;
    
    public virtual void AddChild(string name, Node child)
    {
        _children.Add(name, child);
    }

    public virtual Node? GetChild(string name)
    {
        return _children.GetValueOrDefault(name);
    }

    public virtual void RemoveChild(string name)
    {
        _children.Remove(name);
    }

    public virtual void SetChildren(Dictionary<string, Node> children)
    {
        _children = children;
    }
}