namespace SnowyRiver.Domain.Entities;

public record Size : Size<int>;

public record Size<T> where T : struct
{
    public T Width { get; set; }

    public T Height { get; set; }
}
