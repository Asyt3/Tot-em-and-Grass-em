using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GrassCollector : Node2D
{
    public enum AoeShape { SingleTile, Cross, Square3x3 }

    [Export] private Node2D _tileWorldInstance;
    [Export] public AoeShape CurrentAoeShape { get; set; } = AoeShape.Cross;
    [Export] private float _rippleDelayPerTile = 0.08f;
    [Export] private float _rippleTotalDuration = 0.5f;

    private TileMapLayer _tileMap;
    private TileWorld _tileWorld;
    private GameManager _gameManager;

    public override void _Ready()
    {
        this.ZIndex = 10;
        _gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
        if (_gameManager == null) GD.PrintErr("GrassCollector could not find GameManager.");

        if (_tileWorldInstance != null)
        {
            _tileWorld = _tileWorldInstance.GetNode<TileWorld>("TileMap");
            _tileMap = _tileWorldInstance.GetNode<TileMapLayer>("TileMap/Layer1");
        }
        else
        {
            GD.PrintErr("CRITICAL: GrassCollector's 'Tile World Instance' is not set in the Inspector.");
            SetProcessInput(false);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector2 worldClickPosition = GetViewport().GetCanvasTransform().AffineInverse() * mouseButton.Position;
            CollectAt(worldClickPosition);
        }
    }

    public void CollectAt(Vector2 worldPosition)
    {
        if (_tileMap == null || _tileWorld == null) return;

        // *** THE DEFINITIVE FIX: DELEGATE COORDINATE CONVERSION ***
        // Ask the TileWorld script to convert the coordinate, using its own reliable logic.
        Vector2I clickedCell = _tileWorld.GetCellFromWorldPosition(worldPosition);
        
        if (_tileMap.GetCellSourceId(clickedCell) != -1)
        {
            List<Vector2I> aoeCells = GetAoeCells(clickedCell);
            ShowAoeRippleEffect(clickedCell, aoeCells);

            List<Vector2I> collectedCells = new List<Vector2I>();
            foreach(var targetCell in aoeCells)
            {
                if (GrassSpawner.GrassPositions.TryGetValue(targetCell, out Grass grass) && IsInstanceValid(grass))
                {
                    grass.QueueFree();
                    collectedCells.Add(targetCell);
                }
            }

            if (collectedCells.Count > 0)
            {
                foreach (var cell in collectedCells)
                {
                    GrassSpawner.GrassPositions.Remove(cell);
                }
                _gameManager?.AddGrass(collectedCells.Count);
            }
        }
    }

    private List<Vector2I> GetAoeCells(Vector2I centerCell)
    {
        var cells = new List<Vector2I>();
        switch (CurrentAoeShape)
        {
            case AoeShape.SingleTile:
                cells.Add(centerCell);
                break;
            case AoeShape.Cross:
                cells.Add(centerCell);
                cells.Add(centerCell + Vector2I.Up);
                cells.Add(centerCell + Vector2I.Down);
                cells.Add(centerCell + Vector2I.Left);
                cells.Add(centerCell + Vector2I.Right);
                break;
            case AoeShape.Square3x3:
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        cells.Add(centerCell + new Vector2I(x, y));
                    }
                }
                break;
        }
        return cells;
    }

    private void ShowAoeRippleEffect(Vector2I centerCell, List<Vector2I> shapeCells)
    {
        _tileWorld?.SetHoverEffectActive(false);
        float maxAnimationDuration = 0;

        foreach(var effectCell in shapeCells)
        {
            if (_tileMap.GetCellSourceId(effectCell) == -1) continue;

            var effectPolygon = new Polygon2D
            {
                Polygon = new Vector2[]
                {
                    new Vector2(0, -8), new Vector2(16, 0),
                    new Vector2(0, 8), new Vector2(-16, 0)
                },
                Color = new Color(0.5f, 1f, 0.5f, 0.7f),
                GlobalPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(effectCell)),
                Scale = Vector2.Zero
            };
            AddChild(effectPolygon);

            int distance = Mathf.Max(Mathf.Abs(effectCell.X - centerCell.X), Mathf.Abs(effectCell.Y - centerCell.Y));
            float delay = distance * _rippleDelayPerTile;
            
            var scaleTween = CreateTween();
            scaleTween.Chain().TweenInterval(delay);
            scaleTween.Chain().TweenProperty(effectPolygon, "scale", Vector2.One * 1.3f, _rippleTotalDuration * 0.4f).SetEase(Tween.EaseType.Out);
            scaleTween.Chain().TweenProperty(effectPolygon, "scale", Vector2.One, _rippleTotalDuration * 0.6f).SetEase(Tween.EaseType.In);
            
            var fadeTween = CreateTween();
            fadeTween.Chain().TweenInterval(delay);
            fadeTween.Chain().TweenProperty(effectPolygon, "modulate:a", 0.0f, _rippleTotalDuration);
            fadeTween.Chain().TweenCallback(Callable.From(effectPolygon.QueueFree));

            if (delay + _rippleTotalDuration > maxAnimationDuration)
            {
                maxAnimationDuration = delay + _rippleTotalDuration;
            }
        }
        
        var timer = GetTree().CreateTimer(maxAnimationDuration);
        timer.Timeout += OnEffectTimerTimeout;
    }
    
    private void OnEffectTimerTimeout()
    {
        _tileWorld?.SetHoverEffectActive(true);
    }
}

