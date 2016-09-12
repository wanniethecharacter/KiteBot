using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace KiteBot.Commands
{
    class Eval
    {
        public static void RegisterEvalCommand(DiscordClient client)
        {
            Console.WriteLine("Registering 'Eval'...");
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StreamReader).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DiscordClient).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(CommandEventArgs).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(DateTimeWithZone).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonConvert).Assembly.Location)
            };
            client.GetService<CommandService>().CreateCommand("eval")
                .Parameter("func", ParameterType.Unparsed)
                .Hide()
                .AddCheck((c, u, ch) => u.Id == Program.Settings.OwnerId)
                .Do(async cea =>
                {
                    string arg = cea.Args[0];
                    if (arg.Contains('^'))
                    {
                        await cea.Channel.SendMessage("**Note:** `^` is the Binary XOR operator. Use `Math.Pow(base, exponent)` if you wish to calculate an exponentiation.");
                    }
                    if (!arg.Contains("return"))
                    {
                        arg = $"return {arg}";
                    }
                    if (!arg.EndsWith(";"))
                    {
                        arg += ';';
                    }
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
$@"using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
namespace DynamicCompile
{{
    public class DynEval
    {{
        public async Task<string> Eval<T>(Func<Task<IEnumerable<T>>> set) => String.Join("", "", await set());
        public async Task<string> Eval<T>(Func<Task<T>> func) => (await func())?.ToString() ?? ""null"";
        public async Task<string> Exec(DiscordClient client, CommandEventArgs e) => await Eval(async () => {{ {arg} }});
    }}
}}");

                    string assemblyName = Path.GetRandomFileName();
                    CSharpCompilation compilation = CSharpCompilation.Create(
                        assemblyName: assemblyName,
                        syntaxTrees: new[] { syntaxTree },
                        references: references,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                    using (var ms = new MemoryStream())
                    {
                        EmitResult result = compilation.Emit(ms);

                        if (result.Success)
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            Assembly assembly = Assembly.Load(ms.ToArray());

                            Type type = assembly.GetType("DynamicCompile.DynEval");
                            object obj = Activator.CreateInstance(type);
                            var res = type.InvokeMember("Exec",
                                BindingFlags.Default | BindingFlags.InvokeMethod,
                                null,
                                obj,
                                new object[2] { client, cea });
                            await cea.Channel.SendMessage($"**Result:** {((Task<string>)res).GetAwaiter().GetResult()}");
                        }
                        else
                        {
                            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                                diagnostic.IsWarningAsError ||
                                diagnostic.Severity == DiagnosticSeverity.Error);

                            Console.Error.WriteLine(String.Join("\n", failures.Select(f => $"{f.Id}: {f.GetMessage()}")));
                            await cea.Channel.SendMessage($"**Error:** {failures.First().GetMessage()}");
                        }
                    }
                });
        }
    }
}
