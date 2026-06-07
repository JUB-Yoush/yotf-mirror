namespace Yotf;

//appears in sonar
public interface ISonarable
{
    public Vec3 GlobalPosition
    {
        get => ((Node3D)this).GlobalPosition;
    }

    public StringName Name
    {
        get => ((Node3D)this).Name;
    }

    public float DiscoveryDistance
    {
        get => 10;
    }

    public bool Discovered { get; set; }
}
