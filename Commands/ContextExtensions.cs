using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Kaepora
{
    public static class Responses
    {
        public static async Task ReplyAsync(this ICommandContext context, string message, bool mention = true)
            => await context.Channel.SendMessageAsync($"{(mention ? context.User.Mention + ", " : "")}{message}");

        public static async Task ReplyAsync(this ICommandContext context, Embed embed)
            => await context.Channel.SendMessageAsync(String.Empty, embed: embed);
    }
}