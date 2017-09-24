﻿using System;
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

            string id = "a6661053-41a5-464a-bc4c-166379091881"; //activity.From.Id
            var item = await DocumentDBRepository<Login>.GetItemAsync(id);

            if(item.KeyPhrases.Count(k => k.Text.Equals(activity.Text)) > 0)
            {
                var rand = new Random();
                var keyPhrase = item.KeyPhrases.First(k => k.Text.Equals(activity.Text));

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
                using (tweetResponse)
                {
                    using (var reader = new StreamReader(tweetResponse.GetResponseStream()))
                    {
                        tweetJson = reader.ReadToEnd();
                    }
                }

                dynamic tweet = new JavaScriptSerializer().DeserializeObject(tweetJson);

                textReply = "Ocorreu algum problema ao retornar o texto relacionado a este sentimento.";
                if (!string.IsNullOrEmpty(tweet["text"]))
                {
                    textReply = "Em relação a isso, eu posso dizer que: \"" + tweet["text"] + "\"";
                }
            }
            else
            {
                textReply = "Desculpe, eu não encontrei nada relacionado a sua solicitação!";
            }

            var reply = activity.CreateReply(textReply);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            //context.Done(reply);
            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }
    }
}