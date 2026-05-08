using System;
using Godot;

namespace Yotf;

public partial class NetUi : Control
{
    public override void _Ready()
    {
        LineEdit ipInput = GetNode<LineEdit>("HBox/IPInput");
        LineEdit portInput = GetNode<LineEdit>("HBox/PortInput");

        GetNode<Button>("HBox/ServerBtn").Pressed += () =>
            NetworkHandler.StartServer(portInput.Text);
        GetNode<Button>("HBox/ClientBtn").Pressed += () =>
            NetworkHandler.StartClient(ipInput.Text, portInput.Text);

        foreach (var arg in OS.GetCmdlineArgs())
        {
            if (arg == "--client")
                NetworkHandler.StartClient(ipInput.Text, portInput.Text);
            else if (arg == "--host")
                NetworkHandler.StartServer(portInput.Text);
        }
    }
}
