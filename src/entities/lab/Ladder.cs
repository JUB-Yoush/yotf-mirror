using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Ladder : Node3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    float climbForce = 10f;

    [Export]
    float ladderLength = 3f;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    public Mesh InteractionMesh => Mesh.Mesh;

    public void OnInteraction()
    {
        var player = this.SceneRoot().GetNode<Player>()!;
        CreateTween()
            .AnimateProperty(
                player,
                CharacterBody3D.PropertyName.GlobalPosition,
                GlobalPosition + Vec3.Up,
                1
            );
    }
}
