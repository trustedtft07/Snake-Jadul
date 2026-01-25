namespace SnakeAndroid;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public readonly record struct GridPoint(int X, int Y);
