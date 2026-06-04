namespace Yotf;

/// <summary>
/// Implemented on Fish, Allows us to check if it's in it's action.
/// Could probably just exist in the parent fish class.
/// </summary>
public interface IDoesAction
{
    public bool InAction { set; get; }
}
