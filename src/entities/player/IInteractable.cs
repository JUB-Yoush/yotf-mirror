using System;
using Godot;

namespace Yotf;

// areas that implement this interface must be on collision layer 3
public interface IInteractable
{
    public void OnInteraction();
}
