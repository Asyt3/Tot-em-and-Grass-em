using Godot;
using System.Linq;

public partial class TileWorld : Node2D
{
    [Export] private TileMapLayer _layer0;
    [Export] private TileMapLayer _layer1;

    private Polygon2D _hoverEffect;
    // Variable to store the last hovered tile position
    private Vector2I? _lastHoveredTile = null;

    public override void _Ready()
    {
        if (_layer0 == null)
        {
            GD.PrintErr("Tilemaps: The '_layer0' property is not set. Please assign a TileMapLayer node in the Inspector.");
            SetProcess(false);
            return;
        }
        SetupHoverPolygon();
    }

    public override void _Process(double delta)
    {
        HandleHoverEffect();
    }

    private void SetupHoverPolygon()
    {
        _hoverEffect = new Polygon2D();
        _hoverEffect.Polygon = new Vector2[]
        {
            new Vector2(0, -8),   // Top
            new Vector2(16, 0),   // Right
            new Vector2(0, 8),    // Bottom
            new Vector2(-16, 0)   // Left
        };
        _hoverEffect.Color = new Color(1, 1, 1, 0.2f);
        _hoverEffect.Visible = false;
        // Ensure the hover effect draws on top of the tiles
        _hoverEffect.ZIndex = 1; 
        AddChild(_hoverEffect);
    }

    private void HandleHoverEffect()
    {
        var mousePos = GetLocalMousePosition();
        var currentTile = _layer0.LocalToMap(mousePos);

        // Only update if the hovered tile has changed
        if (_lastHoveredTile.HasValue && _lastHoveredTile.Value == currentTile)
        {
            return;
        }

        _lastHoveredTile = currentTile;

        // Check if there is a tile at the given coordinates.
        // GetCellSourceId returns -1 if the cell is empty.
        if (_layer0.GetCellSourceId(currentTile) != -1)
        {
            _hoverEffect.Visible = true;
            _hoverEffect.Position = _layer0.MapToLocal(currentTile);
        }
        else
        {
            _hoverEffect.Visible = false;
        }
    }
}
