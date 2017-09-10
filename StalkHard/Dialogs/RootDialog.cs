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
    public class RootDialog : IDialog<object>
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

            if(activity.Text.ToLower().Equals("descobrir algo"))
            {
                //Análise do Twitter
                await context.Forward(new DiscoverSomethingDialog(), this.ResumeAfterDiscoverSomethingDialog, activity, CancellationToken.None);
            }
            else if (activity.Text.ToLower().Equals("interesses"))
            {
                //Chama métodos da api do facebook
            }
            else
            {
                //Informações Básicas ou qualquer outra coisa

                //Call API LUIS (Language Understanding Intelligent Service)
                var responseLUIS = await Luis.GetResponse(activity);

                //Trata resposta (DEVE SER CRIADO EM UM OUTRO MÉTODO)
                if (responseLUIS != null)
                {
                    //Verificar se a intent tem um score suficiente para ser usado
                    var intent = responseLUIS.topScoringIntent;
                    //var entity = new Models.Entity();

                    string descricao = string.Empty;
                    string informacao = string.Empty;

                    foreach (var item in responseLUIS.entities)
                    {
                        switch (item.type)
                        {
                            case "Descricao":
                                descricao = item.entity;
                                break;
                            case "Informacao":
                                informacao = item.entity;
                                break;
                        }
                    }

                    if (intent.intent.Equals("BuscarInformacao"))
                    {
                        if (!string.IsNullOrEmpty(descricao))
                        {
                            if (!string.IsNullOrEmpty(informacao))
                            {
                                response = "OK entendi! Estou preparando tudo... (" + descricao + " + " + informacao + ")";
                                //Buscar informação das API, Facebook ou Twitter? Isso seria uma nova entidade?
                            }
                            else
                            {
                                response = "Não entendi qual informação você quer.";
                            }
                        }
                        else
                        {
                            response = "Descreva especificamente o que você quer por favor.";
                        }
                    }
                    else
                    {
                        response = "Desculpe! Não entendi a sua intenção.";
                    }
                }

                // return our reply to the user
                await context.PostAsync(response);
            }

            //context.Wait(this.MessageReceivedAsync);
        }

        public async Task ResumeAfterDiscoverSomethingDialog(IDialogContext context, IAwaitable<object> result)
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