namespace RuneAndRust.Domain.ValueObjects;

public readonly record struct Position(int X, int Y)
{
    public static Position Origin => new(0, 0);

    public Position Move(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);

    public override string ToString() => $"({X}, {Y})";
}
