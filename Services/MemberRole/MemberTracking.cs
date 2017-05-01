using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kaepora
{
    public class MemberTracking : ServiceBase
    {
        private SocketRole _memberRole = null;
        protected override Task PreEnable()
        {
            Client.MessageReceived += messageHandler;
            return Task.CompletedTask;
        }

        protected override Task PreDisable()
        {
            Client.MessageReceived -= messageHandler;
            return Task.CompletedTask;
        }

        private async Task messageHandler(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            
            if (msg is null)
                return;

            var user = msg.Author as SocketGuildUser;

            if (user is null || user.Guild.Id != 207233454671265793 || user.Roles.Count > 1)
                return;

            if (_memberRole == null)
                _memberRole = Client.GetGuild(207233454671265793).GetRole(308586794616881152);

            using (var db = new MemberTrackingContext())
            {
                db.Messages.Add(new TrackedMessage()
                {
                    UserId = user.Id,
                    MessageId = msg.Id,
                    Timestamp = msg.Timestamp.UtcDateTime
                });

                if (fortyMessagesTotal() || twentyMessagesToday() || tenMessagesHour())
                {
                    await user.AddRoleAsync(_memberRole);
                    db.Messages.RemoveRange(db.Messages.Where(x => x.UserId == user.Id));
                }

                await db.SaveChangesAsync();

                bool fortyMessagesTotal() => db.Messages.Count(x => x.UserId == user.Id) >= 40;

                bool twentyMessagesToday() => db.Messages.Count(x => x.UserId == user.Id && x.Timestamp.Date == DateTime.UtcNow.Date) >= 20;

                bool tenMessagesHour() => db.Messages.Count(x => x.UserId == user.Id && x.Timestamp.AddHours(1) <= DateTime.UtcNow) >= 10;
            }
        }
    }
}