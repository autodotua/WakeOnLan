using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WakeOnLan
{
    public static class Config
    {
        private static IConfiguration configuration;

        static Config()
        {
            configuration = new ConfigurationBuilder().AddIniFile("config.ini", true).Build();
        }

        public static string GetAddress()
        {
            var address = configuration["common:address"];
            if (address == null)
            {
                address = "127.0.0.1:3390";
            }
            return address;
        }

        public static string GetPassword()
        {
            var p = configuration["common:password"];
            if (p == null)
            {
                p = "";
            }
            return p;
        }

        public static IReadOnlyList<Host> GetHosts()
        {
            return configuration.AsEnumerable().
                Where(p => p.Key.StartsWith("hosts:"))
                .Select(p => new Host() { Name = p.Key, Mac = p.Value })
                .ToList();
        }
    }

    public class Host
    {
        public string Name { get; set; }
        public string Mac { get; set; }
    }
}