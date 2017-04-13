namespace GeoAwareBotServer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using GeoAwareBotServer.BackChannel;
    using System.Diagnostics;

    [Serializable]
    public class DirectLineBotDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            var hasChildCall = false;

                switch (message.Text.ToLower())
                {
                    case "show me a hero card":
                        reply.Text = $"Sample message with a HeroCard attachment";

                        var heroCardAttachment = new HeroCard
                        {
                            Title = "Sample Hero Card",
                            Text = "Displayed in the DirectLine client"
                        }.ToAttachment();

                        reply.Attachments.Add(heroCardAttachment);
                        break;
                    case "send me a botframework image":
                    
                        reply.Text = $"Sample message with an Image attachment";

                        var imageAttachment = new Attachment()
                        {
                            ContentType = "image/png",
                            ContentUrl = "https://docs.botframework.com/en-us/images/faq-overview/botframework_overview_july.png",
                        };

                        reply.Attachments.Add(imageAttachment);

                        break;
                    /*
                     * This enhances the sample to check for an additional phrase.  With a match we initiate our
                     * back channel framework dialogs to communicate with the client device to request the client
                     * location.
                     * 
                     * To maintain consistancy with the underlying sample we create a chlid dialog calling flag
                     * to support the correct syncronisation and not call context.wait.  In the real world we would
                     * use a more robust solution or higher level dialog (e.g. LUIS)
                     */

                    case "ask for my location":

                        context.Call<string>(new GeoBackChannelDialog<string>(), this.ResumeAfterBackChannelDialogGeo);
                        
                        hasChildCall = true;

                        break;
                    default:
                        reply.Text = $"You said '{message.Text}'";
                        break;
                }

            if (!hasChildCall)
            {
                await context.PostAsync(reply);

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterBackChannelDialogGeo(IDialogContext context, IAwaitable<string> result)
        {
            /*
             * Here the dialog with the client is complete and we have thier location.  This can now be stored
             * in case it needs to be resused later in the conversation.  Here we simply relay it back to the client.
             */

            var parameter = await result;

            await context.PostAsync($"I have your location as {parameter}");

            context.Wait(this.MessageReceivedAsync);
        }
    }
}
