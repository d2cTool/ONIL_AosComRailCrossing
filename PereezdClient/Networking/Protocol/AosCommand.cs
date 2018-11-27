using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PereezdClient.Networking.Protocol
{
    [DataContract]
    public class AosCommand
    {
        [DataMember]
        public string ObjName { get; set; }
        [DataMember]
        public string Class { get; set; }
        [DataMember]
        public string Method { get; set; }
        [DataMember]
        public string Arguments { get; set; }
        [DataMember]
        public int Delay { get; set; }

        public byte[] ToByteArray()
        {
            return Encoding.Unicode.GetBytes(ToUnicodeString());
        }

        public string ToUnicodeString()
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(AosCommand));
            string objStr = string.Empty;
            using (MemoryStream msObj = new MemoryStream())
            {
                js.WriteObject(msObj, this);
                msObj.Position = 0;
                using (StreamReader sr = new StreamReader(msObj))
                {
                    objStr = sr.ReadToEnd();
                    return objStr;
                }
            }
        }

        public static AosCommand ToObj(byte[] jsonByteArray)
        {
            var str = ToStr(jsonByteArray);
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(str)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(AosCommand));
                AosCommand obj = (AosCommand)deserializer.ReadObject(ms);
                return obj;
            }
        }

        public static AosCommand ToObj(string jsonStr)
        {
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonStr)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(AosCommand));
                AosCommand obj = (AosCommand)deserializer.ReadObject(ms);
                return obj;
            }
        }

        public static string ToStr(byte[] json)
        {
            return Encoding.Unicode.GetString(json);
        }
    }
}
