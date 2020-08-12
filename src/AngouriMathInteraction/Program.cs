using AngouriMath;
using AngouriMath.Core.Sys;
using AngouriMath.Extensions;
using AngouriMath.Limits;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            WriteToOutputFile("+am solve {expr}, {expr}, ... for {variable}, {variable}, ... - solves system of equations\n");
            WriteToOutputFile("+am derive {expr} {variable} - differenciates expression\n");
            WriteToOutputFile("+am limit {expr} for {variable} -> {point} {+/-/ } - computes limit of expression\n");
            WriteToOutputFile("+am substitute {expr} for {variable} -> {expr} - replaces variable in expression\n");
            WriteToOutputFile("+am expand {expr} {variable} - expands to get rid of braces\n");
            WriteToOutputFile("+am collapse {expr} {variable} - collapses an expression\n");
            WriteToOutputFile("+am help - displays help message\n");
        }

        // static void CommandVersionHandler(string arg)
        // {
        //     var AngourimathVersion = Assembly.GetAssembly(typeof(MathS)).GetName().Version;
        //     var GenericTensorVersion = Assembly.GetAssembly(typeof(GenericTensor.Core.GenTensor<Entity>)).GetName().Version;
        //     WriteToOutputFile($"> current AngouriMath version: {AngourimathVersion}\n");
        //     WriteToOutputFile($"> current GenericTensor version: {GenericTensorVersion}\n");
        // }

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
                WriteToOutputFile("> cannot evaluate expression");
            }
            else
            {
                string result = expr.Eval().ToString();
                WriteToOutputFile(result);
            }
        }

        static void CommandExpandHandler(string arg)
        {
            var expr = MathS.FromString(arg);
            WriteToOutputFile($"> expand {expr}\n\n");

            string result = expr.Expand().ToString();
            WriteToOutputFile(result);
        }

        static void CommandCollapseHandler(string arg)
        {
            var expr = MathS.FromString(arg);
            WriteToOutputFile($"> collapse {expr}\n\n");

            string result = expr.Collapse().ToString();
            WriteToOutputFile(result);
        }

        static void CommandSubstituteHandler(string arg)
        {
            string[] split = arg.Split("for");
            if (split.Length != 2)
            {
                CommandHandleError("> invalid number of arguments, only one `for` expected");
                return;
            }

            var replacements = split[1].Split("->");
            if (replacements.Length != 2)
            {
                CommandHandleError("> invalid number of arguments, expected {variable} -> {expr}");
                return;
            }

            var expr = MathS.FromString(split[0]);
            var variable = new VariableEntity(replacements[0].Trim());
            var replacement = MathS.FromString(replacements[1]);

            var result = expr.Substitute(variable, replacement);
            WriteToOutputFile($"> raw result: {result}\n");
            WriteToOutputFile($"> after simplify: {result.Simplify()}\n");
        }

        static void CommandLimitHandler(string arg)
        {
            string[] split = arg.Split("for");
            if (split.Length != 2)
            {
                CommandHandleError("> invalid number of arguments, only one `for` expected");
                return;
            }

            var constrains = split[1].Split("->");
            if (constrains.Length != 2)
            {
                CommandHandleError("> invalid number of arguments, expected {variable} -> {expr} {+/- }");
                return;
            }

            var expr = MathS.FromString(split[0]);
            var variable = new VariableEntity(constrains[0].Trim());

            var approach = constrains[1].Trim();
            Entity result;
            if (approach.Last() == '+')
            {
                var point = MathS.FromString(string.Join("", approach.Take(approach.Length - 1)));
                WriteToOutputFile($"> limit {expr} for {variable} -> {point} from right\n\n");
                result = MathS.Limits.Compute(expr, variable, point, ApproachFrom.Right);
            }
            else if (approach.Last() == '-')
            {
                var point = MathS.FromString(string.Join("", approach.Take(approach.Length - 1)));
                WriteToOutputFile($"> limit {expr} for {variable} -> {point} from left\n\n");
                result = MathS.Limits.Compute(expr, variable, point, ApproachFrom.Left);
            }
            else
            {
                var point = MathS.FromString(approach);
                WriteToOutputFile($"> limit {expr} for {variable} -> {point} from both sides\n\n");
                result = MathS.Limits.Compute(expr, variable, point);
            }
            WriteToOutputFile($"> raw result: {result}\n");
            WriteToOutputFile($"> after simplify: {result.Simplify()}\n");
        }

        static void CommandSolveHandler(string arg)
        {
            string[] split = arg.Split("for");

            if(split.Length != 2)
            {
                CommandHandleError("> invalid number of arguments, only one `for` expected");
                return;
            }

            var exprs = split[0].Split(',');
            var vars = split[1].Split(',');

            var entities = exprs.Select(x => MathS.FromString(x)).ToArray();
            var variables = vars.Select(x => new VariableEntity(x.Trim())).ToArray();

            WriteToOutputFile($"> solve {string.Join(", ", exprs)} for {string.Join(", ", vars)}\n\n");

            var solutions = MathS.Equations(entities).Solve(variables);

            WriteToOutputFile("> raw result:\n");
            for (int i = 0; i < variables.Length; i++)
            {
                WriteToOutputFile($"> {variables[i]} = {solutions[0, i]}\n");
            }
            WriteToOutputFile("\n> after simplify:\n");
            for (int i = 0; i < variables.Length; i++)
            {
                WriteToOutputFile($"> {variables[i]} = {solutions[0, i].Simplify()}\n");
            }
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
            WriteToOutputFile($"> raw result: {dfdx}\n");
            WriteToOutputFile($"> after simplify: {dfdx.Simplify()}\n");
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
                    case "collapse":
                        CommandCollapseHandler(arg);
                        break;
                    case "limit":
                        CommandLimitHandler(arg);
                        break;
                    case "substitute":
                        CommandSubstituteHandler(arg);
                        break;
                    default:
                        CommandHandleError("> unknown command: " + command);
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
            // args[0] - output filename
            // args[1] - command (simplify, eval, etc)
            // args[2] - additional arguments

            if(args.Length < 3)
            {
                Console.WriteLine("> not enough arguments: {" + string.Join(", ", args) + "}");
                return;
            }
            fileName = args[0];

            DispatchCommands(args);
        }
    }
}
