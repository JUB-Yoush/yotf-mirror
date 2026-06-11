using System.Collections.Generic;

namespace Yotf;

/// <summary>
/// Node3Ds that implement this interface can appear in photos.
/// </summary>
public interface IPhotographable
{
    enum PhotoModifier
    {
        None,
        Ink,
        Treasure,
    }

    public MeshInstance3D SubjectBoundingMesh { set; get; }
    public Node3D Subject
    {
        get => (Node3D)this;
    }
    public PhotoModifier Modifier { get; set; }
    public bool IsModifier
    {
        get => Modifier != PhotoModifier.None;
    }
    public List<IGiveLight> NearbyLights { get; set; }
    public bool IsInPhoto();

    public void OnReceivedLight(IGiveLight light)
    {
        NearbyLights.Add(light);
    }

    public void OnRemovedLight(IGiveLight light)
    {
        NearbyLights.Remove(light);
    }
    public virtual void Photographed() { }
}
