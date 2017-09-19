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

            var reply = activity.CreateReply("Aqui você pode fazer perguntas abertas, se eu souber irei responder, fique à vontade! :). Ex: \"Qual a sua idade?\"");
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
            await context.PostAsync(resultFromSelectInterestsDialog);

            // Reseta pilha de diálogos e retorna para o diálogo central
            context.Reset();
            context.Call(new RootDialog(), null);
        }
    }
}
 