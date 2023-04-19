using System.Collections.Specialized;
using System.Configuration;
using log4net;
using log4net.Config;

namespace ProtocolHelper
{
    public class SettingsHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SettingsHelper));

        public SettingsHelper()
        {
            XmlConfigurator.Configure();
        }

        public string ReadSettings(string key)
        {
            try
            {
                var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                if (section != null)
                {
                    return section[key];
                }

                return string.Empty;
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Exception:", ex);
                Console.WriteLine("Error occurred reading ");
                return string.Empty;
            }
        }
    }
}