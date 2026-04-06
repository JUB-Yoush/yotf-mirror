using Godot;

public partial class InputManager : Node
{
    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
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
        else if (pEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
