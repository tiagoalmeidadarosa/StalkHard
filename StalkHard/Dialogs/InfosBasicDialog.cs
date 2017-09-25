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
    public class InfosBasicDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var reply = activity.CreateReply("Aqui você pode me conhecer melhor fazendo algumas perguntas básicas, fique à vontade! :). \nEx: \"Qual a sua idade?\"");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            await context.PostAsync(reply);

            context.Call(new SelectInfosBasicDialog(), this.ResumeAfterSelectInfosBasicDialog);
        }

        public async Task ResumeAfterSelectInfosBasicDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Fecha o diálogo, para ser chamado novamente no método de resume do diálogo anterior
            context.Done(resultFromSelectInterestsDialog);
        }
    }
}
 