using Godot;

public partial class InputManager : Node
{
    private PlayerController _player = null!;

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();

        if (_player.IsMultiplayerAuthority())
        {
            Input.SetMouseMode(Input.MouseModeEnum.Captured);
        }
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent.IsActionPressed("quit"))
        {
            GetTree().Quit();
            GetViewport().SetInputAsHandled();
        }
        else if (pEvent.IsActionPressed("unfocus"))
        {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
            GetViewport().SetInputAsHandled();
        }

        if (pEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (Input.MouseMode != Input.MouseModeEnum.Captured)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
                GetViewport().SetInputAsHandled();
            }

            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelUp:
                    _player.MoveSpeed = Mathf.Clamp(_player.MoveSpeed + 5, 2, 500);
                    break;
                case MouseButton.WheelDown:
                    _player.MoveSpeed = Mathf.Clamp(_player.MoveSpeed - 5, 2, 500);
                    break;
            }
        }

        if (pEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.V:
                    _player.FirstPerson = !_player.FirstPerson;
                    break;
                case Key.C:
                    _player.CollisionEnabled = !_player.CollisionEnabled;
                    break;
                case Key.F:
                    _player.SetState(
                        _player.CurrentState == _player.SwimmingState
                            ? _player.WalkingState
                            : _player.SwimmingState
                    );
                    break;
            }
        }
    }
}
