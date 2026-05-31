using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Ghost : Fish
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required Node3D Skin { set; get; }

    public override void _Ready()
    {
        Skin.Visible = false;
        PhotoCamera.AimingChanged += SetVisibility;
    }

    public override void _ExitTree()
    {
        PhotoCamera.AimingChanged -= SetVisibility;
    }

    void SetVisibility(bool state)
    {
        Skin.Visible = state;
    }
}
