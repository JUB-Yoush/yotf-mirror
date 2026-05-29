using System;
using System.Threading.Tasks;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class TreasureFish : Fish
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    public float WaitTimer = 3f;

    Timer eventTimer = null!;
    bool swimmingUp = false;

    public override async void _Ready()
    {
        //eventTimer = this.TimedEvent(() => swimmingUp = true, WaitTimer);
        //CreateTween().Fn(() => swimmingUp = true, WaitTimer);
        await Task.Delay(3000);
        swimmingUp = true;
    }

    //gently ocilating sin wave...
    public override void _PhysicsProcess(double delta)
    {
        if (swimmingUp)
        {
            Velocity = Vector3.Up;
            MoveAndSlide();
        }
    }
}
