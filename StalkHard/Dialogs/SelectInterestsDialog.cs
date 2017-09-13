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
using Facebook;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class SelectInterestsDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string id = "71001bb7-810b-4413-afe8-a85e15b7151b"; //activity.From.Id
            var item = await DocumentDBRepository<Login>.GetItemAsync(id);

            dynamic retorno = null;
            var client = new FacebookClient();
            client.AccessToken = item.AccessTokenFacebook;
            client.Version = "v2.10";
            client.AppId = "254366858409178";
            client.AppSecret = "912c6b60ada739628902e18d09b36f4d";

            switch (activity.Text)
            {
                case "Esportes":
                    break;
                case "Livros":
                    retorno = client.Get("me/books");
                    break;
                case "Eventos":
                    break;
                case "Jogos":
                    break;
                case "Amigos":
                    break;
                case "Gostos":
                    break;
                case "Filmes":
                    break;
                case "Músicas":
                    break;
                case "Fotos":
                    break;
                case "Televisão":
                    break;
                case "Vídeos":
                    break;
                case "Atletas favoritos":
                    break;
                case "Times favoritos":
                    break;
            }

            var reply = activity.CreateReply("Estes são alguns resultados que encontrei:");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            /*reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "Esportes", Type=ActionTypes.ImBack, Value="Esportes" },
                    new CardAction(){ Title = "Livros", Type=ActionTypes.ImBack, Value="Livros" },
                    new CardAction(){ Title = "Eventos", Type=ActionTypes.ImBack, Value="Eventos" },
                    new CardAction(){ Title = "Jogos", Type=ActionTypes.ImBack, Value="Jogos" },
                    new CardAction(){ Title = "Amigos", Type=ActionTypes.ImBack, Value="Amigos" },
                    new CardAction(){ Title = "Gostos", Type=ActionTypes.ImBack, Value="Gostos" },
                    new CardAction(){ Title = "Filmes", Type=ActionTypes.ImBack, Value="Filmes" },
                    new CardAction(){ Title = "Músicas", Type=ActionTypes.ImBack, Value="Música" },
                    new CardAction(){ Title = "Fotos", Type=ActionTypes.ImBack, Value="Fotos" },
                    new CardAction(){ Title = "Televisão", Type=ActionTypes.ImBack, Value="Televisão" },
                    new CardAction(){ Title = "Vídeos", Type=ActionTypes.ImBack, Value="Vídeos" },
                    new CardAction(){ Title = "Atletas favoritos", Type=ActionTypes.ImBack, Value="Eventos" },
                    new CardAction(){ Title = "Times favoritos", Type=ActionTypes.ImBack, Value="Eventos" }
                }
            };*/

            context.Done(reply);
        }

        public async Task ResumeAfterSelectInterestsDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            //var resultFromDiscoverSomething = await result;
            //await context.PostAsync($"New order dialog just told me this: {resultFromDiscoverSomething}");

            // Again, wait for the next message from the user.
            context.Wait(this.MessageReceivedAsync);
        }
    }
}
 