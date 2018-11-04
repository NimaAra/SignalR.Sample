namespace SignalR.Sample.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Internal;
    using Microsoft.AspNetCore.SignalR.Protocol;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal static class SimpleInjectorHelper
    {
        internal static void InitializeContainerForAspNet(this IApplicationBuilder builder, Container container)
        {
            container.AutoCrossWireAspNetComponents(builder);
            container.Verify();
        }

        internal static void IntegrateWithAspNet(this IServiceCollection services, Container container)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton(container);

            services.EnableSimpleInjectorCrossWiring(container);
            services.UseSimpleInjectorAspNetRequestScoping(container);
        }

        internal static void IntegrateWithSignalR(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IHubActivator<>), typeof(SimpleInjectorHubActivator<>));
            services.AddSingleton(typeof(DefaultHubDispatcher<>));
            services.AddSingleton(typeof(HubDispatcher<>), typeof(SimpleInjectorScopeHubDispatcher<>));
        }

        private sealed class SimpleInjectorHubActivator<T> : IHubActivator<T> where T : Hub
        {
            private readonly Container _container;

            public SimpleInjectorHubActivator(Container container) => _container = container;

            public T Create() => _container.GetInstance<T>();

            // SimpleInjector takes care of this.
            public void Release(T hub) {}
        }

        private sealed class SimpleInjectorScopeHubDispatcher<THub> : HubDispatcher<THub> where THub : Hub
        {
            private readonly Container container;
            private readonly HubDispatcher<THub> decorated;

            // ReSharper disable once SuggestBaseTypeForParameter
            public SimpleInjectorScopeHubDispatcher(Container container, DefaultHubDispatcher<THub> decorated)
            {
                this.container = container;
                this.decorated = decorated;
            }

            public override async Task DispatchMessageAsync(HubConnectionContext connection, HubMessage hubMessage)
            {
                using (BeginScope()) await decorated.DispatchMessageAsync(connection, hubMessage);
            }

            public override async Task OnConnectedAsync(HubConnectionContext connection)
            {
                using (BeginScope()) await decorated.OnConnectedAsync(connection);
            }

            public override async Task OnDisconnectedAsync(HubConnectionContext connection, Exception exception)
            {
                using (BeginScope()) await decorated.OnDisconnectedAsync(connection, exception);
            }

            public override IReadOnlyList<Type> GetParameterTypes(string name) => decorated.GetParameterTypes(name);
            public override Type GetReturnType(string invocationId) => decorated.GetReturnType(invocationId);
            private Scope BeginScope() => AsyncScopedLifestyle.BeginScope(container);
        }
    }
}
