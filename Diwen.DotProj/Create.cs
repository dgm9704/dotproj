namespace Diwen.DotProj
{
    class Create
    {
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
                ["ilproj" ] = Tuple.Create("black",   "white"),
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
            => $"\"{name}\" [shape=\"box\",color=\"{colors.Item1}\", style=\"filled\", fillcolor=\"{colors.Item1}\", fontcolor=\"{colors.Item2}\"];\n\"{name}\" -> {{ {values.Select(v => v.GetProjectName().Quote()).Join(" ")} }}";

    }
}