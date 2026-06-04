using Godot;

public partial class FogVolumeFadeScript : FogVolume
{
    [Export]
    public int FadeDistance = 5;

    public override void _Process(double _)
    {
        Camera3D? cam = GetViewport()?.GetCamera3D();
        if (cam == null)
            return;

        Vec3 fadePlaneNormal = cam.GlobalTransform.Basis.Z * -1;
        Vec3 fadePlanePos =
            cam.GlobalTransform.Origin + cam.GlobalTransform.Basis.Z * -FadeDistance;
        float fadePlaneDistance = fadePlanePos.Dot(fadePlaneNormal);
        Vector4 fadePlane = new(
            fadePlaneNormal.X,
            fadePlaneNormal.Y,
            fadePlaneNormal.Z,
            fadePlaneDistance
        );
        (Material as ShaderMaterial)?.SetShaderParameter("fade_plane", fadePlane);
    }
}
