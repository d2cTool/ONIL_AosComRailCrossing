using System.Xml.Serialization;

namespace PereezdSrv.Helpers.Command
{
    [XmlRoot("root")]
    public class AosCommands
    {
        [XmlArray("ui_classes")]
        [XmlArrayItem("ui_class")]
        public UI_Class[] UI_Classes { get; set; }
    }
}
