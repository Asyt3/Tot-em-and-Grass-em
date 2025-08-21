using Godot;

public partial class Grass : Area2D
{
    // This signal will be emitted when the grass is clicked.
    // We'll connect to it from the GrassSpawner.
    [Signal]
    public delegate void CollectedEventHandler(Vector2I cell, Grass grassNode);

    // The spawner will set this property so the grass knows its own location.
    public Vector2I Cell { get; set; }

    private Area2D _clickableArea;

    public override void _Ready()
    {
        // We find the Area2D child node to connect its signal.
        _clickableArea = GetNode<Area2D>("Area2D");
        // Connect the input_event signal from the Area2D to our handler method.
        _clickableArea.InputEvent += OnClickableAreaInputEvent;
    }

    // This method is called whenever the Area2D is clicked or hovered over.
    private void OnClickableAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        // We only care about the left mouse button being pressed.
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            GD.Print($"Grass at cell {Cell} was clicked for collection.");
            // Emit the signal with our cell position and a reference to ourselves.
            EmitSignal(SignalName.Collected, Cell, this);
        }
    }
}
