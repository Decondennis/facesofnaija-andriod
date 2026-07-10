using System;
using System.IO;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: Decompiler <assemblyPath>");
            return 1;
        }
        var assembly = args[0];
        if (!File.Exists(assembly))
        {
            Console.Error.WriteLine($"File not found: {assembly}");
            return 2;
        }
        try
        {
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(assembly, settings);
            var code = decompiler.DecompileWholeModuleAsString();
            Console.WriteLine(code);
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            return 3;
        }
    }
}