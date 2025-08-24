using Godot;

public partial class PlayerMovement : CharacterBody2D
{
    [Export] private TileMapLayer _tileMap;
    [Export] private AnimatedSprite2D _animatedSprite;
    [Export] private float _moveDuration = 0.4f;
    [Export] private float _jumpHeight = 30.0f;
    [Export] private float _moveCooldown = 0.6f;

    private Tween _tween;
    private Timer _moveCooldownTimer;
    private bool _isMoving = false;
    private string _lastIdleAnimation = "idle_front";
    private Vector2I _targetCell;

    public Vector2I CurrentCell { get; private set; }

    public override void _Ready()
    {
        if (_tileMap == null)
        {
            GD.PrintErr("PlayerMovement: The '_tileMap' property is not set.");
            SetProcess(false);
            return;
        }
        
        if (_animatedSprite == null)
        {
            GD.PrintErr("PlayerMovement: The '_animatedSprite' property is not set.");
        }

        var playerLocalPos = _tileMap.ToLocal(GlobalPosition);
        CurrentCell = _tileMap.LocalToMap(playerLocalPos);
        GlobalPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(CurrentCell));
        GD.Print($"Player starting at cell: {CurrentCell}");

        _moveCooldownTimer = new Timer();
        _moveCooldownTimer.WaitTime = _moveCooldown;
        _moveCooldownTimer.OneShot = true;
        AddChild(_moveCooldownTimer);
        
        _animatedSprite?.Play("idle_front");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            if (_isMoving || !_moveCooldownTimer.IsStopped()) return;

            Vector2I targetCell = _tileMap.LocalToMap(_tileMap.ToLocal(GetGlobalMousePosition()));

            int chebyshevDistance = Mathf.Max(Mathf.Abs(targetCell.X - CurrentCell.X), Mathf.Abs(targetCell.Y - CurrentCell.Y));
            if (chebyshevDistance != 1) return; // Not adjacent, do nothing.

            // Action 1: Check for grass to collect.
            if (GrassSpawner.GrassPositions.TryGetValue(targetCell, out Grass grassNode) && IsInstanceValid(grassNode))
            {
                GD.Print($"ACTION: Collect grass at {targetCell}.");
                GrassSpawner.GrassPositions.Remove(targetCell);
                grassNode.QueueFree();
                _moveCooldownTimer.Start();
            }
            // Action 2: If no grass, check for valid ground to move to.
            else if (_tileMap.GetCellTileData(targetCell) != null)
            {
                GD.Print($"ACTION: Move to empty tile {targetCell}.");
                _targetCell = targetCell;
                Vector2 targetPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(targetCell));
                HopTo(targetPosition, targetCell - CurrentCell);
                _moveCooldownTimer.Start();
            }
        }
    }

    /// <summary>
    /// Handles the entire hop animation, including direction, sprite flipping, and tweening.
    /// </summary>
    private void HopTo(Vector2 targetPosition, Vector2I direction)
    {
        _isMoving = true;

        string jumpAnimation;

        if (direction.Y < 0) // Moving Up
        {
            jumpAnimation = "jump_back";
            _lastIdleAnimation = "idle_back";
        }
        else if (direction.Y > 0) // Moving Down
        {
            jumpAnimation = "jump_front";
            _lastIdleAnimation = "idle_front";
        }
        else // Moving Sideways (direction.X will be non-zero)
        {
            jumpAnimation = "jump_side";
            _lastIdleAnimation = "idle_side";
            _animatedSprite.FlipH = direction.X < 0;
        }

        if (_animatedSprite != null && _animatedSprite.SpriteFrames.HasAnimation(jumpAnimation))
        {
            _animatedSprite.Play(jumpAnimation);
        }
        else
        {
            _animatedSprite?.Play("jump_front");
            _lastIdleAnimation = "idle_front";
        }

        if (_tween != null && _tween.IsValid()) _tween.Kill();
        _tween = GetTree().CreateTween();
        
        _tween.Finished += OnHopFinished;

        Vector2 startPosition = GlobalPosition;
        _tween.TweenMethod(
            Callable.From<float>((progress) =>
            {
                Vector2 newPos = startPosition.Lerp(targetPosition, progress);
                float arc = 4 * _jumpHeight * (progress - (progress * progress));
                newPos.Y -= arc;
                GlobalPosition = newPos;
            }),
            0.0f, 1.0f, _moveDuration
        ).SetTrans(Tween.TransitionType.Sine);
    }

    private void OnHopFinished()
    {
        _isMoving = false;
        _animatedSprite?.Play(_lastIdleAnimation);
        
        Vector2I oldCell = CurrentCell;
        CurrentCell = _targetCell;
        GD.Print($"Hop finished. Player cell updated from {oldCell} to {CurrentCell}");
    }
}
