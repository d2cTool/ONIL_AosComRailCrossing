using System.Xml.Serialization;

namespace PereezdSrv.Helpers.Command
{
    public class UI_Class
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public Method[] Methods { get; set; }

        [XmlArray("ui_objects")]
        [XmlArrayItem("ui_object")]
        public string[] UI_Objects { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlIgnore]
        public bool DescriptionSpecified
        {
            get { return !string.IsNullOrEmpty(Description); }
            set { return; }
        }
    }
}