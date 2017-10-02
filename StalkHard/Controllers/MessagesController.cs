using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System.Linq;
using Autofac;

namespace StalkHard
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            switch(message.Type)
            {
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message

                    break;
                case ActivityTypes.ConversationUpdate:
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels

                    /*using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        var client = scope.Resolve<IConnectorClient>();
                        if (message.MembersAdded != null && message.MembersAdded.Any())
                        {
                            foreach (var newMember in message.MembersAdded)
                            {
                                if (newMember.Id != message.Recipient.Id)
                                {
                                    var reply = message.CreateReply("Olá, em que posso ajudá-lo? Escolha uma das três opções:");
                                    reply.Type = ActivityTypes.Message;
                                    reply.TextFormat = TextFormatTypes.Plain;
                                    reply.InputHint = InputHints.IgnoringInput; //Isso deveria desabilitar o input de texto do user

                                    reply.SuggestedActions = new SuggestedActions()
                                    {
                                        Actions = new List<CardAction>()
                                        {
                                            new CardAction(){ Title = "Informações Básicas", Type=ActionTypes.ImBack, Value="Informações Básicas" },
                                            new CardAction(){ Title = "Descobrir Algo", Type=ActionTypes.ImBack, Value="Descobrir Algo" },
                                            new CardAction(){ Title = "Interesses", Type=ActionTypes.ImBack, Value="Interesses" }
                                        }
                                    };

                                    await client.Conversations.ReplyToActivityAsync(reply);

                                    context.Call(new SelectRootDialog(), null);
                                }
                            }
                        }
                    }*/

                    //Testando parâmetro de id de usuário passado para o chatterbot pelo site
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        if (message.MembersAdded != null && message.MembersAdded.Any())
                        {
                            var client = scope.Resolve<IConnectorClient>();

                            string membersAdded = string.Join(
                                ", ",
                                message.MembersAdded.Select(
                                    newMember => (newMember.Id != message.Recipient.Id) ? $"{newMember.Name} (Id: {newMember.Id})"
                                                    : $"{message.Recipient.Name} (Id: {message.Recipient.Id})"));

                            //await context.PostAsync($"Welcome {membersAdded}");

                            //var reply = message.CreateReply(message.From.Id + ", " + message.From.Name + ", " + message.Id + ", " + message.Name);
                            //var reply = message.CreateReply($"Welcome {membersAdded}");
                            var reply = message.CreateReply($"{message.Recipient.Name} (Id: {message.Recipient.Id})");
                            reply.Type = ActivityTypes.Message;
                            reply.TextFormat = TextFormatTypes.Plain;
                            reply.InputHint = InputHints.IgnoringInput; //Isso deveria desabilitar o input de texto do user

                            await client.Conversations.ReplyToActivityAsync(reply);
                        }
                    }

                    break;
                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened

                    break;
                case ActivityTypes.Typing:
                    // Handle knowing tha the user is typing

                    break;
                case ActivityTypes.Ping:
                    break;
            }

            return null;
        }
    }
}