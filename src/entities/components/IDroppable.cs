namespace Yotf;

public interface IDroppable
{
    public PackedScene PackedScene { get; }
    public Mesh DropMesh { get; }
    public static DroppedItem MakeDropItem(IDroppable droppable) =>
        DroppedItem.New(droppable.DropMesh, droppable.PackedScene);
    public DroppedItem MakeDropItem() => DroppedItem.New(DropMesh, PackedScene);
}
