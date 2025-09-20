using Godot;

public partial class HUD : CanvasLayer
{
    private Label _grassCountLabel;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _grassCountLabel = GetNode<Label>("GrassCountLabel");
        
        // Find the GameManager Autoload
        _gameManager = GetNode<GameManager>("/root/GameManager");
        if (_gameManager != null)
        {
            // Connect to the signal
            _gameManager.GrassCountChanged += OnGrassCountChanged;
            // Update the label with the initial value
            OnGrassCountChanged(_gameManager.GrassCollected);
        }
        else
        {
             GD.PrintErr("HUD could not find GameManager. Make sure it's set up as an Autoload.");
        }
    }

    private void OnGrassCountChanged(int newCount)
    {
        _grassCountLabel.Text = $"Grass: {newCount}";
    }
}

