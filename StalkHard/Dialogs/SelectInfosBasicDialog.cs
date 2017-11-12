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
        public Login loginUser;

        public SelectInfosBasicDialog(Login loginUser)
        {
            this.loginUser = loginUser;
        }

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

            if (activity.Text != null)
            {
                //Call API LUIS (Language Understanding Intelligent Service)
                var responseLUIS = await Luis.GetResponse(activity);

                if (responseLUIS != null)
                {
                    //Intenção com a melhor pontuação
                    var intent = responseLUIS.topScoringIntent;

                    dynamic retorno = null;

                    //Verifica se a intent tem um score suficiente para ser usado
                    if (!string.IsNullOrEmpty(intent.intent) && intent.intent.ToUpper() != "NONE" && intent.score >= 0.30) //30%
                    {
                        var item = loginUser;

                        if (item != null)
                        {
                            var client = new FacebookClient();
                            client.AccessToken = item.AccessTokenFacebook.AccessToken;
                            client.Version = "v2.10";
                            client.AppId = ConfigurationManager.AppSettings["appIdFacebook"];
                            client.AppSecret = ConfigurationManager.AppSettings["appSecretFacebook"];

                            if (intent.intent.Equals("hometown")) //Casos em que eu preciso de mais de uma propriedade
                                retorno = client.Get("me?fields=" + intent.intent + ",location");
                            else if (intent.intent.Equals("picture"))
                                retorno = client.Get("me?fields=" + intent.intent + ",about");
                            else
                                retorno = client.Get("me?fields=" + intent.intent);

                            if (retorno.Count > 1)
                            {
                                switch (intent.intent)
                                {
                                    case "name":
                                        response = "Meu nome é " + retorno.name + ", prazer em conhecê-lo! \U0001F600";

                                        break;
                                    case "birthday":
                                        string birthday = retorno.birthday;

                                        if (birthday.Count() == 10)
                                        {
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
                                        }

                                        break;
                                    case "email":
                                        response = "Você pode me contatar no e-mail " + retorno.email + " \U0001F609";

                                        break;
                                    case "education":
                                        foreach (var educ in retorno.education)
                                        {
                                            if (string.IsNullOrEmpty(response))
                                            {
                                                response += educ.type + " - " + educ.school.name;
                                            }
                                            else
                                            {
                                                response += "\n" + educ.type + " - " + educ.school.name;
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = ("Este é o meu histórico de ensino:\n\n" + response);
                                        }

                                        break;
                                    case "hometown":
                                        if (retorno.Count == 3)
                                        {
                                            if (retorno.hometown.name == retorno.location.name)
                                            {
                                                response = "Eu moro em " + retorno.location.name;
                                            }
                                            else
                                            {
                                                response = "Eu sou de " + retorno.hometown.name + ", mas moro em " + retorno.location.name;
                                            }
                                        }

                                        break;
                                    case "interested_in":
                                        response = "Estou interessado em " + string.Join(", ", retorno.interested_in);

                                        break;
                                    case "languages":
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

                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = "Eu falo " + response;
                                        }

                                        break;
                                    case "relationship_status":
                                        response = "Atualmente estou " + retorno.relationship_status;

                                        break;
                                    case "religion":
                                        response = "Falando em religião, eu sou " + retorno.religion;

                                        break;
                                    case "website":
                                        response = "Meu site na web é " + retorno.website;

                                        break;
                                    case "picture":
                                        if (retorno.Count == 3)
                                        {
                                            string about = retorno.about;

                                            List<CardImage> cardImages = new List<CardImage>();
                                            cardImages.Add(new CardImage(url: retorno.picture.data.url));

                                            ThumbnailCard plCard = new ThumbnailCard()
                                            {
                                                Title = "Este sou eu! \U0001F603",
                                                Text = about,
                                                Images = cardImages,
                                            };

                                            Attachment attachment = plCard.ToAttachment();
                                            attachments.Add(attachment);
                                        }
                                        else
                                        {
                                            response = "Este sou eu! \U0001F603";
                                            attachments.Add(new Attachment { ContentType = "image/jpg", ContentUrl = retorno.picture.data.url });
                                        }

                                        break;
                                    case "work":
                                        foreach (var work in retorno.work)
                                        {
                                            string position = "";
                                            try
                                            {
                                                if (work.position.Count > 0)
                                                {
                                                    position = work.position.name;
                                                }
                                            }
                                            catch (Exception ex) { }

                                            if (string.IsNullOrEmpty(response))
                                            {
                                                if (string.IsNullOrEmpty(position))
                                                {
                                                    response += work.employer.name;
                                                }
                                                else
                                                {
                                                    response += position + " - " + work.employer.name;
                                                }
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(position))
                                                {
                                                    response += "\n" + work.employer.name;
                                                }
                                                else
                                                {
                                                    response += "\n" + position + " - " + work.employer.name;
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = ("Estas são as minhas experiências profissionais em ordem decrescente de período:\n\n" + response);
                                        }

                                        break;
                                    case "political":
                                        response = retorno.political;

                                        break;
                                    case "family":
                                        foreach (var family in retorno.family.data)
                                        {
                                            if (string.IsNullOrEmpty(response))
                                            {
                                                response = family.name + " (" + family.relationship + ")";
                                            }
                                            else
                                            {
                                                response += "\n" + family.name + " (" + family.relationship + ")";
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = ("Estes são alguns membros da minha família:\n\n" + response);
                                        }

                                        break;
                                    default:
                                        response = "Desculpe! Não estou habilitado para falar sobre isso \U0001F614";

                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        response = "Desculpe! Analisei a sua pergunta e não entendi muito bem a sua solicitação \U0001F630";
                    }
                }
            }
            else
            {
                response = "Não estou capacitado para entender este tipo de solicitação, desculpe! \U0001F61E";
            }

            if (string.IsNullOrEmpty(response) && attachments.Count == 0)
            {
                response = "Minhas pesquisas não retornaram um valor satisfatório, talvez a informação não esteja habilitada pra mim \U0001F615";
            }

            var reply = activity.CreateReply(response);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Attachments = attachments;

            // return our reply to the user
            await context.PostAsync(reply);

            context.Wait(this.MessageReceivedAsync);
        }
    }
}
 