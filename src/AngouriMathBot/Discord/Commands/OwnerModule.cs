using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBotTemplate.Discord.Commands
{
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        [Command("echo")]
        [Summary("echos your message")]
        public async Task TestCommandAsync([Remainder]string input)
        {
            await ReplyAsync(input);
        }
    }
}
