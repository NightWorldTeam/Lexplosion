namespace Lexplosion.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) => input switch
        {
            null => input,
            "" => input,
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };
    }
}
