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
                    project => project.GetProjectName(),
                    project => new HashSet<string>(ParseProjectReferences(project)));
        }

        static IEnumerable<string> ParseProjectReferences(string path)
        {
            return XDocument.Load(path).
                    Elements("ProjectReference").
                    Select(r => r.Attribute("Include").Value.GetProjectName());
        }

        static void CreateDotFile(Dictionary<string, HashSet<string>> entries, string path)
        {
            File.WriteAllLines(path, entries.Keys);
        }
    }

    internal static class StringExtensions
    {
        public static string GetProjectName(this string Value)
            => Path.GetFileNameWithoutExtension(Value);
    }
}
