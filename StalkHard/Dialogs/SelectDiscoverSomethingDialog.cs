using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StalkHard.Services;
using StalkHard.Models;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Configuration;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class SelectDiscoverSomethingDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            string textReply = "";
            var activity = await result as Activity;

            if (activity.Text != null)
            {
                string appId = ConfigurationManager.AppSettings["MicrosoftAppId"];
                string appPass = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
                StateClient stateClient = new StateClient(new MicrosoftAppCredentials(appId, appPass));
                //StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var item = userData.GetProperty<Login>("UserData");

                if (item != null && item.KeyPhrases.Count(k => k.Text.ToUpper().Contains(activity.Text.ToUpper())) > 0)
                {
                    var rand = new Random();
                    var keyPhrase = item.KeyPhrases.First(k => k.Text.ToUpper().Contains(activity.Text.ToUpper()));

                    string idTweet = keyPhrase.References.ElementAt(rand.Next(keyPhrase.References.Count())).IdTweet;

                    // Retorna o tweet que se refere ao sentimento selecionado
                    var tweetFormat = "https://api.twitter.com/1.1/statuses/show.json?id={0}";
                    var tweetUrl = string.Format(tweetFormat, idTweet);

                    HttpWebRequest tweetRequest = (HttpWebRequest)WebRequest.Create(tweetUrl);
                    var tweetHeaderFormat = "{0} {1}";
                    tweetRequest.Headers.Add("Authorization", string.Format(tweetHeaderFormat, item.AccessTokenTwitter.TokenType, item.AccessTokenTwitter.AccessToken));
                    tweetRequest.Method = "Get";
                    WebResponse tweetResponse = tweetRequest.GetResponse();
                    var tweetJson = string.Empty;
                    dynamic tweet = null;
                    using (tweetResponse)
                    {
                        using (var reader = new StreamReader(tweetResponse.GetResponseStream()))
                        {
                            tweetJson = reader.ReadToEnd();
                        }

                        tweet = new JavaScriptSerializer().DeserializeObject(tweetJson);
                    }

                    string[] defaultMessages = { "\"{0}\"",
                                             "Em relação a isso, eu posso dizer que: \"{0}\"",
                                             "Posso me referir a " + activity.Text + " dizendo que: \"{0}\"" };

                    textReply = "Ocorreu algum problema ao retornar o texto relacionado a este sentimento \U0001F621";
                    if (tweet != null && !string.IsNullOrEmpty(tweet["text"]))
                    {
                        //Pegar uma mensagem aleatória para que a resposta não seja sempre a mesma
                        textReply = string.Format(defaultMessages[rand.Next(defaultMessages.Count())], tweet["text"]);
                    }
                }
                else
                {
                    textReply = "Desculpe, eu não encontrei nada relacionado a sua solicitação \U0001F61E";
                }
            }
            else
            {
                textReply = "Não estou capacitado para entender este tipo de solicitação, desculpe! \U0001F61E";
            }

            var reply = activity.CreateReply(textReply);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }
    }
}