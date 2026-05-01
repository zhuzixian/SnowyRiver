namespace SnowyRiver.Domain.Entities;

public record Rect : Rect<int>;

public record Rect<T> where T : struct
{
    public T X { get; set; }

    public T Y { get; set; }

    public T Width { get; set; }

    public T Height { get; set; }
}
