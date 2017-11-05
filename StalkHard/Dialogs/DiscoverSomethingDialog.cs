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

            var item = Session.Instance.UserLogin;

            List<CardAction> actions = new List<CardAction>();
            List<int> numerosRandom = new List<int>();
            var rand = new Random();

            if (item != null)
            {
                //No máximo 5 palavras, ou a quantidade de palavras chave que tiver
                int qtdKeyPhrases = item.KeyPhrases.Count >= 5 ? 5 : item.KeyPhrases.Count;

                for (int i = 0; i < qtdKeyPhrases; i++)
                {
                    int index = rand.Next(0, item.KeyPhrases.Count);
                    while (numerosRandom.Contains(index))
                    {
                        index = rand.Next(0, item.KeyPhrases.Count);
                    }

                    numerosRandom.Add(index);

                    var keyPhrase = item.KeyPhrases.ElementAt(index);

                    if (!string.IsNullOrEmpty(keyPhrase.Text))
                    {
                        actions.Add(new CardAction() { Title = keyPhrase.Text, Type = ActionTypes.ImBack, Value = keyPhrase.Text });
                    }
                }
            }

            //Validação para ver se foi possível buscar alguma palavra-chave
            string text = "";
            if(actions.Count > 0)
                text = "Separei alguns temas, selecione sobre o que você deseja descobrir:";
            else
                text = "Desculpe, não encontrei nenhuma informação, a culpa não é minha! \U0001F623";

            var reply = activity.CreateReply(text);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = actions
            };

            // return our reply to the user
            await context.PostAsync(reply);

            if (actions.Count > 0)
                context.Call(new SelectDiscoverSomethingDialog(), this.ResumeAfterSelectDiscoverSomethingDialog);
            else
                context.Wait(MessageReceivedAsync);
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