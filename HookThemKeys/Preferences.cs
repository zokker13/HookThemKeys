using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HookThemKeys
{
    public class Preferences
    {
        public static SettingEntries Load()
        {
            SettingEntries settings = new SettingEntries();

            if (!File.Exists(Constants.SettingsFile))
                return settings;

            XmlSerializer serial = new XmlSerializer(settings.GetType());
            settings = (SettingEntries)serial.Deserialize(new StreamReader(Constants.SettingsFile));

            return settings;
        }

        public static bool Write(SettingEntries settings)
        {
            bool result = false;

            try
            {
                XmlSerializer serial = new XmlSerializer(settings.GetType());
                serial.Serialize(new StreamWriter(Constants.SettingsFile), settings);

                result = true;
            }
            catch (IOException)
            {
                // TODO log or whatever
            }
            catch (Exception)
            {
                // TODO: Same 
            }

            return result;
        }
    }

    public class SettingEntries
    {
        public int Blyat;
        public List<Mapping> Mappings;

        public SettingEntries()
        {
            Blyat = 42;
            Mappings =  new List<Mapping>();
        }
    }
}
