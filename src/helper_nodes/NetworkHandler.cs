using System;
using Godot;

namespace Yotf;

public partial class NetworkHandler : Node
{
    public static NetworkHandler Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Instance.Multiplayer.ServerDisconnected += Instance.OnServerDisconnected;
    }

    public required ENetMultiplayerPeer peer;
    public const string IP_ADDRESS = "localhost";
    public const int PORT = 42069;

    public static void StartServer(string port)
    {
        GD.Print($"Starting server on port {port}...");
        try
        {
            Instance.peer = new ENetMultiplayerPeer();
            Instance.peer.CreateServer(int.Parse(port));
            Instance.Multiplayer.MultiplayerPeer = Instance.peer;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Couldn't start server: {e}");
        }
    }

    public static void StartClient(string ip, string port)
    {
        GD.Print($"Joining server at {ip}:{port}...");
        try
        {
            Instance.peer = new ENetMultiplayerPeer();
            Instance.peer.CreateClient(ip, int.Parse(port));
            Instance.Multiplayer.MultiplayerPeer = Instance.peer;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Couldn't join server: {e}");
        }
    }

    private void OnServerDisconnected()
    {
        GD.Print("Server disconnected.");
        peer?.Close();
        Multiplayer.MultiplayerPeer = null;
        GetTree().ReloadCurrentScene();
    }
}
