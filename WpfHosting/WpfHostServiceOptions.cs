using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace WpfHosting
{
    internal class WpfHostServiceOptions
    {
        public Action<IHost> ConfigureApplication { get; set; }

    }
}
