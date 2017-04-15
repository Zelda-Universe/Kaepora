using Discord.Commands;

namespace Kaepora
{
    public abstract class ExtendedModuleBase : ModuleBase<SocketCommandContext>
    {
        public static ServiceDependencyMap DepMap { get; private set; } = null;

        public static void SetDependencyMap(ServiceDependencyMap dependencyMap)
        {
            if (DepMap == null)
                DepMap = dependencyMap;
        }
    }
}