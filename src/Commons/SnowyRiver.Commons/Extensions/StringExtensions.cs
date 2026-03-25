namespace SnowyRiver.Commons.Extensions;

public static class StringExtensions
{
    extension(string? value)
    {
        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(value);
        }


        public bool IsNotNullOrWhiteSpace()
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public bool IsNotNullOrEmpty()
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
