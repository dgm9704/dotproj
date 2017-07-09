using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace dotproj
{
    class Program
    {
        static void Main(string[] args)
            => CreateDotFile(ParseFolder(args[0]), args[1]);

        static Dictionary<string, HashSet<string>> ParseFolder(string path)
        {
            return Directory.EnumerateFiles(path, "*.*proj", SearchOption.AllDirectories).
                ToDictionary(
                    project => project,
                    project => new HashSet<string>(ParseProjectReferences(project)));
        }

        static IEnumerable<string> ParseProjectReferences(string path)
        {
            return XDocument.Load(path).Root.
                    Descendants("ProjectReference").
                    Select(r => r.
                        Attribute("Include").Value);
        }

        static void CreateDotFile(Dictionary<string, HashSet<string>> entries, string path)
        {
            File.WriteAllText(path, "digraph g {\n");
            File.AppendAllLines(path, entries.Select(e => CreateDot(e.Key, e.Value)));
            File.AppendAllText(path, "}");
        }

        static string CreateDot(string key, IEnumerable<string> values)
        {
            return $"{key.GetProjectName().Quote()} -> {{ {values.Select(v => v.GetProjectName().Quote()).Join(", ")} }}";
        }
    }

    internal static class StringExtensions
    {
        public static string GetProjectName(this string Value)
            => Value.Split('\\', '/').Last();

        public static string Join(this IEnumerable<string> values, string separator)
            => string.Join(separator, values);

        public static string Quote(this string value)
            => $"\"{value}\"";
    }



}
