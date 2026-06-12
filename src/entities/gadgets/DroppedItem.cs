using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class DroppedItem : RigidBody3D, IInteractable, IOnMiniMap, IBubbleable
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    private Mesh meshData = null!;

    public PackedScene ItemRef = null!;

    [Export(PropertyHint.Range, "-1,1,")]
    float buoyancy = 0.0f;

    public int stock = 0;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required CollisionShape3D CollisionShape { set; get; }

    public static DroppedItem New(Mesh mesh, PackedScene packedItem, List<Photo>? photos = null)
    {
        var dropped = Packed.Instantiate<DroppedItem>();
        dropped.meshData = mesh;
        dropped.ItemRef = packedItem;
        dropped.photosFromCamera = photos;
        return dropped;
    }

    //Photos needed to persist after a camera was dropped and the instance was freed. Could've went with a static variable but then every camera would have each other's photos.
    public List<Photo>? photosFromCamera;

    public Disposable.Restore restore = Disposable.Restore.None;
    public float RestoreAmount;

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://btgb7l7cdigqw");

    public required Mesh InteractionMesh
    {
        get => meshData;
        set;
    }

    bool IBubbleable.AxolotlTargets
    {
        get => true;
    }

    //TODO (j) IHasMesh interface to prevent having multiple properties for each other interface implementation?
    Mesh IBubbleable.Mesh => meshData;

    public Bubble? BubbleJail { set; get; }

    public override void _Ready()
    {
        Mesh.Mesh = meshData;
    }

    public override void _Process(double delta)
    {
        if (BubbleJail != null)
        {
            GlobalPosition = BubbleJail.GlobalPosition;
        }
    }

    public Item GivePickUpItem()
    {
        var item = ItemRef.Instantiate<Item>();
        if (item is Disposable dispose)
        {
            dispose.restore = restore;
            return dispose;
        }
        return item;
    }

    public void OnInteraction()
    {
        var player = GetTree().CurrentScene.GetNode<Player>();
        var inventory = player.GetNode<Inventory>()!;

        if (inventory.GetEqippedItem() != null)
            return;
        var item = ItemRef.Instantiate<Item>();
        item.stock = stock;
        if (restore != Disposable.Restore.None)
        {
            var disposable = (Disposable)item;
            disposable.restore = restore;
            disposable.restoreAmount = RestoreAmount;
        }
        else if (photosFromCamera != null)
        {
            var camera = (PhotoCamera)item;
            camera.Photos.AddRange(photosFromCamera);
        }

        inventory.AddItem(item);
        QueueFree();
    }

    public void PutInBubble()
    {
        Mesh.Visible = false;
        SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
    }

    public void FreeFromBubble()
    {
        Mesh.Visible = true;
        SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
    }
}
