using Godot;

// The Grass class is now just a simple data container.
// It has no logic and does not handle signals or input.
public partial class Grass : Node2D
{
    public Vector2I Cell { get; set; }
}

