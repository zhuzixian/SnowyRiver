namespace SnowyRiver.Commons.Extensions;

public static class StringExtensions
{
    extension(string value)
    {
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
