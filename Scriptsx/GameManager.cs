using Godot;

public partial class GameManager : Node
{
    private int _grassCollected = 0;
    public int GrassCollected => _grassCollected;

    [Signal]
    public delegate void GrassCountChangedEventHandler(int newCount);

    public void AddGrass(int amount)
    {
        _grassCollected += amount;
        EmitSignal(SignalName.GrassCountChanged, _grassCollected);
    }

    public bool SpendGrass(int amount)
    {
        if (_grassCollected >= amount)
        {
            _grassCollected -= amount;
            EmitSignal(SignalName.GrassCountChanged, _grassCollected);
            return true;
        }
        return false;
    }
}

