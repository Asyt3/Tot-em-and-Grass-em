using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GrassSpawner : Node
{
    [Export] private NodePath _tileMapPath;
    private TileMapLayer _tileMap;

    [Export] private PackedScene _grassScene;
    [Export] private Node2D _grassContainer;
    [Export] private int _maxGrassCount = 10;
    [Export] private float _spawnInterval = 1.0f;

    private Timer _spawnTimer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public static Dictionary<Vector2I, Grass> GrassPositions { get; private set; } = new Dictionary<Vector2I, Grass>();

    public override void _Ready()
    {
        if (_tileMapPath != null)
        {
            _tileMap = GetNode<TileMapLayer>(_tileMapPath);
        }

        if (_tileMap == null || _grassScene == null || _grassContainer == null)
        {
            GD.PrintErr("STOP: GrassSpawner is not configured correctly.");
            SetProcess(false);
            return;
        }

        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = _spawnInterval;
        _spawnTimer.Autostart = true;
        _spawnTimer.Timeout += SpawnGrass;
        AddChild(_spawnTimer);
    }

    private void SpawnGrass()
    {
        if (GrassPositions.Count >= _maxGrassCount) return;

        var usedCells = _tileMap.GetUsedCells();
        var emptyCells = usedCells.Where(cell => !GrassPositions.ContainsKey(cell)).ToList();

        if (emptyCells.Count == 0) return;

        Vector2I randomCell = emptyCells[_rng.RandiRange(0, emptyCells.Count - 1)];
        
        Grass grassInstance = _grassScene.Instantiate<Grass>();
        grassInstance.Cell = randomCell;

        // --- POSITIONING FIX ---
        // Place grass at the top-left corner of the tile, just like the player.
        // This ensures perfect consistency.
        Vector2 worldPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(randomCell));
        
        grassInstance.GlobalPosition = worldPosition;
        _grassContainer.AddChild(grassInstance);
        
        GrassPositions.Add(randomCell, grassInstance);
    }
}
