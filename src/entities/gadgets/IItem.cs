using Godot;

public interface IItem
{
    Node3D GetNode()
    {
        return (Node3D)this;
    }
}
