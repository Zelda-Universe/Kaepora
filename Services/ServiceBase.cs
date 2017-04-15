using Discord.WebSocket;
using NLog;
using System.Threading.Tasks;

namespace Kaepora
{
    public abstract class ServiceBase
    {
        public static DiscordSocketClient Client { get; set; }
        internal static Logger ServiceLogger = LogManager.GetLogger("ServiceManager");
        public bool IsEnabled { get; internal set; }
        protected abstract Task PreEnable();

        protected abstract Task PreDisable();

        public async Task<bool> TryEnable()
        {
            if (IsEnabled)
                return false;

            await PreEnable();
            IsEnabled = true;
            ServiceLogger.Info($"Enabled Service {this.GetType().Name}");
            return true;
        }

        public async Task<bool> TryDisable()
        {
            if (!IsEnabled)
                return false;

            await PreDisable();
            IsEnabled = false;
            ServiceLogger.Info($"Disabled Service {this.GetType().Name}");
            return true;
        }
    }
}