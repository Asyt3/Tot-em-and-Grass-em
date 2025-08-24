using Godot;

// The Grass class no longer needs any logic. It's just a marker.
public partial class Grass : Node2D // Or Area2D, either is fine.
{
    // The spawner sets this, but no other code needs to read it.
    // It's good to keep for potential future features or debugging.
    public Vector2I Cell { get; set; }
}
