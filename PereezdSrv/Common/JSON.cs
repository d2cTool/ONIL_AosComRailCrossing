using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PereezdSrv.Common
{
    public static class JSON
    {
        public static T Deserialize<T>(string aJSON) where T : new()
        {
            T deserializedObj = new T();

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(aJSON));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            deserializedObj = (T)ser.ReadObject(ms);
            ms.Close();
            return deserializedObj;
        }

        public static T Deserialize<T>(byte[] aJSON) where T : new()
        {
            T deserializedObj = new T();

            MemoryStream ms = new MemoryStream(aJSON);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            deserializedObj = (T)ser.ReadObject(ms);
            ms.Close();
            return deserializedObj;
        }

        public static string Serialize<T>(T aObject) where T : new()
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, aObject);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        public static byte[] SerializeB<T>(T aObject) where T : new()
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, aObject);
            byte[] json = ms.ToArray();
            ms.Close();
            return json;
        }
    }
}
