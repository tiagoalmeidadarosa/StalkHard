using Microsoft.Bot.Connector;
using StalkHard.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StalkHard.Services
{
    public class Luis
    {
        public static async Task<Utterance> GetResponse(Activity activity)
        {
            using (var client = new HttpClient())
            {
                //const string authKey = "f8d7a1aa415444ed88b9ca9aeac2aa8f";
                const string authKey = "e7b56aded41647ceb2a25c09d0223db5";

                var url = $"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/f2330796-b110-456b-ad2a-7a43a17a5e55?subscription-key={authKey}&verbose=true&timezoneOffset=-180&q={activity.Text}";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var result = await response.Content.ReadAsStringAsync();

                var js = new DataContractJsonSerializer(typeof(Utterance));
                var ms = new MemoryStream(Encoding.ASCII.GetBytes(result));
                var list = (Utterance)js.ReadObject(ms);

                return list;
            }
        }
    }
}