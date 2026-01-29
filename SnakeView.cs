using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;

namespace SnakeAndroid;

public sealed class SnakeView : View
{
    private const int Columns = 18;
    private const int Rows = 24;
    private const long BaseTickMs = 150;
    private const string PrefsName = "snake_prefs";
    private const string KeyHighScore = "high_score";

    private readonly Handler _handler;
    private readonly Random _random = new();
    private readonly LinkedList<GridPoint> _snake = new();
    private readonly HashSet<GridPoint> _occupied = new();

    private readonly Paint _boardPaint = new() { Color = Color.ParseColor("#111827") };
    private readonly Paint _gridPaint = new() { Color = Color.ParseColor("#1F2937"), StrokeWidth = 2f };
    private readonly Paint _headPaint = new() { Color = Color.ParseColor("#4ADE80") };
    private readonly Paint _bodyPaint = new() { Color = Color.ParseColor("#22C55E") };
    private readonly Paint _foodPaint = new() { Color = Color.ParseColor("#EF4444") };
    private readonly Paint _textPaint = new() { Color = Color.White, TextSize = 44f, AntiAlias = true };
    private readonly Paint _overlayPaint = new() { Color = Color.Argb(170, 0, 0, 0) };

    private Direction _direction = Direction.Right;
    private Direction _pendingDirection = Direction.Right;
    private GridPoint _food;
    private int _score;
    private int _highScore;
    private bool _isGameOver;
    private float _touchStartX;
    private float _touchStartY;

    public SnakeView(Context context) : base(context)
    {
        _handler = new Handler(Looper.MainLooper!);
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        _highScore = prefs?.GetInt(KeyHighScore, 0) ?? 0;
        ResetGame();
    }

    public void StartLoop()
    {
        _handler.RemoveCallbacks(Tick);
        _handler.PostDelayed(Tick, CurrentTickDelay());
    }

    public void StopLoop()
    {
        _handler.RemoveCallbacks(Tick);
    }

    public void ChangeDirection(Direction next)
    {
        if (_isGameOver) return;

        bool isOpposite = (_direction, next) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            _ => false
        };

        if (!isOpposite)
        {
            _pendingDirection = next;
        }
    }

    private void ResetGame()
    {
        _snake.Clear();
        _occupied.Clear();

        int startX = Columns / 2;
        int startY = Rows / 2;
        for (int i = 0; i < 3; i++)
        {
            var pt = new GridPoint(startX - i, startY);
            _snake.AddLast(pt);
            _occupied.Add(pt);
        }

        _direction = Direction.Right;
        _pendingDirection = Direction.Right;
        _score = 0;
        _isGameOver = false;
        SpawnFood();
        Invalidate();
    }

    private long CurrentTickDelay()
    {
        long reduction = (_score / 50) * 10L;
        return Math.Max(75L, BaseTickMs - reduction);
    }

    private void Tick()
    {
        if (_isGameOver) return;

        _direction = _pendingDirection;
        var head = _snake.First!.Value;
        var next = _direction switch
        {
            Direction.Up => new GridPoint(head.X, head.Y - 1),
            Direction.Down => new GridPoint(head.X, head.Y + 1),
            Direction.Left => new GridPoint(head.X - 1, head.Y),
            Direction.Right => new GridPoint(head.X + 1, head.Y),
            _ => head
        };

        if (next.X < 0 || next.X >= Columns || next.Y < 0 || next.Y >= Rows)
        {
            EndGame();
            return;
        }

        bool ateFood = next == _food;
        if (!ateFood)
        {
            var tail = _snake.Last!.Value;
            _snake.RemoveLast();
            _occupied.Remove(tail);
        }

        if (_occupied.Contains(next))
        {
            EndGame();
            return;
        }

        _snake.AddFirst(next);
        _occupied.Add(next);

        if (ateFood)
        {
            _score += 10;
            if (_score > _highScore)
            {
                _highScore = _score;
                Context?.GetSharedPreferences(PrefsName, FileCreationMode.Private)?
                    .Edit()?
                    .PutInt(KeyHighScore, _highScore)?
                    .Apply();
            }
            SpawnFood();
        }

        Invalidate();
        _handler.PostDelayed(Tick, CurrentTickDelay());
    }

    private void EndGame()
    {
        _isGameOver = true;
        Invalidate();
    }

    private void SpawnFood()
    {
        while (true)
        {
            var candidate = new GridPoint(_random.Next(Columns), _random.Next(Rows));
            if (!_occupied.Contains(candidate))
            {
                _food = candidate;
                return;
            }
        }
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e is null) return base.OnTouchEvent(e);

        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:
                if (_isGameOver)
                {
                    ResetGame();
                    StartLoop();
                    return true;
                }
                _touchStartX = e.GetX();
                _touchStartY = e.GetY();
                return true;

            case MotionEventActions.Up:
                float dx = e.GetX() - _touchStartX;
                float dy = e.GetY() - _touchStartY;
                if (Math.Abs(dx) < 30f && Math.Abs(dy) < 30f) return true;

                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    ChangeDirection(dx > 0 ? Direction.Right : Direction.Left);
                }
                else
                {
                    ChangeDirection(dy > 0 ? Direction.Down : Direction.Up);
                }
                return true;
        }

        return base.OnTouchEvent(e);
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        canvas.DrawRect(0, 0, Width, Height, _boardPaint);

        float headerHeight = 110f;
        float availableWidth = Width;
        float availableHeight = Height - headerHeight;
        float cellSize = Math.Min(availableWidth / Columns, availableHeight / Rows);
        float offsetX = (Width - cellSize * Columns) / 2f;
        float offsetY = headerHeight + (availableHeight - cellSize * Rows) / 2f;

        _textPaint.TextAlign = Paint.Align.Left;
        canvas.DrawText($"Skor: {_score}   Terbaik: {_highScore}", 36f, 70f, _textPaint);

        for (int x = 0; x <= Columns; x++)
        {
            float px = offsetX + x * cellSize;
            canvas.DrawLine(px, offsetY, px, offsetY + Rows * cellSize, _gridPaint);
        }
        for (int y = 0; y <= Rows; y++)
        {
            float py = offsetY + y * cellSize;
            canvas.DrawLine(offsetX, py, offsetX + Columns * cellSize, py, _gridPaint);
        }

        DrawCell(canvas, _food, cellSize, offsetX, offsetY, _foodPaint);

        bool isHead = true;
        foreach (var part in _snake)
        {
            DrawCell(canvas, part, cellSize, offsetX, offsetY, isHead ? _headPaint : _bodyPaint);
            isHead = false;
        }

        if (_isGameOver)
        {
            canvas.DrawRect(0, 0, Width, Height, _overlayPaint);
            _textPaint.TextAlign = Paint.Align.Center;
            canvas.DrawText("Game Over", Width / 2f, Height / 2f - 30f, _textPaint);
            canvas.DrawText($"Skor: {_score} - Ketuk untuk main lagi", Width / 2f, Height / 2f + 35f, _textPaint);
        }
    }

    private static void DrawCell(Canvas canvas, GridPoint pt, float size, float offsetX, float offsetY, Paint paint)
    {
        float pad = 2f;
        float left = offsetX + pt.X * size + pad;
        float top = offsetY + pt.Y * size + pad;
        float right = offsetX + (pt.X + 1) * size - pad;
        float bottom = offsetY + (pt.Y + 1) * size - pad;
        canvas.DrawRoundRect(left, top, right, bottom, 8f, 8f, paint);
    }
}
