using AngouriMath;
using AngouriMath.Extensions;
using System;
using System.IO;
using System.Linq;

namespace AngouriMathInteraction
{
    class Program
    {
        static string fileName;

        static void WriteToOutputFile(string text)
        {
            File.AppendAllText(fileName, text);
        }

        static void CommandHandleError(string text)
        {
            WriteToOutputFile(text);
        }

        static void CommandHelpHandler(string arg)
        {
            WriteToOutputFile("I'm bot based on https://github.com/asc-community/AngouriMath \n");
            WriteToOutputFile("+am simplify {expr} - simplifies expression\n");
            WriteToOutputFile("+am eval {expr} - evaluates expression if possible\n");
            WriteToOutputFile("+am solve {expr} {variable} - solves expression for given variable\n");
            WriteToOutputFile("+am derive {expr} {variable} - differenciate expression\n");
            WriteToOutputFile("+am expand {expr} {variable} - expands to get rid of braces\n");
            WriteToOutputFile("+am collapse {expr} {variable} - collapses an expression\n");
            WriteToOutputFile("+am help - displays help message\n");
        }

        static void CommandSimplifyHandler(string arg)
        {
            var expr = MathS.FromString(arg);
            WriteToOutputFile($"> simplify {expr}\n\n");

            string result = expr.Simplify(5).ToString();
            WriteToOutputFile(result);
        }

        static void CommandEvalHandler(string arg)
        {
            var expr = MathS.FromString(arg);
            WriteToOutputFile($"> eval {expr}\n\n");

            if (!MathS.CanBeEvaluated(expr))
            {
                WriteToOutputFile("cannot evaluate expression");
            }
            else
            {
                string result = expr.Eval().ToString();
                WriteToOutputFile(result);
            }
        }

        static void CommandExpandHandler(string arg)
        {
            var expr = arg.Expand();
            WriteToOutputFile(expr.ToString());
        }

        static void CommandCollapseHandler(string arg)
        {
            var expr = arg.Collapse();
            WriteToOutputFile(expr.ToString());
        }

        static void CommandSolveHandler(string arg)
        {
            string[] split = arg.Split();
            if (split.Length < 2)
            {
                CommandHandleError("> not enough arguments to solve method");
            }
            var expr = string.Join("", split.Take(split.Length - 1));
            var variable = split.Last();
            WriteToOutputFile($"> solve {expr} for {variable}\n\n");

            var solutions = expr.SolveEquation(variable);
            WriteToOutputFile($"> raw result: {solutions}\n");
            solutions.FiniteApply(x => x.Simplify());
            WriteToOutputFile($"> after simplify: {solutions}\n");
        }

        static void CommandDeriveHandler(string arg)
        {
            string[] split = arg.Split();
            if (split.Length < 2)
            {
                CommandHandleError("> not enough arguments to derive method");
            }
            var expr = string.Join("", split.Take(split.Length - 1));
            var variable = split.Last();
            WriteToOutputFile($"> derive {expr} for {variable}\n\n");

            var dfdx = MathS.FromString(expr).Derive(variable);
            WriteToOutputFile($"> raw result: {dfdx.ToString()}\n");
            WriteToOutputFile($"> after simplify: {dfdx.Simplify().ToString()}\n");
        }

        static void DispatchCommands(string[] args)
        {
            try
            {
                string command = args[1];
                string arg = args[2];

                switch (command)
                {
                    case "help":
                        CommandHelpHandler(arg);
                        break;
                    case "simplify":
                        CommandSimplifyHandler(arg);
                        break;
                    case "eval":
                        CommandEvalHandler(arg);
                        break;
                    case "solve":
                        CommandSolveHandler(arg);
                        break;
                    case "derive":
                        CommandDeriveHandler(arg);
                        break;
                    case "expand":
                        CommandExpandHandler(arg);
                        break;
                    case "collaps":
                        CommandCollapseHandler(arg);
                        break;
                    default:
                        CommandHandleError("unknown command: " + command);
                        return;
                }
            }
            catch(Exception e)
            {
                CommandHandleError(e.Message + "\n\n" + e.StackTrace);
            }
        }

        static void Main(string[] args)
        {
            // usage
            // args[0] - result filename
            // args[1] - command simplify, eval, etc

            if(args.Length < 3)
            {
                Console.WriteLine("not enough arguments: {" + string.Join(", ", args) + "}");
                return;
            }
            fileName = args[0];

            DispatchCommands(args);
        }
    }
}
