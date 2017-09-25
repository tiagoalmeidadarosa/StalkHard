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
            var activity = await result as Activity;

            if(activity.Text.ToUpper().Equals("DESCOBRIR ALGO"))
            {
                //Análise a partir dos tweets, na busca de sentimentos
                await context.Forward(new DiscoverSomethingDialog(), this.ResumeAfterDiscoverSomethingDialog, activity, CancellationToken.None);
            }
            else if (activity.Text.ToUpper().Equals("INTERESSES"))
            {
                //Chama métodos da api do Facebook, para buscar os principais interesses
                await context.Forward(new InterestsDialog(), this.ResumeAfterInterestsDialog, activity, CancellationToken.None);
            }
            else
            {
                //INFORMAÇÕES BÁSICAS ou qualquer outra coisa
                //Faz chamadas a API LUIS (Language Understanding Intelligent Service) para entender o que é solicitado
                await context.Forward(new InfosBasicDialog(), this.ResumeAfterInfosBasicDialog, activity, CancellationToken.None);
            }

            //context.Wait(this.MessageReceivedAsync);
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