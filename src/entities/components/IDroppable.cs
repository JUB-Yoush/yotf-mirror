namespace Yotf;

/// <summary>
/// Implemented on Items, Allows us to create a dropped item of it.
/// </summary>
public interface IDroppable
{
    public PackedScene PackedScene { get; }
    public Mesh DropMesh { get; }
    public static DroppedItem MakeDropItem(IDroppable droppable)
    {
        Log.PrintLn($"dropping {((Node3D)droppable).Name}");
        return DroppedItem.New(droppable.DropMesh, droppable.PackedScene);
    }
}
