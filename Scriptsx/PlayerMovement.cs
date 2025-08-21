using Godot;

public partial class PlayerMovement : CharacterBody2D
{
    [Export] private TileMapLayer _tileMap;
    // Changed from AnimationPlayer to AnimatedSprite2D
    [Export] private AnimatedSprite2D _animatedSprite; 
    [Export] private float _moveDuration = 0.4f; 
    [Export] private float _jumpHeight = 30.0f; 
    [Export] private float _moveCooldown = 0.6f;

    private Tween _tween;
    private Timer _moveCooldownTimer;
    private bool _isMoving = false; // State flag to prevent input during movement
    private string _lastIdleAnimation = "idle_front"; // Keep track of the last direction

    public override void _Ready()
    {
        if (_tileMap == null)
        {
            GD.PrintErr("PlayerMovement: The '_tileMap' property is not set.");
            SetProcessInput(false);
            return;
        }
        // Updated null check for the new node type
        if (_animatedSprite == null)
        {
            GD.PrintErr("PlayerMovement: The '_animatedSprite' property is not set.");
        }

        var playerLocalPos = _tileMap.ToLocal(GlobalPosition);
        var startTile = _tileMap.LocalToMap(playerLocalPos);
        GlobalPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(startTile));

        _moveCooldownTimer = new Timer();
        _moveCooldownTimer.WaitTime = _moveCooldown;
        _moveCooldownTimer.OneShot = true;
        AddChild(_moveCooldownTimer);
        
        // Play the default animation using the new node
        _animatedSprite?.Play("idle_front");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            if (_isMoving || !_moveCooldownTimer.IsStopped())
            {
                return;
            }

            var localMousePosition = _tileMap.ToLocal(GetGlobalMousePosition());
            Vector2I targetCell = _tileMap.LocalToMap(localMousePosition);
            
            var playerLocalPos = _tileMap.ToLocal(GlobalPosition);
            Vector2I currentCell = _tileMap.LocalToMap(playerLocalPos);
            
            int chebyshevDistance = Mathf.Max(Mathf.Abs(targetCell.X - currentCell.X), Mathf.Abs(targetCell.Y - currentCell.Y));
            bool isTargetCellEmpty = _tileMap.GetCellTileData(targetCell) == null;

            if (isTargetCellEmpty || chebyshevDistance != 1)
            {
                return;
            }

            Vector2 targetPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(targetCell));
            
            HopTo(targetPosition, targetCell - currentCell);
            
            _moveCooldownTimer.Start();
        }
    }

    private void HopTo(Vector2 targetPosition, Vector2I direction)
    {
        _isMoving = true;
        
        // --- FLIP LOGIC RE-ADDED ---
        // Set the horizontal flip based on the X direction of movement.
        if (direction.X != 0)
        {
            _animatedSprite.FlipH = direction.X < 0; // true for left, false for right
        }
        else
        {
            // Ensure the sprite is not flipped for vertical movement (front/back).
            _animatedSprite.FlipH = false;
        }

        string jumpAnimation = GetJumpAnimationName(direction);
        // Play animation on the AnimatedSprite2D
        _animatedSprite?.Play(jumpAnimation);

        if (_tween != null && _tween.IsValid())
        {
            _tween.Kill();
        }
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
        // Play the correct idle animation. The sprite will remain flipped if the last move was left.
        _animatedSprite?.Play(_lastIdleAnimation);
        _isMoving = false; // Allow movement again
    }

    private string GetJumpAnimationName(Vector2I direction)
    {
        string animName = "jump_front"; // Default value
        _lastIdleAnimation = "idle_front"; // Default idle

        // --- FIX: Use an if/else if structure to prevent logic override ---
        // Logic for determining which animation to play based on direction.
        if (direction.Y < 0) 
        { 
            animName = "jump_back"; 
            _lastIdleAnimation = "idle_back"; 
        }
        else if (direction.Y > 0) 
        { 
            animName = "jump_front"; 
            _lastIdleAnimation = "idle_front"; 
        }
        // This will only be checked if the Y direction is 0.
        else if (direction.X != 0) 
        { 
            animName = "jump_front"; 
            _lastIdleAnimation = "idle_front"; 
        }

        // Check if the determined animation exists, otherwise fall back to the default.
        if (_animatedSprite.SpriteFrames.HasAnimation(animName))
        {
            return animName;
        }

        // If the specific animation doesn't exist, fall back to the default "jump_front"
        _lastIdleAnimation = "idle_front";
        return "jump_front";
    }
}
