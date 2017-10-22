using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StalkHard.Services;
using StalkHard.Models;
using System.Threading;
using Facebook;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class SelectRootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var entrou = true;
            var activity = await result as Activity;

            if (activity.Text != null)
            {
                //switch case aqui e colocar o 
                switch(activity.Text.ToUpper())
                {
                    case "DESCOBRIR ALGO":
                        //Análise a partir dos tweets, na busca de sentimentos
                        await context.Forward(new DiscoverSomethingDialog(), this.ResumeAfterDiscoverSomethingDialog, activity, CancellationToken.None);

                        break;
                    case "INTERESSES":
                        //Chama métodos da api do Facebook, para buscar os principais interesses
                        await context.Forward(new InterestsDialog(), this.ResumeAfterInterestsDialog, activity, CancellationToken.None);

                        break;
                    case "INFORMAÇÕES BÁSICAS":
                        //Faz chamadas a API LUIS (Language Understanding Intelligent Service) para entender o que é solicitado
                        await context.Forward(new InfosBasicDialog(), this.ResumeAfterInfosBasicDialog, activity, CancellationToken.None);

                        break;
                    default:
                        entrou = false;

                        break;
                }
            }
            else
            {
                entrou = false;
            }

            //Qualquer outra coisa
            if (!entrou)
            {
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var client = scope.Resolve<IConnectorClient>();

                    var reply = activity.CreateReply("Inicialmente você deve escolher um caminho para as buscas, escolha uma das três opções abaixo:");
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
                    context.Wait(this.MessageReceivedAsync);
                }
            }
        }

        public async Task ResumeAfterDiscoverSomethingDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            //var resultFromDiscoverSomething = await result;
            var resultFromSelectInterestsDialog = await result as Activity;

            // Chama o diálogo de Descobrir Algo novamente, pois foi solicitado um cancelar
            await context.Forward(new DiscoverSomethingDialog(), this.ResumeAfterDiscoverSomethingDialog, resultFromSelectInterestsDialog, CancellationToken.None);
        }

        public async Task ResumeAfterInterestsDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Chama o diálogo de Interesses novamente, pois foi solicitado um cancelar
            await context.Forward(new InterestsDialog(), this.ResumeAfterInterestsDialog, resultFromSelectInterestsDialog, CancellationToken.None);
        }

        public async Task ResumeAfterInfosBasicDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromSelectInterestsDialog = await result as Activity;

            // Chama o diálogo de Informações Básicas novamente, pois foi solicitado um cancelar
            await context.Forward(new InfosBasicDialog(), this.ResumeAfterInfosBasicDialog, resultFromSelectInterestsDialog, CancellationToken.None);
        }
    }
}