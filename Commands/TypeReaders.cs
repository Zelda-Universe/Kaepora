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
    public class ProfileTypeReader : TypeReader
    {
        private WelcomeDbContext _dbContext;
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var profile = _dbContext.Profiles.FirstOrDefault(x => x.Key == input);

            if (profile == null)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"\"{input}\" is not an existing profile"));
            else
                return Task.FromResult(TypeReaderResult.FromSuccess(profile));
        }

        public ProfileTypeReader(ServiceDependencyMap depMap)
        {
            _dbContext = depMap.Get<WelcomeDbContext>();
        }
    }

    public class SegmentTypeReader : TypeReader
    {
        private WelcomeDbContext _dbContext;
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var segment = _dbContext.Segments.FirstOrDefault(x => x.Key == input);

            if (segment == null)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"\"{input}\" is not an existing segment"));
            else
                return Task.FromResult(TypeReaderResult.FromSuccess(segment));
        }

        public SegmentTypeReader(ServiceDependencyMap depMap)
        {
            _dbContext = depMap.Get<WelcomeDbContext>();
        }
    }

    public class DateTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            var sb = new StringBuilder(input)
                .Replace('-', '.')
                .Replace(',', '.')
                .Replace('.', '.')
                .Replace('\\', '.')
                .Replace('/', '.');

            try
            {
                var today = DateTime.UtcNow.Date;
                var date = DateTime.ParseExact(sb.ToString(), "MMM.dd", DateTimeFormatInfo.InvariantInfo).ToUniversalTime().Date;

                date.AddYears(today.Year - date.Year);

                if (date.Month < today.Month || (date.Month == today.Month && date.Day < today.Day))
                    date.AddYears(1);

                return Task.FromResult(TypeReaderResult.FromSuccess(date));
            }
            catch (FormatException ex)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, ex.Message));
            }
        }
    }
}