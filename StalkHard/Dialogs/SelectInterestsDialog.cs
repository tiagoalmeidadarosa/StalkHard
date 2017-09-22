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

            string id = "2d6e47ac-0e93-4e87-9200-31582d5a531c"; //activity.From.Id
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
                /*case "Amigos":
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;

                    retorno = client.Get("me/friends?fields=name");

                    List<CardElement> cardElements = new List<CardElement>();

                    foreach (var friend in retorno.data)
                    {
                        cardElements.Add(new TextBlock { Text = friend.name, Size = TextSize.Small });
                    }

                    AdaptiveCard adaptiveCard = new AdaptiveCard()
                    {
                        Body = cardElements
                    };

                    reply.Attachments.Add(new Attachment { ContentType = "application/vnd.microsoft.card.adaptive", Content = adaptiveCard });

                    break;*/
                case "Atletas favoritos":
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;

                    retorno = client.Get("me?fields=favorite_athletes");

                    foreach (var athlete in retorno.favorite_athletes)
                    {
                        HeroCard plCard = new HeroCard()
                        {
                            Title = athlete.name
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Esportes":
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;

                    retorno = client.Get("me?fields=sports");

                    foreach (var sport in retorno.sports)
                    {
                        HeroCard plCard = new HeroCard()
                        {
                            Title = sport.name
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

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
                    //FILMES JÁ ASSISTIDOS:
                    //retorno = client.Get("me/video.watches?fields=data");
                    retorno = client.Get("me/movies?fields=name,genre,description,link,cover");

                    foreach (var movie in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: movie.cover.source));

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = movie.link,
                            Type = "openUrl",
                            Title = "Link do Filme"
                        });

                        HeroCard plCard = new HeroCard()
                        {
                            Title = movie.name,
                            Subtitle = movie.genre,
                            Text = movie.description,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

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
                        cardElements.Add(new Image { Url = photo.picture.data.url, Size = ImageSize.Large, HorizontalAlignment = HorizontalAlignment.Center });
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
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;

                    retorno = client.Get("me/likes?fields=name,about,picture");

                    foreach (var like in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: like.picture.data.url));

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = like.name,
                            Subtitle = like.about,
                            Images = cardImages
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Jogos":
                    retorno = client.Get("me/games?fields=name,about,link,picture,description");

                    foreach (var game in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: game.picture.data.url));

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = game.link,
                            Type = "openUrl",
                            Title = "Link"
                        });

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = game.name,
                            Text = game.description,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Livros":
                    //LIVROS JÁ LIDOS:
                    //retorno = client.Get("me/books.reads?fields=data");
                    retorno = client.Get("me/books?fields=name,description,link,picture");

                    foreach (var book in retorno.data)
                    {
                        /*List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: book.picture.data.url));*/

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = book.link,
                            Type = "openUrl",
                            Title = "Link do Livro"
                        });

                        HeroCard plCard = new HeroCard()
                        {
                            Title = book.name,
                            Text = book.description,
                            //Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Músicas":
                    //MÚSICAS JÁ ESCUTADAS:
                    //retorno = client.Get("me/music.listens?fields=data");
                    retorno = client.Get("me/music?fields=name,about,link,picture,genre");

                    foreach (var music in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: music.picture.data.url));

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = music.link,
                            Type = "openUrl",
                            Title = "Link"
                        });

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = music.name,
                            Subtitle = music.genre,
                            Text = music.about,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Televisão":
                    //PROGRAMAS DE TV JÁ ASSISTIDOS:
                    //retorno = client.Get("me/video.watches?fields=data");
                    retorno = client.Get("me/television?fields=name,genre,description,link,cover");

                    foreach (var tv in retorno.data)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: tv.cover.source));

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = tv.link,
                            Type = "openUrl",
                            Title = "Link do Programa"
                        });

                        HeroCard plCard = new HeroCard()
                        {
                            Title = tv.name,
                            Subtitle = tv.genre,
                            Text = tv.description,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Times favoritos":
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;

                    retorno = client.Get("me?fields=favorite_teams");

                    foreach (var team in retorno.favorite_teams)
                    {
                        HeroCard plCard = new HeroCard()
                        {
                            Title = team.name
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
                case "Vídeos":
                    retorno = client.Get("me/videos?fields=description,source,permalink_url,thumbnails");

                    foreach (var video in retorno.data)
                    {
                        ThumbnailUrl image = new ThumbnailUrl();
                        image.Url = video.thumbnails.data[0].uri;
                        foreach (var thumbnail in video.thumbnails.data)
                        {
                            if(thumbnail.is_preferred)
                            {
                                image.Url = thumbnail.uri;
                                break;
                            }
                        }

                        List<CardAction> cardButtons = new List<CardAction>();
                        cardButtons.Add(new CardAction()
                        {
                            Value = "https://www.facebook.com" + video.permalink_url,
                            Type = "openUrl",
                            Title = "Link do Vídeo"
                        });

                        List<MediaUrl> mediaUrl = new List<MediaUrl>();
                        mediaUrl.Add(new MediaUrl(url: video.source));

                        VideoCard plCard = new VideoCard()
                        {
                            Title = video.description,
                            Media = mediaUrl,
                            Image = image,
                            Buttons = cardButtons
                        };

                        Attachment attachment = plCard.ToAttachment();
                        reply.Attachments.Add(attachment);
                    }

                    break;
            }

            //context.Done(reply);
            await context.PostAsync(reply);
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
 