using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace SketchfabPublisher
{
    [DataContract]
    internal class JSON
    {
        public string stringify()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(this.GetType());

            ser.WriteObject(stream, this);

            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        public static object toJSON(string json_str, Type t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(t);
            MemoryStream stream = new MemoryStream(getBytes(json_str));
            stream.Position = 0;

            return ser.ReadObject(stream);
        }

        private static byte[] getBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    [DataContract]
    internal class SketchfabWebRequest : JSON
    {
        [DataMember]
        internal string title = null; // model title

        [DataMember]
        internal string description = null; // model description (optional)

        [DataMember]
        internal string contents = null; // base64 encoded model data

        [DataMember]
        internal string filename = null; // name of your model file, this must be given with the extension related to content

        [DataMember]
        internal string tags = null; // list of space separated tags (optional)

        [DataMember]
        internal string token = null; // your sketchfab API token

        [DataMember]
        internal string thumbnail = null; // base64 encoded png thumbnail (optional)

        [DataMember]
        internal string source = null; // model origin (optional)
    }

    [DataContract]
    internal class SketchfabWebResponse : JSON
    {
        [DataMember]
        internal string success = null;

        [DataMember]
        internal string id = null;

        [DataMember]
        internal string error = null;

        [DataMember]
        internal string warn = null;
    }
}
