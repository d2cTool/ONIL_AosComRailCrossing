using System.Xml.Serialization;

namespace PereezdSrv.Helpers.Command
{
    public class Argument
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("values")]
        [XmlArrayItem("value")]
        public string[] Values { get; set; }

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
