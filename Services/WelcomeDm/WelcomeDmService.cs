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
    public class WelcomeDmService : ServiceBase
    {
        private WelcomeDbContext _dbContext;
        private const string _header = "**:blue_heart: :blue_heart: __Welcome to the Zelda Universe Discord!!__ :blue_heart: :blue_heart:**";
        private string _assetsDir = null;
        private string _welcomeMessage = "";
        private DateTime _msgExpiresOn = default(DateTime);

        public WelcomeDmService(ServiceDependencyMap map)
        {
            _dbContext = new WelcomeDbContext();
            map.Add(_dbContext);

            _assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        }

        protected override Task PreEnable()
        {
            Client.UserJoined += WelcomeUser;
            _welcomeMessage = GetProfileForDate().ToString();
            _msgExpiresOn = DateTime.UtcNow.Date.AddDays(1);
            return Task.CompletedTask;
        }

        protected override Task PreDisable()
        {
            Client.UserJoined -= WelcomeUser;
            return Task.CompletedTask;
        }
        
        public void ReloadCurrentWelcome()
        {
            _welcomeMessage = GetProfileForDate().ToString();
        }

        public bool TryCreateSegment(string key, int priority, string content, out Segment segment)
        {
            segment = null;

            if (_dbContext.Segments.Any(x => x.Key == key))
                return false;

            segment = new Segment()
            {
                Key = key,
                Priority = priority,
                Content = content
            };

            _dbContext.Segments.Add(segment);

            return true;
        }

        public bool TryCreateProfile(string key, DateTime startDate, DateTime endDate, out Profile profile)
        {
            profile = null;

            if (_dbContext.Profiles.Any(x => x.Key == key))
                return false;

            profile = new Profile()
            {
                Key = key,
                StartDate = startDate,
                EndDate = endDate
            };

            _dbContext.Profiles.Add(profile);

            return true;
        }

        public bool CheckDateRangeForProfile(DateTime startDate, DateTime endDate, out Profile profile)
            => CheckDateRangeForProfile(startDate, endDate, out profile, new Profile[0]);

        public bool CheckDateRangeForProfile(DateTime startDate, DateTime endDate, out Profile profile, params Profile[] profilesToIgnore)
        {
            profile = null;

            foreach (var p in _dbContext.Profiles)
            {
                if (profilesToIgnore.Any(x => x.Key == p.Key) || !(p.StartDate <= endDate && startDate <= p.EndDate))
                    continue;

                profile = p;
                return true;
            }

            return false;
        }

        public bool TryGetSegment(string key, out Segment segment)
            => (segment = _dbContext.Segments.FirstOrDefault(x => x.Key == key)) == null;

        public bool TryGetProfile(string key, out Profile profile)
            => (profile = _dbContext.Profiles.FirstOrDefault(x => x.Key == key)) == null;

        public Profile GetProfileForDate() => GetProfileForDate(DateTime.UtcNow.Date);

        public Profile GetProfileForDate(DateTime date)
            => _dbContext.Profiles.FirstOrDefault(x => x.StartDate <= date && date <= x.EndDate)
                ?? _dbContext.Profiles.FirstOrDefault(x => x.Key == "Default");

        public void DeleteSegment(Segment segment)
        {
            _dbContext.Segments.Remove(segment);
        }

        public void DeleteProfile(Profile profile)
        {
            _dbContext.Profiles.Remove(profile);
        }

        public async Task WelcomeUser(SocketGuildUser user)
        {
            if (user.Guild.Id != 207233454671265793)
                return;

            if (_msgExpiresOn <= DateTime.UtcNow.Date)
            {
                _welcomeMessage = GetProfileForDate().ToString();
                _msgExpiresOn = DateTime.UtcNow.Date.AddDays(1);
            }

            await WelcomeUser(user, null);
        }

        public async Task WelcomeUser(SocketGuildUser user, Profile profile)
        {
            var channel = await user.CreateDMChannelAsync();

            var message = profile?.ToString() ?? _welcomeMessage;

            try
            {
                await Task.Delay(2500);
                await channel.SendMessageAsync(message);
            }
            catch (HttpException ex)
            {
                if (ex.HttpCode == HttpStatusCode.Forbidden)
                    return;

                throw ex;
            }
        }
    }
}