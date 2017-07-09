namespace dotproj
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

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
            => GetReferences(Directory.EnumerateFiles(path, "*.*proj", SearchOption.AllDirectories));

        static Dictionary<string, HashSet<string>> ParseSolution(string path)
            => GetReferences(GetProjectsInSolution(path));

        static Dictionary<string, HashSet<string>> GetReferences(IEnumerable<string> projects)
            => projects.ToDictionary(
                        project => project,
                        project => new HashSet<string>(ParseProjectReferences(project).Concat(ParsePackageReferences(project))));

        private static IEnumerable<string> ParsePackageReferences(string project)
        {
            var result = new List<string>();
            var packageFile = Path.Combine(Path.GetDirectoryName(project), "packages.config");
            if (File.Exists(packageFile))
            {
                var doc = XDocument.Load(packageFile);
                var elements = doc.Root.Descendants("package");
                result = elements.Select(p => $"{p.Attribute("id").Value}.nuget").ToList();
            }
            return result;
        }

        static IEnumerable<string> GetProjectsInSolution(string solutionFile)
            => File.ReadAllLines(solutionFile).
                Where(l => l.StartsWith("Project")).
                Select(l => l.
                    Split('=').Last().
                    Split(',').Skip(1).First().
                    Trim().Trim('"')).
                Select(p => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionFile), p)).Replace("\\", "/"));


        static IEnumerable<string> ParseProjectReferences(string path)
            => GetProjectReferences(XDocument.Load(path).Root);

        static IEnumerable<string> GetProjectReferences(XElement root)
            => root.
                Descendants(root.GetDefaultNamespace() + "ProjectReference").
                Select(r => r.Attribute("Include").Value);

        static void CreateDotFile(string path, Dictionary<string, HashSet<string>> entries, string containerName)
        {
            var packages = entries.SelectMany(e => e.Value.Where(v => v.EndsWith(".nuget"))).Distinct();
            File.WriteAllText(path, "digraph {\n");
            if (packages.Any())
            {
                File.AppendAllText(path, "subgraph cluster_0 { label=\"nuget\"\n ");
                File.AppendAllLines(path, packages.Select(p => p.Remove(p.LastIndexOf('.'))).Select(p => $"\"{p}\" [shape=\"box\"];"));
                File.AppendAllText(path, "}\n");
            }

            File.AppendAllText(path, $"subgraph cluster_1 {{\nlabel=\"{containerName}\"");
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
