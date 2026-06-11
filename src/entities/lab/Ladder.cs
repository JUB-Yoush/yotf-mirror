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

    [Export]
    bool isTop = false;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Marker3D BottomMarker { set; get; }

    public Mesh InteractionMesh => Mesh.Mesh;

    public void OnInteraction()
    {
        var player = this.SceneRoot().GetNode<Player>()!;
        if (isTop)
            return;
        if (player.InNegationArea)
        {
            var downTween = CreateTween();
            downTween.AnimateProperty(
                player,
                CharacterBody3D.PropertyName.GlobalPosition,
                BottomMarker.GlobalPosition,
                1
            );
            downTween.AnimateProperty(
                player,
                CharacterBody3D.PropertyName.GlobalPosition,
                BottomMarker.GlobalPosition + Vector3.Back,
                .1f
            );

            return;
        }
        var tween = CreateTween();
        tween.AnimateProperty(
            player,
            CharacterBody3D.PropertyName.GlobalPosition,
            GlobalPosition + Vec3.Up,
            1
        );
        tween.AnimateProperty(
            player,
            CharacterBody3D.PropertyName.GlobalPosition,
            GlobalPosition - GlobalTransform.Basis.Z,
            .1f
        );
    }
}
