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
using Facebook;
using System.Configuration;
using AdaptiveCards;
using System.Globalization;

namespace StalkHard.Dialogs
{
    [Serializable]
    public class SelectInfosBasicDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var response = String.Empty;
            List<Attachment> attachments = new List<Attachment>();

            var activity = await result as Activity;

            //Call API LUIS (Language Understanding Intelligent Service)
            var responseLUIS = await Luis.GetResponse(activity);

            //Trata resposta (DEVE SER CRIADO EM UM OUTRO MÉTODO)
            if (responseLUIS != null)
            {
                var intent = responseLUIS.topScoringIntent;
                //var entity = new Models.Entity();

                //Verificar se a intent tem um score suficiente para ser usado

                /*foreach (var item in responseLUIS.entities)
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
                }*/

                dynamic retorno = null;

                if (!string.IsNullOrEmpty(intent.intent) && intent.score >= 0.40) //40%
                {
                    string id = "a6661053-41a5-464a-bc4c-166379091881"; //activity.From.Id
                    var item = await DocumentDBRepository<Login>.GetItemAsync(id);

                    var client = new FacebookClient();
                    client.AccessToken = item.AccessTokenFacebook.AccessToken;
                    client.Version = "v2.10";
                    client.AppId = ConfigurationManager.AppSettings["appIdFacebook"];
                    client.AppSecret = ConfigurationManager.AppSettings["appSecretFacebook"];
                    
                    //E SE O RETORNO DA API DO FACEBOOK NÃO RETORNAR NADA? NÃO FIZ VALIDAÇÃO

                    switch (intent.intent)
                    {
                        case "name":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Meu nome é " + retorno.name + ", prazer em conhecê-lo! :)";

                            break;
                        case "birthday":
                            retorno = client.Get("me?fields=" + intent.intent);

                            string birthday = retorno.birthday;

                            DateTime dataNasc = Convert.ToDateTime(birthday, CultureInfo.CreateSpecificCulture("en-US"));
                            string format = "dd/MM/yyyy";

                            // Retorna o número de anos
                            int anos = DateTime.Now.Year - dataNasc.Year;

                            // Se a data de aniversário não ocorreu ainda este ano, subtrair um ano a partir da idade
                            if (DateTime.Now.Month < dataNasc.Month || (DateTime.Now.Month == dataNasc.Month && DateTime.Now.Day < dataNasc.Day))
                            {
                                anos--;
                            }

                            response = "Eu nasci em " + dataNasc.ToString(format) + ", tenho " + anos + " anos!";

                            break;
                        case "email":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Você pode me contatar no e-mail " + retorno.email + " ;)";

                            break;
                        case "education":
                            retorno = client.Get("me?fields=" + intent.intent);

                            foreach (var educ in retorno.education)
                            {
                                if(string.IsNullOrEmpty(response))
                                {
                                    response += educ.type + " - " + educ.school.name;
                                }
                                else
                                {
                                    response += "\n" + educ.type + " - " + educ.school.name;
                                }
                            }

                            break;
                        case "hometown":
                            retorno = client.Get("me?fields=" + intent.intent + ",location");

                            if(retorno.hometown.name == retorno.location.name)
                            {
                                response = "Eu moro em " + retorno.location.name;
                            }
                            else
                            {
                                response = "Eu sou de " + retorno.hometown.name + ", mas moro em " + retorno.location.name;
                            }

                            break;
                        case "interested_in":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Estou interessado em " + string.Join(", ", retorno.interested_in);

                            break;
                        case "languages":
                            retorno = client.Get("me?fields=" + intent.intent);

                            foreach (var language in retorno.languages)
                            {
                                if (response == "")
                                {
                                    response += language.name;
                                }
                                else
                                {
                                    response += ", " + language.name;
                                }
                            }

                            if(!string.IsNullOrEmpty(response))
                            {
                                response = "Eu falo " + response;
                            }

                            break;
                        case "relationship_status":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Atualmente estou " + retorno.relationship_status;

                            break;
                        case "religion":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Falando em religião, eu sou " + retorno.religion;

                            break;
                        case "website":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Meu site na web é " + retorno.website;

                            break;
                        case "picture":
                            retorno = client.Get("me?fields=" + intent.intent);

                            response = "Este sou eu! =)";
                            attachments.Add(new Attachment { ContentType = "image/jpg", ContentUrl = retorno.picture.data.url });

                            break;
                        case "work":
                            retorno = client.Get("me?fields=" + intent.intent);

                            foreach (var work in retorno.work)
                            {
                                if (string.IsNullOrEmpty(response))
                                {
                                    response += work.position.name + " - " + work.employer.name;
                                }
                                else
                                {
                                    response += "\n" + work.position.name + " - " + work.employer.name;
                                }
                            }
                                
                            break;
                        default:
                            response = "Desculpe! Eu não encontrei nada sobre isso.";

                            break;
                    }
                }
                else
                {
                    response = "Desculpe! Não entendi muito bem a sua intenção.";
                }
            }

            if(!string.IsNullOrEmpty(response))
            {
                var reply = activity.CreateReply(response);
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.Attachments = attachments;

                // return our reply to the user
                //context.Done(reply);
                await context.PostAsync(reply);
            }

            context.Wait(this.MessageReceivedAsync);
        }

        /*public async Task ResumeAfterSelectInterestsDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that DiscoverSomethingDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            //var resultFromDiscoverSomething = await result;
            //await context.PostAsync($"New order dialog just told me this: {resultFromDiscoverSomething}");

            // Again, wait for the next message from the user.
            context.Wait(this.MessageReceivedAsync);
        }*/
    }
}
 