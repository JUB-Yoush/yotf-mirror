using Godot;

namespace Yotf;

public partial class InputManager : Node
{
    private Player player = null!;

    public override void _Ready()
    {
        player = GetParent<Player>();

        if (player.IsMultiplayerAuthority())
        {
            Input.SetMouseMode(Input.MouseModeEnum.Captured);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("quit"))
        {
            GetTree().Quit();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("unfocus"))
        {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
            GetViewport().SetInputAsHandled();
        }

        if (@event.IsActionPressed("reload_scene"))
            GetTree().ReloadCurrentScene();

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && !player.IsInMenu)
        {
            if (Input.MouseMode != Input.MouseModeEnum.Captured)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
                //TODO(j) was making the ui uninteractable. not sure what the consequences will be.
                //GetViewport().SetInputAsHandled();
            }

            // switch (mouseEvent.ButtonIndex)
            // {
            //     case MouseButton.WheelUp:
            //         player.MoveSpeed = Mathf.Clamp(player.MoveSpeed + 5, 2, 500);
            //         break;
            //     case MouseButton.WheelDown:
            //         player.MoveSpeed = Mathf.Clamp(player.MoveSpeed - 5, 2, 500);
            //         break;
            // }
        }

        //TODO (j) wrap this in some debug mode checker
        // if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        // {
        //     switch (keyEvent.Keycode)
        //     {
        //         case Key.V:
        //             player.FirstPerson = !player.FirstPerson;
        //             break;
        //         case Key.C:
        //             player.CollisionEnabled = !player.CollisionEnabled;
        //             break;
        //         case Key.F:
        //             player.SetState(
        //                 player.CurrentState == player.SwimmingState
        //                     ? player.WalkingState
        //                     : player.SwimmingState
        //             );
        //             break;
        //     }
        // }
    }
}
