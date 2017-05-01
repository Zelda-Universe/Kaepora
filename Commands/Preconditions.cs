using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaepora
{
    public class IsLeader : PreconditionAttribute
    {
        public const ulong LeaderRoleId = 207234548306673664;

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as SocketGuildUser;

            if (!user.Roles.Any(r => r.Id == LeaderRoleId))
                return Task.FromResult(PreconditionResult.FromError("Only ZU Leaders can use this command"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    public class IsHost : PreconditionAttribute
    {
        public const ulong HostRoleId = 292818427566096396;
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as SocketGuildUser;

            if (!user.Roles.Any(r => r.Id == HostRoleId || r.Id == IsLeader.LeaderRoleId))
                return Task.FromResult(PreconditionResult.FromError("Only ZU Hosts and Leaders can use this command"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}