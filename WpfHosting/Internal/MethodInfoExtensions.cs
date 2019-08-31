using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WpfHosting.Internal
{
    internal static class MethodInfoExtensions
    {
        // This version of MethodInfo.Invoke removes TargetInvocationExceptions
        public static object InvokeWithoutWrappingExceptions(this MethodInfo methodInfo, object obj, object[] parameters)
        {
            // These are the default arguments passed when methodInfo.Invoke(obj, parameters) are called. We do the same
            // here but specify BindingFlags.DoNotWrapExceptions to avoid getting TAE (TargetInvocationException)
            // methodInfo.Invoke(obj, BindingFlags.Default, binder: null, parameters: parameters, culture: null)

#pragma warning disable CS8603 // Possible null reference return.
            return methodInfo.Invoke(obj, BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameters, culture: null);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
