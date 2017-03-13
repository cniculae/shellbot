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
using System.Threading;

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
    protected String host, username, password, fileName;
    protected bool connectedToSSH, writeInFile;
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
        if (text.Trim().Equals("exit", StringComparison.InvariantCultureIgnoreCase))
        {
            await context.PostAsync("exiting session..\n To connect to a new host type \"Connect\":)");
            host = "";
            username = "";
            password = "";
            this.waitingHost = false;
            this.waitingPassword = false;
            this.waitingUsername = false;
            this.writeInFile = false;
            cd = new List<String>();
            cdN = 0;
            context.Wait(MessageReceivedAsync);
            return;
        }
        if (text.Equals("presentation"))
        {
            await context.PostAsync("Hey Facebook. My name is ShellBot. Probably never heard of me :'( . It's alright I can show you what I can do:");
            Thread.Sleep(5000);
            await context.PostAsync("Wait...");
            Thread.Sleep(550);
            await context.PostAsync("...");
            Thread.Sleep(400);
            await context.PostAsync("my name is too obvious; yeah right I'm a messenger bot that can be used as a ssh terminal to connect to a ssh server...");
            Thread.Sleep(5000);
            await context.PostAsync("I forgot about this little detail. But you know about what I don't forget?");
            Thread.Sleep(5000);
            await context.PostAsync("My ssh signups. Yeah, that's right, connect once to the ssh with me, and it will be possible that you'll forget your server's password");
            Thread.Sleep(5000);
            await context.PostAsync("But you think that's all? I may surprise you. How many times did you accessed your personal server while on the London tube or while going to Costa to take a coffee?");
            Thread.Sleep(5000);
            await context.PostAsync("Yeah, you never could do that right? Or at least not when you ended up spending 5 hours in Costa with your facny Macbook.");
            Thread.Sleep(5000);
            await context.PostAsync("I may not be a macbook. But I'm more flexible than a macbook. Let's face it; A MESSENGER BOT!!");
            Thread.Sleep(5000);
            await context.PostAsync("That's insane, right? You don't have to download anything right? I mean, who doesn't have messenger these days. And still look what I can do!");
            using (var client = new SshClient(host, username, password))
            {
                //Start the connection
                client.Connect();
                Thread.Sleep(5000);
                await context.PostAsync("Maybe you want to give your other servers a ping? No problem");

                Thread.Sleep(5000);

                var output = client.RunCommand("ping -c 1 www.facebook.com");
                await context.PostAsync(output.Result.ToString());

                Thread.Sleep(7000);

                await context.PostAsync("Maybe you want to check the man page for something. Why not go Vim? Careful, the page is quite horrible and big! (let's do a 'man vim')");

                Thread.Sleep(6000);
                output = client.RunCommand("man vim");
                await context.PostAsync(output.Result.ToString());

                Thread.Sleep(7000);

                await context.PostAsync("...sorry about that...");

                Thread.Sleep(6000);

                await context.PostAsync("but what about man page of vim without spaces? A 'sed' can't do so much bad! :D ");

                Thread.Sleep(4000);

                output = client.RunCommand("man vim | sed 's/ //g'");
                await context.PostAsync(output.Result.ToString());

                Thread.Sleep(3000);

                await context.PostAsync("Sorry for the bad joke...But look, I can be useful okay?");
                Thread.Sleep(3000);
                await context.PostAsync("Look, I can run a simple ls");
                Thread.Sleep(3000);

                output = client.RunCommand("ls");
                await context.PostAsync(output.Result.ToString());

                Thread.Sleep(3000);
                await context.PostAsync("Maybe something more difficult? mkdir? How about a directory called FacebookHack?");
                Thread.Sleep(3000);

                output = client.RunCommand("mkdir FacebookHack");
                await context.PostAsync(output.Result.ToString());

                Thread.Sleep(3000);
                await context.PostAsync("Followed of course by another ls to show that it worked!");
                Thread.Sleep(3000);

                output = client.RunCommand("ls");
                await context.PostAsync(output.Result.ToString());
                Thread.Sleep(3000);

                await context.PostAsync("Yey...");
                Thread.Sleep(3000);

                await context.PostAsync("How about some bash? Can you write some scripts? Let's create one!");
                Thread.Sleep(3000);

                await context.PostAsync("Is this good enough:\r\n #!/bin/bash \r\n echo\"Hello Facebook <3 \"? And ofc, we should name it weLoveFacebook.sh");

                output = client.RunCommand("echo \"#!/bin/bash \r\n echo\"Hello Facebook <3 \"\" > script.sh");
                await context.PostAsync(output.Result.ToString());
                Thread.Sleep(3000);

                await context.PostAsync("Let's see what happened. Another ls?");
                Thread.Sleep(3000);

                output = client.RunCommand("ls");
                await context.PostAsync(output.Result.ToString());

                await context.PostAsync("It worked! But wait...What if there's actually nothing in the script :| ?");
                Thread.Sleep(5000);
                await context.PostAsync("Let's do a cat.");
                Thread.Sleep(7000);
                await context.PostAsync("😺");
                Thread.Sleep(2000);
                await context.PostAsync("...");
                Thread.Sleep(2000);
                await context.PostAsync("It's a cat joke! Please don't be mad.....here's the actual command: cat weLoveFacebook.sh");
                Thread.Sleep(7000);
                output = client.RunCommand("cat weLoveFacebook.sh");
                await context.PostAsync(output.Result.ToString());
                Thread.Sleep(2000);
                await context.PostAsync("See? It works! Please don't be mad now.");
                Thread.Sleep(3000);
                await context.PostAsync("So what do we wait for? Let's run it");
                Thread.Sleep(3000);
                await context.PostAsync("Wait.");
                Thread.Sleep(3000);
                await context.PostAsync("A room full of programmers and nobody noticed that I created the script without giving it permissions? chmoding it riiiight now!");
                Thread.Sleep(5000);
                output = client.RunCommand("chmod +u+x weLoveFacebook.sh");
                await context.PostAsync(output.Result.ToString());
                Thread.Sleep(3000);
                await context.PostAsync("Now, let's test it!");
                Thread.Sleep(3000);
                output = client.RunCommand("sh weLoveFacebook.sh");
                await context.PostAsync(output.Result.ToString());
                
                client.Disconnect();

                this.connectedToSSH = true;

            }



        }
        if (text.ToLower() == "connect")
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
                    var output = client.RunCommand("echo test");
                    client.Disconnect();
                    this.connectedToSSH = true;
                    await context.PostAsync("Cool. You're connected. Type in a command.");
                    await context.PostAsync("$/>");

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
            if (this.writeInFile)
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
                        string command = "echo \"" + text +"\" > " + this.fileName;
                        var output = client.RunCommand("cd " + pwd + ";" + command);
                        client.Disconnect();
                        if (!output.Result.ToString().Equals(""))
                            await context.PostAsync("$Output: " + output.Result.ToString());
                    }
                }
                catch (Exception e)
                {
                    await context.PostAsync("An error occurred. Try again.");
                }
                this.writeInFile = false;
            }
            else
            if (text.Length > 2 && text[0] == 'c' && text[1] == 'd')
            {
                //construct cd string
                //run command on generated cd string
                //if successful

                if (text.Trim().Equals("cd"))
                {
                    this.writeInFile = false;
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
            else if(text.Split(' ')[0].Equals("nano", StringComparison.InvariantCultureIgnoreCase)){
                String[] parts = text.Split(' ');
                String file = parts[parts.Length - 1];
                this.writeInFile = true;
                this.fileName = file;
                await context.PostAsync("Enter text:");
                context.Wait(MessageReceivedAsync);
                return;
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
                            string command = text.ToString();
                            var output = client.RunCommand("cd " + pwd + ";" + Char.ToLowerInvariant(text[0]) + text.Substring(1));
                            client.Disconnect();
                            if (!output.Result.ToString().Equals(""))
                                await context.PostAsync("$Output: " + output.Result.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        await context.PostAsync("An error occurred. Try again.");
                    }              
            }

            string pwdFinal = "/";
            for (int i = 0; i < cdN; i++)
            {
                pwdFinal += cd[i] + "/";
            }

            await context.PostAsync("$" + pwdFinal + ">");
            context.Wait(MessageReceivedAsync);
            return;
        }
    }
}
