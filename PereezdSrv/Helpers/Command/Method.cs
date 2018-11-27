using System.Xml.Serialization;

namespace PereezdSrv.Helpers.Command
{
    public class Method
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("arguments")]
        [XmlArrayItem("argument")]
        public Argument[] Arguments { get; set; }

        [XmlIgnore]
        public bool ArgumentsSpecified
        {
            get { return !(Arguments == null); }
            set { return; }
        }

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
