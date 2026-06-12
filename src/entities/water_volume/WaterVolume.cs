using System;
using Godot;
using Yotf;

[Tool]
public partial class WaterVolume : CsgBox3D
{
    // ====================== EXPORTS ======================
    [Export]
    public Color WaterColor = new(0.3098039f, 0.5411765f, 0.8666667f, 0.3882353f);

    [Export]
    public Color FogColor = new(0f, 0.04313726f, 0.1568628f);

    [Export(PropertyHint.Range, "0.0,250.0")]
    public float FogFadeDist = 5.0f;

    // ====================== REFERENCES ======================
    private ShapeCast3D _camShapeCast = null!;
    private Area3D _swimmableArea = null!;
    private FogVolume _fogVolume = null!;
    private TextureRect _waterRippleOverlay = null!;
    private CollisionShape3D? _collisionShape;

    // Prevents multiple overlapping water bodies from drawing the effect in the same frame.
    private static long _lastFrameDrawUnderwaterEffect = -999L;

    public override void _Ready()
    {
        _camShapeCast = GetNode<ShapeCast3D>("%CameraPosShapeCast3D");
        _swimmableArea = GetNode<Area3D>("%SwimmableArea3D");
        _fogVolume = GetNode<FogVolume>("%FogVolume");
        _waterRippleOverlay = GetNode<TextureRect>("%WaterRippleOverlay");
        _collisionShape = GetNodeOrNull<CollisionShape3D>("%CollisionShape3D");

        _swimmableArea.BodyEntered += OnBodyEntered;
        _swimmableArea.BodyExited += OnBodyExited;

        ProcessPriority = 999;
    }

    private void OnBodyExited(Node3D body)
    {
        if (Engine.IsEditorHint())
            return;
        if (body is Player player && !player.InNegationArea)
        {
            // for handling overlapping water volumes, only set the player to walking state if they have no more water volumes affecting them
            player.WaterVolumeCount = Mathf.Max(0, player.WaterVolumeCount - 1);
            if (player.WaterVolumeCount == 0)
                player.SetState(player.WalkingState);
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (Engine.IsEditorHint())
            return;

        if (body is Player player && !player.InNegationArea)
        {
            // for handling overlapping water volumes, only set the player to walking state if they have no more water volumes affecting them
            player.WaterVolumeCount++;
            player.SetState(player.SwimmingState);
        }
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(_fogVolume))
            return;

        UpdateMesh();

        if (Material is StandardMaterial3D mat)
        {
            mat.AlbedoColor = WaterColor;
        }

        ShaderMaterial? fogMat = _fogVolume.Material as ShaderMaterial;
        fogMat?.SetShaderParameter("albedo", FogColor);
        fogMat?.SetShaderParameter("emission", FogColor);
        _fogVolume.Size = Size;
        _fogVolume.Set("fade_distance", (int)FogFadeDist);

        if (Engine.IsEditorHint())
            return;

        if (ShouldDrawCameraUnderwaterEffect())
        {
            _waterRippleOverlay.Visible = true;
            fogMat?.SetShaderParameter("edge_fade", 0.1f);
            _lastFrameDrawUnderwaterEffect = (long)Engine.GetProcessFrames();
        }
        else
        {
            _waterRippleOverlay.Visible = false;
            fogMat?.SetShaderParameter("edge_fade", 1.1f);
        }
    }

    // ====================== HELPERS ======================

    private void UpdateMesh()
    {
        if (_collisionShape?.Shape is BoxShape3D box)
            box.Size = Size;
    }

    private bool ShouldDrawCameraUnderwaterEffect()
    {
        Camera3D? camera = GetViewport()?.GetCamera3D();
        if (camera == null)
            return false;

        Aabb aabb = GlobalTransform * GetAabb().Grow(0.025f);
        if (!aabb.HasPoint(camera.GlobalPosition))
            return false;

        if (_lastFrameDrawUnderwaterEffect == (long)Engine.GetProcessFrames())
            return false;

        _camShapeCast.GlobalPosition = camera.GlobalPosition;
        _camShapeCast.ForceShapecastUpdate();
        for (int i = 0; i < _camShapeCast.GetCollisionCount(); i++)
        {
            if (_camShapeCast.GetCollider(i) == _swimmableArea)
                return true;
        }

        return false;
    }
}
