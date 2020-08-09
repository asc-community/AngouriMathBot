using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotTemplate.Models;
using DiscordBotTemplate.Utilities;

namespace DiscordBotTemplate.Discord.Commands
{
    public class AngouriMathModule : ModuleBase
    {
        [Command("am")]
        [Summary("invokes AngouriMath module")]
        public async Task AngouriMathInvokeAsync(string command, [Remainder]string input)
        {
            string uniqueId = Context.Message.Id.ToString();
            string resultFile = uniqueId + ".txt";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "AngouriMathInteraction.exe",
                CreateNoWindow = true,
                Arguments = $"{resultFile} {command} \"{input}\""
            });

            if (!process.WaitForExit(10000)) // wait for 10 seconds
            {
                process.Kill();
                await ReplyAsync(Context.User.Mention + " operation timeout");
            }
            else
            {
                string result = File.ReadAllText(resultFile);
                await ReplyAsync(Context.User.Mention + " result: ```\n" + result + "```");
            }

            if (File.Exists(resultFile)) File.Delete(resultFile);
        }

        [Command("am")]
        [Summary("invokes AngouriMath module")]
        public async Task AngouriMathInvokeAsync(string command)
        {
            await AngouriMathInvokeAsync(command, "_");
        }
    }
}
