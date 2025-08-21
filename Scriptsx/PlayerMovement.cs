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

    public override void _Ready()
    {
        if (_tileMap == null)
        {
            GD.PrintErr("PlayerMovement: The '_tileMap' property is not set.");
            SetProcessInput(false);
            return;
        }
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

            if (chebyshevDistance != 1) return; // Exit if not an adjacent tile

            // --- NEW COLLECTION LOGIC ---
            // Check if there is grass on the target tile.
            if (GrassSpawner.GrassPositions.TryGetValue(targetCell, out Grass grass))
            {
                // Collect the grass
                grass.QueueFree(); // Remove the grass from the scene
                GrassSpawner.GrassPositions.Remove(targetCell); // Remove it from the dictionary
                GameUI.GrassCount++; // Increment the UI counter
                
                // Optional: Play a collection sound or animation here
                
                _moveCooldownTimer.Start(); // Start cooldown even after collecting
                return; // IMPORTANT: Do not proceed to the movement logic
            }

            // --- MOVEMENT LOGIC (Only runs if no grass was found) ---
            bool isTargetCellEmpty = _tileMap.GetCellTileData(targetCell) == null;
            if (isTargetCellEmpty)
            {
                return; // Clicked on an empty tile with no grass
            }

            Vector2 targetPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(targetCell));
            HopTo(targetPosition, targetCell - currentCell);
            _moveCooldownTimer.Start();
        }
    }

    private void HopTo(Vector2 targetPosition, Vector2I direction)
    {
        _isMoving = true;
        
        if (direction.X != 0)
        {
            _animatedSprite.FlipH = direction.X < 0; 
        }
        else
        {
            _animatedSprite.FlipH = false;
        }

        string jumpAnimation = GetJumpAnimationName(direction);
        _animatedSprite?.Play(jumpAnimation);

        if (_tween != null && _tween.IsValid())
        {
            _tween.Finished -= OnHopFinished;
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
        _animatedSprite?.Play(_lastIdleAnimation);
        _isMoving = false; 
    }

    private string GetJumpAnimationName(Vector2I direction)
    {
        string animName = "jump_front"; 
        _lastIdleAnimation = "idle_front"; 

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
        else if (direction.X != 0) 
        { 
            animName = "jump_front"; 
            _lastIdleAnimation = "idle_front"; 
        }

        if (_animatedSprite.SpriteFrames.HasAnimation(animName))
        {
            return animName;
        }

        _lastIdleAnimation = "idle_front";
        return "jump_front";
    }
}
