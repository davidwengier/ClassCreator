using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClassCreator
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Option countOption = new Option("--count", "The number of classes to create", new Argument<int>(1));
            countOption.AddAlias("-c");

            Option outputOption = new Option("--output", "Where to output the files to", new Argument<string>(Environment.CurrentDirectory));
            outputOption.AddAlias("-o");

            Option namespaceOption = new Option("--namespace", "The namespace to use for the class file", new Argument<string>("Classes"));
            namespaceOption.AddAlias("-n");

            var command = new RootCommand();
            command.Description = "Creates a large number of C# classes";
            command.AddOption(countOption);
            command.AddOption(outputOption);
            command.AddOption(namespaceOption);

            command.Handler = CommandHandler.Create<int, string, string>((c, o, n) => CreateClasses(c, o, n));

            return await command.InvokeAsync(args);
        }

        private static void CreateClasses(int count, string output, string nameSpace)
        {
            Console.WriteLine($"Creating {count} class{(count == 1 ? "" : "es")} in " + output);

            int start = (from file in Directory.EnumerateFiles(output, "Class*.cs")
                         let num = ParseFileName(file)
                         where num != null
                         select num.Value).DefaultIfEmpty(0).Max() + 1;

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = start; i < (start + count); i++)
            {
                CreateClass(output, nameSpace, i);
            }
            Console.WriteLine($"Created Class{start}.cs through Class{start + count - 1}.cs {sw.Elapsed}.");
        }

        private static void CreateClass(string outputDir, string nameSpace, int num)
        {
            File.WriteAllText(Path.Combine(outputDir, $"Class{num}.cs"), $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace {nameSpace}
{{
    class Class{num}
    {{
    }}
}}
");
        }

        private static int? ParseFileName(string fileName)
        {
            if (fileName == null) return null;
            var span = fileName.AsSpan();
            span = Path.GetFileName(span);
            int dot = span.LastIndexOf(".");
            if (dot > -1 && int.TryParse(span.Slice(5, dot - 5), out var result))
            {
                return result;
            }
            return null;
        }
    }
}
