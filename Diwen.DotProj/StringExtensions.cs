namespace Diwen.DotProj.Extensions
{
    internal static class StringExtensions
    {
        public static string GetProjectName(this string Value)
            => Value.Substring(0, Value.LastIndexOf('.')).Split('\\', '/').Last();

        public static string GetProjectType(this string value)
            => value.Split('.').Last();

        public static string Join(this IEnumerable<string> values, string separator)
            => string.Join(separator, values);

        public static string Quote(this string value)
            => $"\"{value}\"";
    }
}