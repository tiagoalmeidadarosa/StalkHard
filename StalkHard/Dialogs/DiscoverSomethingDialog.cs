using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StalkHard.Services;
using StalkHard.Models;
using System.Threading;

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
            var response = String.Empty;
            var activity = await result as Activity;

            // return our reply to the user
            await context.PostAsync(response);

            context.Wait(MessageReceivedAsync);
            //context.Done("");
        }
    }
}