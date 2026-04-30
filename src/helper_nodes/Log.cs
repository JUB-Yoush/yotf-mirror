using System;
using Godot;

public partial class Log : Control
{
    public static Log Instance { get; private set; } = null!;
    public static int MsgCount = 0;
    public const int LOG_LIMIT = 500;
    VBoxContainer LogMessages = null!;

    static readonly PackedScene LogMsg = GD.Load<PackedScene>(
        "res://src/helper_nodes/log_label.tscn"
    );

    public override void _Ready()
    {
        Instance = this;
        Instance.LogMessages = GetNode<VBoxContainer>("%LogMessages");
        Print("DEBUG LOG");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_log"))
        {
            Instance.Visible = !Instance.Visible;
        }
    }

    public static void Print(object msg)
    {
        if (MsgCount == LOG_LIMIT)
        {
            Instance.LogMessages.GetChild<Label>(0).QueueFree();
        }
        msg = $"{MsgCount}: {msg}";
        var label = LogMsg.Instantiate<Label>();
        label.Text = msg.ToString();
        Instance.LogMessages.AddChild(label);
        MsgCount++;
    }
}
