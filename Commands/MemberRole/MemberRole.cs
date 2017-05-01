using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kaepora
{
    public class MemberRoleCommand : ExtendedModuleBase
    {
        private const string thumbsUpUnicode = "\x1F44D";

        [Command("make member"), Name("Give Member Role"), IsHost]
        public async Task GiveMemberRole(SocketGuildUser user)
        {
            if (user.Roles.Any(x => x.Id == 308586794616881152))
            {
                await ReplyAsync($":warning: {user} already has the member role!");
                return;
            }

            var role = DepMap.GetService<MemberTracking>().EnsureRole();

            await user.AddRoleAsync(role);

            await Context.Message.AddReactionAsync(thumbsUpUnicode);
        }
    }
}