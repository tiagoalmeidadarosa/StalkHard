using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StalkHard.Services;
using StalkHard.Models;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Linq;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result as Activity;

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                var client = scope.Resolve<IConnectorClient>();
                
                var reply = message.CreateReply("Olá! Em que posso ajudá-lo? \n\nA qualquer momento você pode: \nDigitar \"Voltar\" - Para ser encaminhado a um diálogo anterior \nDigitar \"Menu Principal\" - Para voltar a este menu inicial \n\nPrimeiramente, defina seu tipo de busca selecionando uma das três opções abaixo:");
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.InputHint = InputHints.IgnoringInput; //Isso deveria desabilitar o input de texto do user
                reply.AttachmentLayout = AttachmentLayoutTypes.List;

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Descobrir Algo", Type=ActionTypes.ImBack, Value="Descobrir Algo" },
                        new CardAction(){ Title = "Informações Básicas", Type=ActionTypes.ImBack, Value="Informações Básicas" },
                        new CardAction(){ Title = "Interesses", Type=ActionTypes.ImBack, Value="Interesses" }
                    }
                };

                await client.Conversations.ReplyToActivityAsync(reply);

                context.Call(new SelectRootDialog(), this.ResumeAfterSelectRootDialog);
            }
        }

        public async Task ResumeAfterSelectRootDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Chama o diálogo para selecionar o diálogo inicial novamente, pois foi solicitado um cancelar
            await context.Forward(new RootDialog(), null, resultFromSelectInterestsDialog, CancellationToken.None);
        }
    }
}