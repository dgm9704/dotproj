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
        {
            var isSolution = Path.HasExtension(args[0]);
            var projects = isSolution
                ? ParseSolution(args[0])
                : ParseFolder(args[0]);

            var containerName = isSolution
                ? Path.GetFileNameWithoutExtension(args[0])
                : Path.GetDirectoryName(args[0]);

            CreateDotFile(args[1], projects, containerName);
        }

        static Dictionary<string, HashSet<string>> ParseFolder(string path)
        {
            return Directory.EnumerateFiles(path, "*.*proj", SearchOption.AllDirectories).
                ToDictionary(
                    project => project,
                    project => new HashSet<string>(ParseProjectReferences(project)));
        }

        static Dictionary<string, HashSet<string>> ParseSolution(string path)
        {
            return GetProjectsInSolution(path).
                ToDictionary(
                    project => project,
                    project => new HashSet<string>(ParseProjectReferences(project)));
        }

        static IEnumerable<string> GetProjectsInSolution(string solutionFile)
        {
            return File.ReadAllLines(solutionFile).
                Where(l => l.StartsWith("Project")).
                Select(l => l.
                    Split('=').Last().
                    Split(',').Skip(1).First().
                    Trim().Trim('"')).
                Select(p => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionFile), p)).Replace("\\", "/"));
        }

        static IEnumerable<string> ParseProjectReferences(string path)
        {
            // return XDocument.Load(path).Root.
            //         Descendants("ProjectReference").
            //         Select(r => r.
            //             Attribute("Include").Value);
            var doc = XDocument.Load(path);
            var root = doc.Root;
            var ns = root.GetDefaultNamespace();
            return root.
                Descendants(ns + "ProjectReference").
                Select(r => r.Attribute("Include").Value);
        }

        static void CreateDotFile(string path, Dictionary<string, HashSet<string>> entries, string containerName)
        {
            File.WriteAllText(path, "digraph g {\n");
            File.AppendAllText(path, $"subgraph {containerName} {{\n");
            File.AppendAllLines(path, entries.Select(e => CreateDot(e.Key, e.Value)));
            File.AppendAllText(path, "}}");
        }

        static string CreateDot(string key, IEnumerable<string> values)
        {
            key = key.Split('/', '\\').Last();
            var idx = key.LastIndexOf('.');
            var l = key.Substring(idx + 1);
            var name = key.Remove(idx).Quote();
            var bgcolor = "white";
            var fontcolor = "black";

            switch (l)
            {
                case "csproj":
                    bgcolor = "#388A34";
                    fontcolor = "white";
                    break;
                case "vbproj":
                    bgcolor = "#00539C";
                    fontcolor = "white";
                    break;
                case "fsproj":
                    bgcolor = "#672878";
                    fontcolor = "white";
                    break;
            }

            return $"{name} [shape=\"box\",color=\"{bgcolor}\", style=\"filled\", fillcolor=\"{bgcolor}\", fontcolor=\"{fontcolor}\"];\n{name} -> {{ {values.Select(v => v.GetProjectName().Quote()).Join(", ")} }}";
        }
    }

    internal static class StringExtensions
    {
        public static string GetProjectName(this string Value)
            => Value.Substring(0, Value.LastIndexOf('.')).Split('\\', '/').Last();

        public static string Join(this IEnumerable<string> values, string separator)
            => string.Join(separator, values);

        public static string Quote(this string value)
            => $"\"{value}\"";
    }



}
