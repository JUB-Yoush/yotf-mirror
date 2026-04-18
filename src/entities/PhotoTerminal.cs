using System;
using Godot;

public partial class PhotoTerminal : Node3D
{
    // [Rpc(
    //     MultiplayerApi.RpcMode.AnyPeer,
    //     CallLocal = true,
    //     TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
    //     TransferChannel = 0
    // )]
    // public void ChangeDisplayImage(Guid guid)
    // {
    //     var imgTex = new ImageTexture();
    //     imgTex.SetImage(img);
    //     var sprite = GetNode<Sprite3D>("Sprite3D");
    //     sprite.Texture = imgTex;
    // }
}
