using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class InkArea : Area3D, IPhotographable
{
    public override void _Notification(int what) => this.Notify(what);

    static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/fish/inkfish/ink_area.tscn"
    );

    Inkfish parent = null!;

    [Node]
    public required CollisionShape3D InkCollider { set; get; }

    [Node]
    public required VisibleOnScreenNotifier3D VisibilityNotifier { set; get; }

    public required MeshInstance3D SubjectBoundingMesh
    {
        get => parent.SubjectBoundingMesh;
        set;
    }

    public Node3D Subject
    {
        get => this;
        set;
    }

    public bool IsModifier
    {
        get => true;
    }

    public static InkArea New(Inkfish parent)
    {
        var inkArea = Packed.Instantiate<InkArea>();
        inkArea.parent = parent;
        parent.SceneRoot().AddChild(inkArea);
        inkArea.GlobalPosition = parent.GlobalPosition;
        return inkArea;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalTransform = parent.GlobalTransform;
    }

    public bool IsInPhoto() => VisibilityNotifier.IsOnScreen();
}
