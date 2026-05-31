using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Log : Control
{
    public override void _Notification(int what) => this.Notify(what);

    private static Log Instance { get; set; } = null!;
    public static int MsgCount = 0;
    public const int LOG_LIMIT = 500;

    [Node]
    public required VBoxContainer LogMessages { set; get; }

    static readonly PackedScene LogMsg = GD.Load<PackedScene>(
        "res://src/helper_nodes/log_label.tscn"
    );

    public override void _Ready()
    {
        Instance = this;
        Instance.LogMessages = GetNode<VBoxContainer>("%LogMessages");
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

    public static void PrintLn(
        object message,
        object? message1 = null,
        object? message2 = null,
        object? message3 = null,
        object? message4 = null,
        object? message5 = null,
        object? message6 = null,
        object? message7 = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        bool newlines = true
    )
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        object?[] messages =
        [
            message,
            message1,
            message2,
            message3,
            message4,
            message5,
            message6,
            message7,
        ];
        StringBuilder output = new("");
        foreach (var msg in messages)
        {
            if (msg == null)
                continue;

            if (newlines)
            {
                output.Append('\n');
                output.Append(msg.ToString());
            }
            else
            {
                output.Append(msg.ToString());
                output.Append('|');
            }
        }
        if (!newlines && output.Length > 0)
            output.Remove(output.Length - 1, 1);
        GD.Print($"[{className}.{memberName}:{lineNumber}] {output}");
    }

    public static void PrintLn(
        object[] messages,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        bool newlines = false
    )
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        StringBuilder output = new("");
        foreach (var msg in messages)
        {
            if (msg == null)
                continue;

            if (newlines)
            {
                output.Append('\n');
                output.Append(msg.ToString());
            }
            else
            {
                output.Append(msg.ToString());
                output.Append('|');
            }
        }
        if (!newlines)
            output.Remove(output.Length - 1, 1);
        GD.Print($"[{className}.{memberName}:{lineNumber}] {output}");
    }
}
