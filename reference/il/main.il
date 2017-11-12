.assembly HelloWorld
{
}

.assembly extern mscorlib { }

.namespace DefaultNamespace
{
	.class private auto ansi beforefieldinit MainClass
	{
		.method public hidebysig static void Main(string[] args) cil managed
		{
			.entrypoint
			.maxstack  1
			ldstr "Hello World!"
			call void [mscorlib]System.Console::WriteLine(string)
			ret
		}
	}
}
