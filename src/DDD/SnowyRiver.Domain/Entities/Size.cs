namespace SnowyRiver.Domain.Entities;

public record Size : Size<int>
{
    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public Size()
    {
    }
}

public record Size<T> where T : struct
{
    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    public Size()
    {
    }

    public T Width { get; set; }

    public T Height { get; set; }
}
