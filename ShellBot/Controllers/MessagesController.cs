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
    protected List<String> cd = new List<String>();
    protected int cdN = 0;

    public async Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);
    }
    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        var text = message.Text.Trim();
        if (this.firstMessage)
        {
            firstMessage = false;
            await context.PostAsync("Hey. I'm shell bot and I can run for you SSH commands on any server you want. Try me by typing \"Connect\" :)");
            context.Wait(MessageReceivedAsync);
            return;
        }

        if (text == "Connect")
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
            try
            {

                using (var client = new SshClient(host, username, password))
                {
                    //Start the connection
                    client.Connect();
                    var output = client.RunCommand("ls");
                    client.Disconnect();
                    this.connectedToSSH = true;
                    await context.PostAsync(output.Result.ToString());

                }
            }
            catch (Exception e)
            {
                await context.PostAsync("Invalid credentials. Please try again. Enter your host:");
                this.waitingHost = true;

            }
            context.Wait(MessageReceivedAsync);
            return;
        }

        if (this.connectedToSSH)
        {
            if (text.Length > 2 && text[0] == 'c' && text[1] == 'd')
            {
                //construct cd string
                //run command on generated cd string
                //if successful

                if (text.Trim().Equals("cd"))
                {

                }
                else
                {
                    if (text.IndexOf('/') != -1) {
                        String[] tempCd = text.Substring(text.IndexOf(" ")).Trim().Split('/');
                        for (int i = 0; i < tempCd.Length; ++i)
                        {
                            if (i == tempCd.Length - 1 && tempCd[i].Trim().Equals(""))
                                break;
                            if (tempCd[i].Equals("../") || tempCd[i].Equals(".."))
                            {
                                if (cdN != 0)
                                {
                                    cd.RemoveAt(cdN-1);
                                    cdN--;
                                }
                            }
                            else
                            {
                                cdN++;
                                cd.Add(tempCd[i]);
                            }
                        }
                    }
                    else
                    {
                        if (text.Substring(text.IndexOf(" ")).Trim().Equals(".."))
                        {
                            if (cdN != 0)
                            {
                                cd.RemoveAt(cdN-1);
                                cdN--;
                            }
                        }
                        else
                        {
                            cdN++;
                            cd.Add(text.Substring(text.IndexOf(" ")).Trim());
                        }
                    }
                    
                }
            }
            else
            {
                try
                {
                    string pwd = "/";
                    for (int i = 0; i < cdN; i++)
                    {
                        pwd += cd[i] + "/";
                    }
                    using (var client = new SshClient(host, username, password))
                    {
                        //Start the connection
                        client.Connect();
                        var output = client.RunCommand("cd " + pwd + ";" + text);
                        
                        client.Disconnect();
                        await context.PostAsync(output.Result.ToString());

                    }
                }
                catch (Exception e)
                {
                    await context.PostAsync("An error occurred");
                }
            }

            if (text.Trim().Equals("exit"))
            {
                await context.PostAsync("exiting session..\n To connect to a new host type \"Connect\":)");
                host = "";
                username = "";
                password = "";
                cd = new List<String>();
                cdN = 0;
            }
            else
            {
                string pwdFinal = "/";
                for (int i = 0; i < cdN; i++)
                {
                    pwdFinal += cd[i] + "/";
                }

                await context.PostAsync("$" + pwdFinal + ">");
            }
            context.Wait(MessageReceivedAsync);
            return;
        }
    }
}
