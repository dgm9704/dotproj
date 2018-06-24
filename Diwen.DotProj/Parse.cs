namespace Diwen.DotProj
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    
    class Parsing
    {
        static Dictionary<string, HashSet<string>> ParseFolder(string path)
            => GetReferences(Directory.EnumerateFiles(path, "*.*proj", SearchOption.AllDirectories));

        static Dictionary<string, HashSet<string>> ParseSolution(string path)
            => GetReferences(GetProjectsInSolution(path));

        static Dictionary<string, HashSet<string>> GetReferences(IEnumerable<string> projects)
            => projects.ToDictionary(
                        project => project,
                        project => new HashSet<string>(ParseProjectReferences(project).
                        //Concat(ParsePackageReferencesForProject(project)).
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

    }
}