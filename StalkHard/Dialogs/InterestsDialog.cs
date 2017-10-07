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

namespace StalkHard.Dialogs
{
    [Serializable]
    public class InterestsDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var reply = activity.CreateReply("Veja sobre o que você pode descobrir e escolha um tema:\n(Fique à vontade também para me fazer perguntas abertas sobre cada tema)");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "Amigos", Type=ActionTypes.ImBack, Value="Amigos" },
                    new CardAction(){ Title = "Atletas", Type=ActionTypes.ImBack, Value="Atletas" },
                    new CardAction(){ Title = "Esportes", Type=ActionTypes.ImBack, Value="Esportes" },
                    new CardAction(){ Title = "Eventos", Type=ActionTypes.ImBack, Value="Eventos" },
                    new CardAction(){ Title = "Filmes", Type=ActionTypes.ImBack, Value="Filmes" },
                    new CardAction(){ Title = "Fotos", Type=ActionTypes.ImBack, Value="Fotos" },
                    new CardAction(){ Title = "Gostos", Type=ActionTypes.ImBack, Value="Gostos" },
                    new CardAction(){ Title = "Jogos", Type=ActionTypes.ImBack, Value="Jogos" },
                    new CardAction(){ Title = "Livros", Type=ActionTypes.ImBack, Value="Livros" },
                    new CardAction(){ Title = "Músicas", Type=ActionTypes.ImBack, Value="Músicas" },
                    new CardAction(){ Title = "Televisão", Type=ActionTypes.ImBack, Value="Televisão" },
                    new CardAction(){ Title = "Times", Type=ActionTypes.ImBack, Value="Times" },
                    new CardAction(){ Title = "Vídeos", Type=ActionTypes.ImBack, Value="Vídeos" }
                }
            };

            await context.PostAsync(reply);

            context.Call(new SelectInterestsDialog(), this.ResumeAfterSelectInterestsDialog);
        }

        public async Task ResumeAfterSelectInterestsDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Fecha o diálogo, para ser chamado novamente no método de resume do diálogo anterior
            context.Done(resultFromSelectInterestsDialog);
        }
    }
}
 