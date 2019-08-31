using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WpfHosting.Internal
{
    internal class ConfigureBuilder
    {
        public ConfigureBuilder(MethodInfo configure)
        {
            MethodInfo = configure;
        }

        public MethodInfo MethodInfo { get; }

        public Action<IHost> Build(object instance) => host => Invoke(instance, host);

        private void Invoke(object instance, IHost host)
        {   
            var serviceProvider = host.Services;
            var parameterInfos = MethodInfo.GetParameters();
            var parameters = new object[parameterInfos.Length];
            for (var index = 0; index < parameterInfos.Length; index++)
            {
                var parameterInfo = parameterInfos[index];
                if (parameterInfo.ParameterType == typeof(IHost))
                {
                    parameters[index] = host;
                }
                else
                {
                    try
                    {
                        parameters[index] = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format(
                            "Could not resolve a service of type '{0}' for the parameter '{1}' of method '{2}' on type '{3}'.",
                            parameterInfo.ParameterType.FullName,
                            parameterInfo.Name,
                            MethodInfo.Name,
                            MethodInfo.DeclaringType.FullName), ex);
                    }
                }
            }

            MethodInfo.InvokeWithoutWrappingExceptions(instance, parameters);
            
        }
    }
}
