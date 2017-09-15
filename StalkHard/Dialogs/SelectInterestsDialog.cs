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

namespace StalkHard.Dialogs
{
    [Serializable]
    public class SelectInterestsDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string id = "3d58e7d8-344c-408c-9d27-cb9064f7141e"; //activity.From.Id
            var item = await DocumentDBRepository<Login>.GetItemAsync(id);

            var client = new FacebookClient();
            client.AccessToken = item.AccessTokenFacebook.AccessToken;
            client.Version = "v2.10";
            client.AppId = ConfigurationManager.AppSettings["appIdFacebook"];
            client.AppSecret = ConfigurationManager.AppSettings["appSecretFacebook"];

            var reply = activity.CreateReply("Estes são alguns resultados que encontrei:");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            dynamic retorno = null;

            switch (activity.Text)
            {
                case "Amigos":
                    break;
                case "Atletas favoritos":
                    break;
                case "Esportes":
                    break;
                case "Eventos":
                    retorno = client.Get("me/events?fields=name,cover");

                    foreach (var evento in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: evento.cover.source));

                        /*List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = photo.link,
                            Type = "openUrl",
                            Title = "Link da Foto"
                        });*/

                        HeroCard plCard = new HeroCard()
                        {
                            Title = evento.name,
                            Images = cardImages/*,
                            Buttons = cardButtons*/
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Filmes":
                    break;
                case "Fotos":
                    retorno = client.Get("me/photos?fields=name,picture,link,webp_images");

                    foreach (var photo in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: photo.webp_images[0].source));

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = photo.link,
                            Type = "openUrl",
                            Title = "Link da Foto"
                        });

                        HeroCard plCard = new HeroCard()
                        {
                            Subtitle = photo.name,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);

                        //ADAPTIVECARD : http://adaptivecards.io/explorer/#ActionOpenUrl
                        /*List<CardElement> cardElements = new List<CardElement>();
                        cardElements.Add(new Image { Url = photo.picture, Size = ImageSize.Large, HorizontalAlignment = HorizontalAlignment.Center });
                        cardElements.Add(new TextBlock { Text = photo.name, Size = TextSize.Small });

                        List<ActionBase> cardActions = new List<ActionBase>();
                        cardActions.Add(new OpenUrlAction { Url = photo.link, Title = "Link da Foto" });

                        AdaptiveCard adaptiveCard = new AdaptiveCard()
                        {
                            Body = cardElements,
                            Actions = cardActions
                        };

                        Attachment attachment = new Attachment();
                        attachment.ContentType = "application/vnd.microsoft.card.adaptive";
                        attachment.Content = adaptiveCard;*/
                    }

                    break;
                case "Gostos":
                    break;
                case "Jogos":
                    break;
                case "Livros":
                    retorno = client.Get("me/books");

                    break;
                case "Músicas":
                    break;
                case "Televisão":
                    break;
                case "Times favoritos":
                    break;
                case "Vídeos":
                    retorno = client.Get("me/events?fields=description,source,permalink_url,thumbnails");

                    //Falta arrumar muita coisa: https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-connector-api-reference#videocard-object

                    foreach (var video in retorno.data)
                    {
                        /*List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: video.));*/

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = "www.facebook.com" + video.permalink_url,
                            Type = "openUrl",
                            Title = "Link do Vídeo"
                        });

                        List<MediaUrl> mediaUrl = new List<MediaUrl>();
                        mediaUrl.Add(new MediaUrl(url: video.source));

                        VideoCard plCard = new VideoCard()
                        {
                            Title = video.description,
                            Media = mediaUrl,
                            //Images = cardImages,
                            //Image = 
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
            }

            context.Done(reply);
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
 