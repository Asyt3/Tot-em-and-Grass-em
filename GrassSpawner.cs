using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GrassSpawner : Node
{
    // Instead of a hardcoded path, we export a NodePath.
    // This allows you to pick the TileMapLayer directly from the scene tree in the Inspector,
    // which is much more reliable.
    [Export] private NodePath _tileMapPath;
    private TileMapLayer _tileMap;

    [Export] private PackedScene _grassScene; // Drag your Grass.tscn file here
    [Export] private Node2D _grassContainer; // A node within Tile_World to hold spawned grass
    [Export] private int _maxGrassCount = 10;
    [Export] private float _spawnInterval = 4.0f;

    private Timer _spawnTimer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public static Dictionary<Vector2I, Grass> GrassPositions { get; private set; } = new Dictionary<Vector2I, Grass>();

    public override void _Ready()
    {
        GD.Print("--- GrassSpawner Initializing ---");
        // We now get the node using the NodePath you assign in the Inspector.
        if (_tileMapPath != null)
        {
            _tileMap = GetNode<TileMapLayer>(_tileMapPath);
        }

        if (_tileMap == null || _grassScene == null || _grassContainer == null)
        {
            GD.PrintErr("STOP: GrassSpawner is not configured correctly.");
            if (_tileMap == null) GD.PrintErr(" - Tile Map Path is not assigned or is invalid.");
            if (_grassScene == null) GD.PrintErr(" - Grass Scene is not assigned.");
            if (_grassContainer == null) GD.PrintErr(" - Grass Container is not assigned.");
            SetProcess(false);
            return;
        }
        GD.Print("Configuration is valid.");

        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = _spawnInterval;
        _spawnTimer.Autostart = true;
        _spawnTimer.Timeout += SpawnGrass;
        AddChild(_spawnTimer);
        GD.Print($"Spawn timer started. Will attempt to spawn grass every {_spawnInterval} seconds.");
    }

    private void SpawnGrass()
    {
        GD.Print("Attempting to spawn grass...");

        if (GrassPositions.Count >= _maxGrassCount)
        {
            GD.Print($"Skipping spawn: Grass count ({GrassPositions.Count}) is at max ({_maxGrassCount}).");
            return;
        }

        // GetUsedCells() on a TileMapLayer does not take any arguments,
        // because the variable itself is already a specific layer.
        var usedCells = _tileMap.GetUsedCells();
        GD.Print($"Found {usedCells.Count} used cells on the tilemap layer.");

        // If you haven't painted any tiles on this layer, usedCells.Count will be 0.
        if (usedCells.Count == 0)
        {
            GD.Print("Stopping spawn attempt because no used cells were found on the tilemap layer. Have you painted any tiles?");
            return;
        }

        var emptyCells = usedCells.Where(cell => !GrassPositions.ContainsKey(cell)).ToList();

        if (emptyCells.Count == 0)
        {
            GD.Print("Stopping spawn attempt because all available cells already have grass.");
            return;
        }

        Vector2I randomCell = emptyCells[_rng.RandiRange(0, emptyCells.Count - 1)];
        
        Grass grassInstance = _grassScene.Instantiate<Grass>();

        // --- THIS IS THE FIX ---
        // 1. Get the top-left corner position of the cell.
        Vector2 localPosition = _tileMap.MapToLocal(randomCell);
        // 2. Get the size of the tiles from the TileSet.
        Vector2 halfTileSize = _tileMap.TileSet.TileSize / 2;
        // 3. Add half the tile size to the local position to find the center.
        Vector2 centeredLocalPosition = localPosition + halfTileSize;
        // 4. Convert the centered local position to a global world position.
        Vector2 worldPosition = _tileMap.ToGlobal(centeredLocalPosition);
        
        grassInstance.GlobalPosition = worldPosition;
        _grassContainer.AddChild(grassInstance);
        
        GrassPositions.Add(randomCell, grassInstance);
        GD.Print($"SUCCESS: Spawned grass at cell {randomCell}. Total grass: {GrassPositions.Count}");
    }
}
