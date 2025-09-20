using Godot;

public partial class UpgradeMenu : Control
{
    // --- NEW: Define a new signal that this scene can emit. ---
    [Signal]
    public delegate void CloseRequestedEventHandler();

    public override void _Ready()
    {
        // Start hidden. The UIManager will show this menu.
        Hide();
    }

    private void OnUpgradeButtonPressed()
    {
        GD.Print("Upgrade button pressed!");
        // Example: GetNode<GameManager>("/root/GameManager").TryPurchaseUpgrade("more_grass");
    }

    // --- NEW: This method will be connected to the CloseButton's pressed() signal. ---
    // It's job is to emit our custom signal.
    private void OnCloseButtonPressed()
    {
        EmitSignal(SignalName.CloseRequested);
    }
}

