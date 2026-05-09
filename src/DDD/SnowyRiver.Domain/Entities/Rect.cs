namespace SnowyRiver.Domain.Entities;

public record Rect : Rect<int>
{
    public Rect()
    {
    }

    public Rect(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }
}

public record Rect<T>:Size<T> where T : struct
{
    public Rect()
    {

    }

    public Rect(T x, T y, T width, T height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public T X { get; set; }

    public T Y { get; set; }

}
