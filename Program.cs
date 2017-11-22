namespace dotproj
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
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
                : Path.GetDirectoryName(args[0]).Split('/', '\\').Last();

            var output = args.Length > 1
                ? args[1]
                : Path.ChangeExtension(args[0], "gv");

            CreateDotFile(output, projects, containerName);
        }

        static Dictionary<string, HashSet<string>> ParseFolder(string path)
            => GetReferences(Directory.EnumerateFiles(path, "*.*proj", SearchOption.AllDirectories));

        static Dictionary<string, HashSet<string>> ParseSolution(string path)
            => GetReferences(GetProjectsInSolution(path));

        static Dictionary<string, HashSet<string>> GetReferences(IEnumerable<string> projects)
            => projects.ToDictionary(
                        project => project,
                        project => new HashSet<string>(ParseProjectReferences(project).
                        Concat(ParsePackageReferencesForProject(project)).
                        Concat(ParseSolutionItemReferences(project))));

        static IEnumerable<string> ParsePackageReferencesForProject(string project)
            => ParsePackageFile(Path.Combine(Path.GetDirectoryName(project), "packages.config"));

        static IEnumerable<string> ParsePackageFile(string path)
            => File.Exists(path)
                ? XDocument.Load(path).Root.Descendants("package").Select(p => $"{p.Attribute("id").Value}.nuget")
                : new List<string>();

        static IEnumerable<string> GetProjectsInSolution(string solutionFile)
            => File.ReadAllLines(solutionFile).
                Where(l => l.StartsWith("Project")).
                Select(l => l.Split('=').Last().Split(',').Skip(1).First().Trim().Trim('"')).
                Where(x => x.EndsWith("proj")).
                Select(p => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionFile), p)).Replace("\\", "/"));

        static IEnumerable<string> ParseProjectReferences(string path)
            => GetProjectReferences(TryGetRootElement(path));

        static IEnumerable<string> ParseSolutionItemReferences(string path)
            => GetSolutionItemReferences(TryGetRootElement(path));

        static XElement TryGetRootElement(string path)
        {
            try
            {
                return XDocument.Load(path).Root;
            }
            catch
            { // vdproj?
                return new XElement("dummy");
            }
        }

        static IEnumerable<string> GetProjectReferences(XElement root)
            => root.
                Descendants(root.GetDefaultNamespace() + "ProjectReference").
                Select(r => r.Attribute("Include").Value);

        static IEnumerable<string> GetSolutionItemReferences(XElement root)
            => root.
                Descendants(root.GetDefaultNamespace() + "SolutionItemReference").
                SelectMany(r => r.Descendants(root.GetDefaultNamespace() + "path")).
                Select(p => p.Value.Split(@"\").Last());

        static void CreateDotFile(string path, Dictionary<string, HashSet<string>> entries, string containerName)
        {
            var packages = entries.SelectMany(e => e.Value.Where(v => v.EndsWith(".nuget"))).Distinct();
            var dot = new StringBuilder();
            dot.AppendLine("digraph {");

            if (packages.Any())
            {
                dot.AppendLine("subgraph cluster_0 { label=\"nuget\"");
                dot.AppendLine(packages.Select(p => p.Remove(p.LastIndexOf('.'))).Select(p => $"\"{p}\" [shape=\"box\"];").Join("\n"));
                dot.AppendLine("}\n");
            }

            dot.AppendLine($"subgraph cluster_1 {{\nlabel=\"{containerName}\";");
            dot.AppendLine(entries.Select(e => CreateDot(e.Key, e.Value)).Join("\n"));
            dot.AppendLine("}}");
            File.WriteAllText(path, dot.ToString());
            dot.Clear();
        }

        static Dictionary<string, Tuple<string, string>> ProjectColors
        = new Dictionary<string, Tuple<string, string>>
        {
                ["csproj" ] = Tuple.Create("#388A34", "white"),
                ["vbproj" ] = Tuple.Create("#00539C", "white"),
                ["fsproj" ] = Tuple.Create("#672878", "white"),
                ["pyproj" ] = Tuple.Create("#879636", "white"),
                ["vcxproj"] = Tuple.Create("#9B4F96", "white"),
                ["vdproj" ] = Tuple.Create("gray",    "white"),
                ["mdproj" ] = Tuple.Create("gray",    "white"),
                [""       ] = Tuple.Create("white",   "black"),
        };

        static Tuple<string, string> GetProjectColors(string type)
            => ProjectColors.ContainsKey(type)
                ? ProjectColors[type]
                : ProjectColors[""];

        static string CreateDot(string key, IEnumerable<string> values)
            => CreateDot(
                key.GetProjectName(),  //.LastIndexOf('.')).Split('/', '\\').Last(),
                GetProjectColors(key.GetProjectType()),
                values);

        static string CreateDot(string name, Tuple<string, string> colors, IEnumerable<string> values)
            => $"\"{name}\" [shape=\"box\",color=\"{colors.Item1}\", style=\"filled\", fillcolor=\"{colors.Item1}\", fontcolor=\"{colors.Item2}\"];\n\"{name}\" -> {{ {values.Select(v => v.GetProjectName().Quote()).Join(", ")} }}";
    }

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
