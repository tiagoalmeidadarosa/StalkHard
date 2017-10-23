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
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Builder.Location.Bing;

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

            var item = await DocumentDBRepository<Login>.GetItemAsync(activity.From.Id);

            var client = new FacebookClient();
            client.AccessToken = item.AccessTokenFacebook.AccessToken;
            client.Version = "v2.10";
            client.AppId = ConfigurationManager.AppSettings["appIdFacebook"];
            client.AppSecret = ConfigurationManager.AppSettings["appSecretFacebook"];

            var rand = new Random();

            string[] defaultMessages = { "Estes são alguns resultados que encontrei:",
                                         "Aqui estão alguns dos principais resultados sobre isso:",
                                         "Eu gosto disso, fique à vontade para saber mais:" };

            var reply = activity.CreateReply(defaultMessages[rand.Next(defaultMessages.Count())]);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            dynamic retorno = null;

            //Verifica se foi solicitado algum interesse via pergunta
            if (activity.Text != null)
            {
                string[] words = activity.Text.Split(' ');
                if (words.Length > 1)
                {
                    //Call API LUIS (Language Understanding Intelligent Service)
                    var responseLUIS = await Luis.GetResponse(activity);

                    //Trata resposta (DEVE SER CRIADO EM UM OUTRO MÉTODO)
                    if (responseLUIS != null)
                    {
                        var intent = responseLUIS.topScoringIntent;

                        if (!string.IsNullOrEmpty(intent.intent) && intent.score >= 0.30) //30%
                        {
                            activity.Text = intent.intent;
                        }
                    }
                }

                switch (activity.Text.ToUpper())
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
                    case "ATLETAS":
                    case "ATHLETES":
                        reply.AttachmentLayout = AttachmentLayoutTypes.List;

                        retorno = client.Get("me?fields=favorite_athletes");

                        if (retorno.Count > 1)
                        {
                            foreach (var athlete in retorno.favorite_athletes)
                            {
                                HeroCard plCard = new HeroCard()
                                {
                                    Title = athlete.name
                                };

                                Attachment attachment = plCard.ToAttachment();
                                reply.Attachments.Add(attachment);
                            }
                        }

                        break;
                    case "ESPORTES":
                    case "SPORTS":
                        reply.AttachmentLayout = AttachmentLayoutTypes.List;

                        retorno = client.Get("me?fields=sports");

                        if (retorno.Count > 1)
                        {
                            foreach (var sport in retorno.sports)
                            {
                                HeroCard plCard = new HeroCard()
                                {
                                    Title = sport.name
                                };

                                Attachment attachment = plCard.ToAttachment();
                                reply.Attachments.Add(attachment);
                            }
                        }

                        break;
                    case "EVENTOS":
                    case "EVENTS":
                        retorno = client.Get("me/events?fields=name,cover,id");

                        foreach (var evento in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                cardImages.Add(new CardImage(url: evento.cover.source));
                            }
                            catch (Exception ex) { }

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = "https://www.facebook.com/events/" + evento.id,
                                Type = "openUrl",
                                Title = "Mais Informações"
                            });

                            HeroCard plCard = new HeroCard()
                            {
                                Title = evento.name,
                                Images = cardImages,
                                Buttons = cardButtons
                            };

                            Attachment attachment = plCard.ToAttachment();
                            reply.Attachments.Add(attachment);
                        }

                        break;
                    case "FILMES":
                    case "MOVIES":
                        //FILMES JÁ ASSISTIDOS:
                        //retorno = client.Get("me/video.watches?fields=data");
                        retorno = client.Get("me/movies?fields=name,genre,about,description,link,cover");

                        foreach (var movie in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                cardImages.Add(new CardImage(url: movie.cover.source));
                            }
                            catch (Exception ex) { }

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = movie.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
                            });

                            HeroCard plCard = new HeroCard()
                            {
                                Title = movie.name,
                                Subtitle = movie.genre,
                                Text = !string.IsNullOrEmpty(movie.description) ? movie.description : movie.about,
                                Images = cardImages,
                                Buttons = cardButtons
                            };

                            Attachment attachment = plCard.ToAttachment();
                            reply.Attachments.Add(attachment);
                        }

                        break;
                    case "FOTOS":
                    case "PHOTOS":
                        retorno = client.Get("me/photos?fields=name,link,webp_images");

                        foreach (var photo in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            cardImages.Add(new CardImage(url: photo.webp_images[0].source));

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = photo.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
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
                            cardActions.Add(new OpenUrlAction { Url = photo.link, Title = "Mais Informações" });

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
                    case "GOSTOS":
                    case "LIKES":
                        reply.AttachmentLayout = AttachmentLayoutTypes.List;

                        retorno = client.Get("me/likes?fields=name,about,picture");

                        foreach (var like in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                cardImages.Add(new CardImage(url: like.picture.data.url));
                            }
                            catch (Exception ex) { }

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
                    case "JOGOS":
                    case "GAMES":
                        retorno = client.Get("me/games?fields=name,link,picture,description,category");

                        foreach (var game in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                cardImages.Add(new CardImage(url: game.picture.data.url));
                            }
                            catch (Exception ex) { }

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = game.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
                            });

                            ThumbnailCard plCard = new ThumbnailCard()
                            {
                                Title = game.name,
                                Subtitle = game.category,
                                Text = game.description,
                                Images = cardImages,
                                Buttons = cardButtons
                            };

                            Attachment attachment = plCard.ToAttachment();
                            reply.Attachments.Add(attachment);
                        }

                        break;
                    case "LIVROS":
                    case "BOOKS":
                        //LIVROS JÁ LIDOS:
                        //retorno = client.Get("me/books.reads?fields=data");
                        retorno = client.Get("me/books?fields=name,description,link,picture,about");

                        foreach (var book in retorno.data)
                        {
                            /*List<CardImage> cardImages = new List<CardImage>();
                            cardImages.Add(new CardImage(url: book.picture.data.url));*/

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = book.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
                            });

                            HeroCard plCard = new HeroCard()
                            {
                                Title = book.name,
                                Text = !string.IsNullOrEmpty(book.description) ? book.description : book.about,
                                //Images = cardImages,
                                Buttons = cardButtons
                            };

                            Attachment attachment = plCard.ToAttachment();
                            reply.Attachments.Add(attachment);
                        }

                        break;
                    case "MÚSICAS":
                    case "MUSICAS":
                    case "MUSIC":
                        //MÚSICAS JÁ ESCUTADAS:
                        //retorno = client.Get("me/music.listens?fields=data");
                        retorno = client.Get("me/music?fields=name,about,link,picture,genre");

                        foreach (var music in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                cardImages.Add(new CardImage(url: music.picture.data.url));
                            }
                            catch (Exception ex) { }

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = music.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
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
                    case "TELEVISÃO":
                    case "TELEVISAO":
                    case "TELEVISION":
                        //PROGRAMAS DE TV JÁ ASSISTIDOS:
                        //retorno = client.Get("me/video.watches?fields=data");
                        retorno = client.Get("me/television?fields=name,genre,description,link,cover,about");

                        foreach (var tv in retorno.data)
                        {
                            List<CardImage> cardImages = new List<CardImage>();
                            try
                            {
                                if (tv.cover.Count > 0)
                                {
                                    cardImages.Add(new CardImage(url: tv.cover.source));
                                }
                            }
                            catch (Exception ex) { }

                            List<CardAction> cardButtons = new List<CardAction>();
                            cardButtons.Add(new CardAction()
                            {
                                Value = tv.link,
                                Type = "openUrl",
                                Title = "Mais Informações"
                            });

                            HeroCard plCard = new HeroCard()
                            {
                                Title = tv.name,
                                Subtitle = tv.genre,
                                Text = !string.IsNullOrEmpty(tv.description) ? tv.description : tv.about,
                                Images = cardImages,
                                Buttons = cardButtons
                            };

                            Attachment attachment = plCard.ToAttachment();
                            reply.Attachments.Add(attachment);
                        }

                        break;
                    case "TIMES":
                    case "TEAMS":
                        reply.AttachmentLayout = AttachmentLayoutTypes.List;

                        retorno = client.Get("me?fields=favorite_teams");

                        if (retorno.Count > 1)
                        {
                            foreach (var team in retorno.favorite_teams)
                            {
                                HeroCard plCard = new HeroCard()
                                {
                                    Title = team.name
                                };

                                Attachment attachment = plCard.ToAttachment();
                                reply.Attachments.Add(attachment);
                            }
                        }

                        break;
                    case "VÍDEOS":
                    case "VIDEOS":
                        retorno = client.Get("me/videos?fields=description,source,permalink_url,thumbnails");

                        foreach (var video in retorno.data)
                        {
                            ThumbnailUrl image = new ThumbnailUrl();
                            image.Url = video.thumbnails.data[0].uri;
                            foreach (var thumbnail in video.thumbnails.data)
                            {
                                if (thumbnail.is_preferred)
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
                                Title = "Mais Informações"
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
                    case "LUGARES":
                    case "PLACES":
                        retorno = client.Get("me/tagged_places?fields=name,place");

                        var apiKey = ConfigurationManager.AppSettings["BingMapsApiKey"];

                        List<Models.Location> locations = new List<Models.Location>();
                        foreach (var place in retorno.data)
                        {
                            Models.Location location = new Models.Location();

                            Models.GeocodePoint geocodePoint = new Models.GeocodePoint();
                            geocodePoint.Coordinates = new List<double>();
                            geocodePoint.Coordinates.Add(place.place.location.latitude); //latitude
                            geocodePoint.Coordinates.Add(place.place.location.longitude); //longitude

                            location.Point = geocodePoint;
                            location.Name = place.place.name;

                            try
                            {
                                string endereco = "";
                                if (!string.IsNullOrEmpty(place.place.location.street))
                                    endereco = place.place.location.street;

                                if (!string.IsNullOrEmpty(place.place.location.city) && !string.IsNullOrEmpty(place.place.location.state))
                                {
                                    if(string.IsNullOrEmpty(endereco))
                                        endereco += place.place.location.city + "/" + place.place.location.state;
                                    else
                                        endereco += " - " + place.place.location.city + "/" + place.place.location.state;
                                }

                                if (!string.IsNullOrEmpty(place.place.location.country))
                                {
                                    if (string.IsNullOrEmpty(endereco))
                                        endereco += place.place.location.country;
                                    else
                                        endereco += " - " + place.place.location.country;
                                }

                                if (!string.IsNullOrEmpty(endereco))
                                    location.Name += " (" + endereco + ")";
                            }
                            catch (Exception ex) { }

                            locations.Add(location);
                        }

                        var cards = new List<HeroCard>();
                        int i = 1;
                        foreach (var location in locations)
                        {
                            var heroCard = new HeroCard
                            {
                                Subtitle = location.Name
                            };

                            if (location.Point != null)
                            {
                                var image =
                                    new CardImage(
                                        url: new BingGeoSpatialService(apiKey).GetLocationMapImageUrl(location, i));

                                heroCard.Images = new[] { image };
                            }

                            cards.Add(heroCard);

                            i++;
                        }

                        foreach (var card in cards)
                        {
                            reply.Attachments.Add(card.ToAttachment());
                        }

                        break;
                }
            }

            if(reply.Attachments.Count == 0)
            {
                reply.Text = "Desculpe! Eu não encontrei nada sobre isso ou não estou capacitado para entender esse tipo de solicitação \U0001F61E";
            }

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }
    }
}
 