using Discord;
using Discord.WebSocket;
using NLog;
using System.Threading.Tasks;

namespace Kaepora
{
    class Program
    {
        static void Main() => AsyncMain().GetAwaiter().GetResult();
        private static Logger _coreLogger = LogManager.GetLogger("Main");

        static async Task AsyncMain()
        {
            _coreLogger.Info("Starting up bot");

            _coreLogger.Info("Reading Config");
            var config = CoreConfig.ReadConfig();

            var client = new DiscordSocketClient();
            client.Log += logging;

            _coreLogger.Info("Initializing Service Manager");
            var serviceDepMap = new ServiceDependencyMap(client);

            _coreLogger.Info("Initializing Command Handler");
            var cmdHandler = new CommandHandler(client, serviceDepMap);
            await cmdHandler.AddAllCommands();

            _coreLogger.Info("Beginning Login and Connection to Discord");
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            cmdHandler.StartListening();

            await Task.Delay(-1);
        }

        private static Task logging(LogMessage arg)
        {
            LogLevel level;

            switch (arg.Severity)
            {
                case LogSeverity.Debug:
                    level = LogLevel.Trace;
                    break;
                case LogSeverity.Verbose:
                    level = LogLevel.Debug;
                    break;
                case LogSeverity.Info:
                    level = LogLevel.Info;
                    break;
                case LogSeverity.Warning:
                    level = LogLevel.Warn;
                    break;
                case LogSeverity.Error:
                    level = LogLevel.Error;
                    break;
                case LogSeverity.Critical:
                    level = LogLevel.Fatal;
                    break;
                default:
                    level = LogLevel.Off;
                    break;
            }

            if (arg.Exception == null)
                _coreLogger.Log(level, arg.Message);
            else
                _coreLogger.Log(level, arg.Exception, arg.Message);

            return Task.CompletedTask;
        }
    }
}