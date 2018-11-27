using PereezdSrv.Common;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace PereezdSrv.Helpers.Command
{
    public static class CommandManager
    {
        public static AosCommands GetCommands()
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            XmlSerializer serializer = new XmlSerializer(typeof(AosCommands));
            using (XmlReader reader = XmlReader.Create(currentDir + Globals.SettingsFileName))
            {
                return (AosCommands)serializer.Deserialize(reader);
            }
        }

        public static AosCommands GetCommands(string pathToSettings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AosCommands));
            using (XmlReader reader = XmlReader.Create(pathToSettings + Globals.SettingsFileName))
            {
                return (AosCommands)serializer.Deserialize(reader);
            }
        }

        public static bool SetCommands(AosCommands model)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            XmlSerializer serializer = new XmlSerializer(typeof(AosCommands));
            using (XmlWriter writer = XmlWriter.Create(currentDir + Globals.SettingsFileName))
            {
                serializer.Serialize(writer, model);
                return true;
            }
        }

        public static bool SetCommands(string pathToSettings, AosCommands commands)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AosCommands));
            using (XmlWriter writer = XmlWriter.Create(pathToSettings + Globals.SettingsFileName))
            {
                serializer.Serialize(writer, commands);
                return true;
            }
        }
    }
}
