namespace dotproj
{
    using System.IO;
    using System.Linq;
    using Diwen.DotProj;

    public class Program
    {
        public static void Main(string[] args)
        {
            var isSolution = Path.HasExtension(args[0]);
            var projects = isSolution
                ? Parse.ParseSolution(args[0])
                : Parse.ParseFolder(args[0]);

            var containerName = isSolution
                ? Path.GetFileNameWithoutExtension(args[0])
                : Path.GetDirectoryName(args[0]).Split('/', '\\').Last();

            var output = args.Length > 1
                ? args[1]
                : Path.ChangeExtension(args[0], "gv");

            Create.CreateDotFile(output, projects, containerName);
        }
    }
}
