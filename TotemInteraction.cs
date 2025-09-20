using Godot;

public partial class TotemInteraction : Area2D
{
	[Signal]
	public delegate void InteractedEventHandler();

	// This ensures mouse events are handled.
	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			GD.Print("Totem clicked!");
			EmitSignal(SignalName.Interacted);
		}
	}
}
