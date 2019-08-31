using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WpfHosting.Internal
{
    internal class ConfigureContainerBuilder
    {
        public ConfigureContainerBuilder(MethodInfo configureContainerMethod)
        {
            MethodInfo = configureContainerMethod;
        }

        public MethodInfo MethodInfo { get; }

        public Func<Action<object>, Action<object>> ConfigureContainerFilters { get; set; } = f => f;

        public Action<object> Build(object instance) => container => Invoke(instance, container);

        public Type GetContainerType()
        {
            var parameters = MethodInfo.GetParameters();
            if (parameters.Length != 1)
            {
                // REVIEW: This might be a breaking change
                throw new InvalidOperationException($"The {MethodInfo.Name} method must take only one parameter.");
            }
            return parameters[0].ParameterType;
        }

        private void Invoke(object instance, object container)
        {
            ConfigureContainerFilters(StartupConfigureContainer)(container);

            void StartupConfigureContainer(object containerBuilder) => InvokeCore(instance, containerBuilder);
        }

        private void InvokeCore(object instance, object container)
        {
            if (MethodInfo == null)
            {
                return;
            }

            var arguments = new object[1] { container };

            MethodInfo.InvokeWithoutWrappingExceptions(instance, arguments);
        }
    }
}
