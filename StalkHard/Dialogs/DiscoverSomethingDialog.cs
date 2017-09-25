using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StalkHard.Services;
using StalkHard.Models;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class DiscoverSomethingDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string id = "a6661053-41a5-464a-bc4c-166379091881"; //activity.From.Id
            var item = await DocumentDBRepository<Login>.GetItemAsync(id);

            List<CardAction> actions = new List<CardAction>();
            foreach (var keyPhrase in item.KeyPhrases)
            {
                actions.Add(new CardAction() { Title = keyPhrase.Text, Type = ActionTypes.ImBack, Value = keyPhrase.Text });
            }

            var reply = activity.CreateReply("Separei alguns temas, selecione sobre o que você deseja descobrir:");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = actions
            };

            // return our reply to the user
            await context.PostAsync(reply);

            context.Call(new SelectDiscoverSomethingDialog(), this.ResumeAfterSelectDiscoverSomethingDialog);
        }

        public async Task ResumeAfterSelectDiscoverSomethingDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Fecha o diálogo, para ser chamado novamente no método de resume do diálogo anterior
            context.Done(resultFromSelectInterestsDialog);
        }
    }
}