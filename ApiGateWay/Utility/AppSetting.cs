using System;
namespace ApiGateWay.Utility
{
    public class AppSetting
    {
        private IConfiguration _iconfiguration;
        public string Get(string key)
        {
            string value = string.Empty;
            _iconfiguration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            value = _iconfiguration[key];
            return value;
        }
    }
}

