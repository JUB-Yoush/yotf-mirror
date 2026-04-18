using System;
using System.Diagnostics;
using Godot;

public partial class PlayerSpawner : MultiplayerSpawner
{
    static readonly PackedScene NetworkPlayer = GD.Load<PackedScene>(
        "res://src/entities/ramy_player/ramy_player.tscn"
    );

    public override void _Ready()
    {
        Multiplayer.PeerConnected += SpawnPlayer;
    }

    private void SpawnPlayer(long id)
    {
        if (!Multiplayer.IsServer())
            return;
        var player = NetworkPlayer.Instantiate<PlayerController>();
        player.Name = id.ToString();

        GetParent().AddChild(player, true);
    }
}
