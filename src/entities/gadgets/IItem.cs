using Godot;

public interface IItem
{
    string GetItemName();
    Node3D GetNode() => (Node3D)this;
    Texture2D GetIcon();
    void Update(double delta);
}
