using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class DisposableRestore : Item
{
    public enum Restore
    {
        None,
        Oxygen,
        Battery,
        Injuries,
    }

    [Export]
    float restoreAmount = 50;

    //public static Dictionary<Restores,(float,float)> RestoreMap = [{Oxygen,()}];

    public static PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/disposable_refill/disposable_restore.tscn"
    );

    [Export]
    public Mesh OnlyDropMesh = null!;

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => OnlyDropMesh;

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("use_item"))
            Use();

        if (@event.IsActionPressed("drop_item"))
        {
            var player = this.SceneRoot().GetNode<Player>()!;
            var camera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
            var dropItem = IDroppable.MakeDropItem(this);
            dropItem.GlobalTransform = camera.GlobalTransform;
            dropItem.Position += -camera.GlobalTransform.Basis.Z;
            GetTree().CurrentScene.AddChild(dropItem);
            player.Inventory.RemoveCurrentItem();
        }
    }

    private void Use()
    {
        var player = this.SceneRoot().GetNode<Player>()!;
        player.Stats.RestoreStat(restore, restoreAmount);
        player.Inventory.RemoveCurrentItem();
    }

    // public static (ref float, ref float) RestoreMap(PlayerStats stats,Restore restore) => restore switch{
    //     Restore.None => new Exception("cannot restore from none type"),
    //     Restore.Oxygen => (stats.Oxygen,stats.MaxBattery)
    // }
}
