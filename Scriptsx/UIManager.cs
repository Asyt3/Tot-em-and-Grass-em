using Godot;

public partial class UIManager : CanvasLayer
{
    private UpgradeMenu _upgradeMenu;

    public override void _Ready()
    {
        _upgradeMenu = GetNode<UpgradeMenu>("UpgradeMenu");
        if (_upgradeMenu == null)
        {
            GD.PrintErr("UIManager could not find UpgradeMenu child node.");
            return;
        }

        // --- THIS IS THE KEY ---
        // We connect to the custom signal from our child instance in code.
        _upgradeMenu.CloseRequested += OnCloseMenu;
    }

    // This method is called by the Totem's "Interacted" signal
    public void ShowTotemMenu()
    {
        _upgradeMenu.Show();
    }

    // This method is now correctly called by the UpgradeMenu's "CloseRequested" signal
    private void OnCloseMenu()
    {
        _upgradeMenu.Hide();
    }
}

