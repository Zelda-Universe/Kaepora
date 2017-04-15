using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Kaepora
{
    public class HelpCommand : ExtendedModuleBase
    {
        private HelpService _helpService;
        private DiscordSocketClient _client;

        [Command("help")]
        public async Task Help()
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(80, 187, 229));

            var selfuser = _client.CurrentUser;

            embedBuilder.WithTitle(selfuser.Username)
                .WithThumbnailUrl(selfuser.GetAvatarUrl())
                .WithDescription("An implimentation of Kaepora Bot Template\nEmbellish with bot details here");

            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("help")]
        public async Task Help([Remainder]string Query)
        {
            var embed = _helpService.HelpLookup(Query);

            await ReplyAsync("", embed: embed);
        }

        public HelpCommand()
        {
            _helpService = DepMap.GetService<HelpService>();
            _client = DepMap.Get<DiscordSocketClient>();
        }
    }
}