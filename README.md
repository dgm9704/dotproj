# Introduction
dotproj is a project for creating [graphviz](http://www.graphviz.org/) 
[dot](http://www.graphviz.org/doc/info/lang.html) files from .NET solution/project references 

## Environment
I use [Visual Studio Code](https://code.visualstudio.com/) 
on [Arch Linux](https://www.archlinux.org/)

Code is written in [C#](https://docs.microsoft.com/en-us/dotnet/csharp/index) 7.0, targeting 
[.NET Standard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md) / 
[.NET Core 2.0](https://docs.microsoft.com/en-us/dotnet/core/)

I use [WebGraphviz](http://webgraphviz.com/) a lot for testing the output

## License:
GNU Lesser General Public License v3.0

[http://www.gnu.org/licenses/gpl.txt](http://www.gnu.org/licenses/gpl.txt)

[http://www.gnu.org/licenses/lgpl.txt](http://www.gnu.org/licenses/lgpl.txt)

# reference solution
there is also a reference solution with different project types used for testing

### License
[Free Public License 1.0.0](https://opensource.org/licenses/FPL-1.0.0)

at the moment the output from reference solution looks something like this:
```
digraph {
subgraph cluster_1 {
label="reference";
"fsharp" [shape="box",color="#672878", style="filled", fillcolor="#672878", fontcolor="white"];
"fsharp" -> {  }
"il" [shape="box",color="black", style="filled", fillcolor="black", fontcolor="white"];
"il" -> {  }
"vbnet" [shape="box",color="#00539C", style="filled", fillcolor="#00539C", fontcolor="white"];
"vbnet" -> { "csharp" "il" }
"packaging" [shape="box",color="gray", style="filled", fillcolor="gray", fontcolor="white"];
"packaging" -> { "csharp" "fsharp" "il" "vbnet" }
"csharp" [shape="box",color="#388A34", style="filled", fillcolor="#388A34", fontcolor="white"];
"csharp" -> {  }
}}

```

