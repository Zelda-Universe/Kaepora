using Discord.Commands;
using Discord.WebSocket;
using NLog;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kaepora
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private ServiceDependencyMap _dependencyMap;
        private Logger _logger = LogManager.GetLogger("Commands");

        private bool _isReady = false;

        public CommandService CommandService => _commandService;

        public CommandHandler(DiscordSocketClient client, ServiceDependencyMap dependencyMap)
        {
            _client = client;
            _dependencyMap = dependencyMap;

            _commandService = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            });

            HelpService.RegisterCommandService(_commandService);

            ExtendedModuleBase.SetDependencyMap(dependencyMap);

            _client.MessageReceived += handler;
        }

        private async Task handler(SocketMessage arg)
        {
            if (!_isReady)
                return;

            var msg = arg as SocketUserMessage;

            if (msg == null)
                return;

            if (msg.Author.IsBot)
                return;

            int argPos = 0;

            if (!msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(_client, msg);

            var result = await _commandService.ExecuteAsync(context, argPos, _dependencyMap, MultiMatchHandling.Best);

            _logger.Info($"Command ran by {context.User} in {context.Channel.Name} - {context.Message.Content}");

            if (result.IsSuccess)
                return;


            string response = null;

            switch (result)
            {
                case ParseResult parseResult:
                    response = $":warning: There was an error parsing your command: `{parseResult.ErrorReason}`";
                    break;
                case PreconditionResult preconditionResult:
                    response = $":warning: A precondition of your command failed: `{preconditionResult.ErrorReason}`";
                    break;
                case ExecuteResult executeResult:
                    response = $":warning: Your command failed to execute. If this persists, contact the Bot Developer.\n`{executeResult.Exception.Message}`";
                    _logger.Error(executeResult.Exception);
                    break;
            }

            if (response != null)
                await context.ReplyAsync(response);
        }

        public void StartListening()
        {
            _logger.Info("Now listening for commands");
            _isReady = true;
        }

        public async Task AddAllCommands()
            => await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}