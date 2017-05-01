using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kaepora
{
    [Group("welcome")]
    public class WelcomeDmCommands : ExtendedModuleBase
    {
        private const string thumbsUpUnicode = "\x1F44D";
        private WelcomeDmService _welcomeService;

        [Command("demo"), Name("Demo WelcomeDM"), IsHost]
        public async Task Demo() => await Demo(DateTime.UtcNow.Date);

        [Command("demo"), Name("Demo WelcomeDM"), IsHost]
        public async Task Demo(DateTime date) => await Demo(date, null);

        [Command("demo"), Name("Demo WelcomeDM"), IsHost]
        public async Task Demo(Profile stage) => await Demo(default(DateTime), stage);

        private async Task Demo(DateTime date, Profile stage)
        {
            if (stage is null)
                stage = _welcomeService.GetProfileForDate(date);

            await _welcomeService.WelcomeUser(Context.User as SocketGuildUser, stage);
            await Context.Message.AddReactionAsync(thumbsUpUnicode);
        }

        [Command("reload"), Name("Reload Current Welcome")]
        public async Task ReloadCurrentWelcome()
        {
            _welcomeService.ReloadCurrentWelcome();
            await Context.Message.AddReactionAsync(thumbsUpUnicode);
        }

        [Group("segment"), Name("Segment Control"), IsLeader]
        public class Segments : ExtendedModuleBase
        {
            private WelcomeDmService _welcomeService;

            [Command("create"), Name("Create Segment")]
            public async Task Create(string name, int priority, string content)
            {
                if (!_welcomeService.TryCreateSegment(name, priority, content, out Segment segment))
                {
                    await ReplyAsync(":warning: A segment already exists by that name");
                    return;
                }

                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("edit"), Name("Edit Segment")]
            public async Task Edit(Segment segment, string content) => await Edit(segment, -1, content);

            [Command("edit"), Name("Edit Segment")]
            public async Task Edit(Segment segment, int priority) => await Edit(segment, priority, null);

            [Command("edit"), Name("Edit Segment")]
            public async Task Edit(Segment segment, int priority, string content)
            {
                if (priority != -1)
                    segment.Priority = priority;

                if (content != null)
                    segment.Content = content;

                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("delete"), Name("Delete Segment")]
            public async Task Delete(Segment segment)
            {
                _welcomeService.DeleteSegment(segment);
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("activate"), Name("Activate Segment")]
            public async Task Activate(Segment segment)
            {
                if (segment.IsActive)
                {
                    await ReplyAsync(":warning: Segment is already active");
                    return;
                }

                segment.IsActive = true;
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("deactivate"), Name("Deactivate Segment")]
            public async Task Deactivate(Segment segment)
            {
                if (!segment.IsActive)
                {
                    await ReplyAsync(":warning: Segment is already inactive");
                    return;
                }

                segment.IsActive = false;
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            public Segments()
            {
                _welcomeService = DepMap.GetService<WelcomeDmService>();
            }
        }

        [Group("stage"), Name("Stage Control"), IsLeader]
        public class Stages : ExtendedModuleBase
        {
            private WelcomeDmService _welcomeService;

            [Command("create"), Name("Create Stage")]
            public async Task Create(string name, DateTime startDate, DateTime endDate)
            {
                if (_welcomeService.CheckDateRangeForProfile(startDate, endDate, out var foundStage))
                {
                    await ReplyAsync($":warning: Stage `{foundStage.Key}` is already in that date range");
                    return;
                }

                if (!_welcomeService.TryCreateProfile(name, startDate, endDate, out var profile))
                {
                    await ReplyAsync(":warning: A profile already exists by that name");
                    return;
                }

                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("delete"), Name("Delete Stage")]
            public async Task Delete(Profile stage)
            {
                _welcomeService.DeleteProfile(stage);
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("add seg"), Name("Add Segment")]
            public async Task AddTo(Profile stage, Segment segment)
            {
                if (stage.Segments.Any(x => x.Key == segment.Key))
                {
                    await ReplyAsync($":warning: Stage `{stage.Key}` already contains Segment `{segment.Key}`");
                    return;
                }

                stage.Segments.Add(segment);
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("rem seg"), Name("Remove Segment")]
            public async Task RemoveFrom(Profile stage, Segment segment)
            {
                if (!stage.Segments.Any(x => x.Key == segment.Key))
                {
                    await ReplyAsync($":warning: Stage `{stage.Key}` doesn't contain Segment `{segment.Key}`");
                    return;
                }

                stage.Segments.Remove(segment);
                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            [Command("set period"), Name("Set Active Period")]
            public async Task SetPeriod(Profile stage, DateTime startDate, DateTime endDate)
            {
                if (_welcomeService.CheckDateRangeForProfile(startDate, endDate, out var foundStage, stage))
                {
                    await ReplyAsync($":warning: Stage `{foundStage.Key}` is already in that date range");
                    return;
                }

                stage.StartDate = startDate;
                stage.EndDate = endDate;

                await Context.Message.AddReactionAsync(thumbsUpUnicode);
            }

            public Stages()
            {
                _welcomeService = DepMap.GetService<WelcomeDmService>();
            }
        }

        public WelcomeDmCommands()
        {
            _welcomeService = DepMap.GetService<WelcomeDmService>();
        }
    }
}
