using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Renci.SshNet;

namespace ShellBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private Dictionary<string, int> dict = new Dictionary<string, int>();
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new EchoDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    protected bool firstMessage = true;
    protected bool waitingHost, waitingPassword, waitingUsername;
    protected String host, username, password;
    protected bool connectedToSSH;
    protected SshClient client;
    public async Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);
    }
    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        if(this.firstMessage) {
            firstMessage = false;
            await context.PostAsync("Hey. I'm shell bot and I can run for you SSH commands on any server you want. Try me by typing \"Connect\" :)");
            context.Wait(MessageReceivedAsync);
            return;
        }
        var text = message.Text;
        if(text == "Connect")
        {
            waitingHost = true;
            await context.PostAsync("Type in your host:");
            context.Wait(MessageReceivedAsync);
            return;
        }
        if (waitingHost)
        {
            host = text;
            waitingHost = false;
            waitingUsername = true;
            await context.PostAsync("Type in your username:");
            context.Wait(MessageReceivedAsync);
            return;
        }
        if (waitingUsername)
        {
            username = text;
            waitingUsername = false;
            waitingPassword = true;
            await context.PostAsync("Type in your password:");
            context.Wait(MessageReceivedAsync);
            return;
        }
        if (waitingPassword)
        {
            password = text;
            waitingPassword = false;
            await context.PostAsync("Trying to connect..");

            //Set up the SSH connection
            try {
                //Start the connection
                this.client = new SshClient(host, username, password);
                this.client.Connect();
                var output = client.RunCommand("echo test");
                this.client.Disconnect();
                await context.PostAsync(output.Result.ToString());
            }catch(Exception e)
            {
                await context.PostAsync("A problem appeared"+e.Message);

            }
            context.Wait(MessageReceivedAsync);
            return;
        }


        if (message.Text == "start")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.None);
        }
        else
        {
            await context.PostAsync($"{this.count++}: You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        }
    }
    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }
}