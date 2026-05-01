namespace SnowyRiver.Domain.Entities;

public record Rect : Rect<int>;

public record Rect<T>:Size<T> where T : struct
{
    public T X { get; set; }

    public T Y { get; set; }

}
