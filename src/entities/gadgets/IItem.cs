using Godot;

public interface IItem
{
    static readonly Texture2D moonin = GD.Load<Texture2D>("res://assets/2d/mooninicon.png");
    string GetItemName();
    Node3D GetNode() => (Node3D)this;
    Texture2D GetIcon();
    void Enter() { }
    void Update(double delta) { }
    void Exit() { }
}
