using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Net;

namespace SketchfabPublisher
{
    public static class Publisher
    {
        internal static string Publish(
            string modelPath,
            string modelName,
            string description,
            string tags,
            string token,
            string imagePath,
            string source,
            ref string warn,
            ref string error)
        {
            try
            {
                SketchfabWebRequest p = new SketchfabWebRequest();
                p.title = modelName;
                p.description = description;
                p.contents = Convert.ToBase64String(File.ReadAllBytes(modelPath));
                p.filename = modelName + Path.GetExtension(modelPath);
                p.tags = tags;
                p.token = token;
                p.source = source;
                //p.thumbnail = Convert.ToBase64String(File.ReadAllBytes(imagePath));

                string json_str = p.stringify();

                // Get the URI from the command line.
                Uri httpSite = new Uri(@"https://api.sketchfab.com/model");
                //Uri httpSite = new Uri(@"https://dev.sketchfab.com/model");

                // Create a 'WebRequest' object with the specified url. 
                WebRequest myWebRequest = WebRequest.Create(httpSite);
                myWebRequest.Method = "POST";

                // Write the payload to the request.
                UTF8Encoding encoding = new UTF8Encoding();
                myWebRequest.ContentLength = encoding.GetByteCount(json_str);
                myWebRequest.ContentType = "application/json";

                // Write request data over the wire.
                using (Stream reqStream = myWebRequest.GetRequestStream())
                {
                    reqStream.Write(encoding.GetBytes(json_str), 0,
                        encoding.GetByteCount(json_str));
                }


                // Send the 'WebRequest' and wait for response.
                WebResponse myWebResponse = myWebRequest.GetResponse();

                StreamReader read = new StreamReader(myWebResponse.GetResponseStream());
                string fullResponse = read.ReadToEnd();

                var json_response = JSON.toJSON(fullResponse, typeof(SketchfabWebResponse)) as SketchfabWebResponse;

                if (json_response == null)
                    return null;

                if (warn != null)
                    warn = json_response.warn;

                if (error != null)
                    error = json_response.error;

                return json_response.id;
            }
            catch
            {
                return null;
            }
        }
    }
}
