using Godot;
using System.Linq;

public partial class TileWorld : Node2D
{
    [Export] private TileMapLayer _layer0;
    [Export] private TileMapLayer _layer1;

    private Polygon2D _hoverEffect;
    private Vector2I? _lastHoveredTile = null;

    // A flag to control the hover effect's visibility from other scripts.
    private bool _isHoverEffectActive = true;

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

    // *** NEW PUBLIC METHOD ***
    // This allows any other script to reliably get a tile coordinate from a world position.
    public Vector2I GetCellFromWorldPosition(Vector2 worldPosition)
    {
        // Convert the global world position to a position local to our TileMapLayer.
        Vector2 localPos = _layer0.ToLocal(worldPosition);
        // Convert that local position into a tile cell coordinate.
        return _layer0.LocalToMap(localPos);
    }

    public void SetHoverEffectActive(bool isActive)
    {
        _isHoverEffectActive = isActive;
        if (!isActive)
        {
            _hoverEffect.Visible = false;
        }
    }

    private void SetupHoverPolygon()
    {
        _hoverEffect = new Polygon2D
        {
            Polygon = new Vector2[]
            {
                new Vector2(0, -8),   // Top
                new Vector2(16, 0),   // Right
                new Vector2(0, 8),    // Bottom
                new Vector2(-16, 0)   // Left
            },
            Color = new Color(1, 1, 1, 0.2f),
            Visible = false,
            ZIndex = 1
        };
        AddChild(_hoverEffect);
    }

    private void HandleHoverEffect()
    {
        // Don't show the hover effect if it's been temporarily disabled.
        if (!_isHoverEffectActive)
        {
            _hoverEffect.Visible = false;
            return;
        }

        var mousePos = GetViewport().GetMousePosition();
        Vector2 worldPos = GetViewport().GetCanvasTransform().AffineInverse() * mousePos;
        var currentTile = GetCellFromWorldPosition(worldPos);

        if (_lastHoveredTile.HasValue && _lastHoveredTile.Value == currentTile)
        {
            return;
        }
        _lastHoveredTile = currentTile;

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

