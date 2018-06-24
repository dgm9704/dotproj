namespace dotproj
{
    using Diwen.DotProj;

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
    }
}
