using Discord.Commands;
using Discord.WebSocket;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kaepora
{
    public class ServiceDependencyMap : DependencyMap
    {
        private Dictionary<Type, ServiceBase> _registeredServices = new Dictionary<Type, ServiceBase>();
        private DiscordSocketClient _client;
        private Logger _logger = LogManager.GetLogger("ServiceManager");

        public ServiceDependencyMap(DiscordSocketClient client)
        {
            _client = client;
            ServiceBase.Client = client;

            var serviceTypes = Assembly
                .GetEntryAssembly()
                .ExportedTypes
                .Where(x => x.GetTypeInfo().IsSubclassOf(typeof(ServiceBase)));

            foreach (var type in serviceTypes)
            {
                ServiceBase instance = InstantiateService(type);
                instance.TryEnable().GetAwaiter().GetResult();
                _registeredServices.Add(type, instance);

                _logger.Info($"Registered and Enabled Service '{type.FullName}'");
            }

            TryAdd(client);
        }

        ServiceBase InstantiateService(Type type)
        {
            var ctors = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Select(c => (ConstructorInfo: c, Parameters: c.GetParameters()))
                .ToArray();

            var diCtor = ctors
                .Where(c => c.Parameters.Length == 1)
                .FirstOrDefault(c => typeof(DependencyMap).IsAssignableFrom(c.Parameters[0].ParameterType));

            if (diCtor.ConstructorInfo != null)
                return (ServiceBase)diCtor.ConstructorInfo.Invoke(new object[] { this });

            var noParamsCtor = ctors.FirstOrDefault(c => c.Parameters.Length == 0);
            if (noParamsCtor.ConstructorInfo != null)
                return (ServiceBase)noParamsCtor.ConstructorInfo.Invoke(new object[] { });

            throw new MissingMemberException($"Service '{type.FullName}' should provide either a parameterless consturctor or a constructor taking a '{typeof(DependencyMap).FullName}'. No matching public constructor could be found ({ctors.Length} candidate constructors)");
        }

        public TService GetService<TService>() where TService : ServiceBase
        {
            if (_registeredServices.TryGetValue(typeof(TService), out ServiceBase service))
                return service as TService;

            return null;
        }

        public async Task<bool> TryEnable<TService>() where TService : ServiceBase
            => await (GetService<TService>()).TryEnable();

        public async Task<bool> TryDisable<TService>() where TService : ServiceBase
            => await (GetService<TService>()).TryDisable();
    }
}