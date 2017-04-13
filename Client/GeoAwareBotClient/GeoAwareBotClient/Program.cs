using GeoAwareBotClient.BackChannel;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GeoAwareBotClient
{
    class Program
    {
        private static string directLineSecret;
        private static string botId;
        private static string fromUser;

        private static BackChannelProcessor processor;
        
        public static void Main(string[] args)
        {
            directLineSecret = "{YOUR SECRET}";
            botId = "{YOUR BOT ID";
            fromUser = "{YOUR USER}";

            StartBotConversation().Wait();
        }

        private static async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(directLineSecret);

            var conversation = await client.Conversations.StartConversationAsync();

            /*
             * Create a new processor to handle one or more back channel objects and relay messages
             * back to the bot when a request is handled
             */
            processor = new BackChannelProcessor(client, conversation.ConversationId);

            /*
             * The hack scenario that generated this sample required GPS co-ordinates so we create
             * the sample geographic handler.  Further tpyes of handler could be created to fetch device
             * data (e.g. images, barcode/QRcode etc.)
             */

            processor.RegisterBackChannelHandler(new GeoBackChannel(fromUser));

            /*
             * Start the background message pump that will push back channel messages to the server.  This
             * avoids problems with the client waiting on UI input to proceed and the fact that the device
             * operation may take some time.
             */

            processor.Start();

            new System.Threading.Thread(async () => await ReadBotMessagesAsync(client, conversation.ConversationId)).Start();

            Console.Write("Command> ");

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "exit")
                {
                    break;
                }
                else
                {
                    if (input.Length > 0)
                    {
                        Activity message = new Activity();

                        message = new Activity
                        {
                            From = new ChannelAccount(fromUser),
                            Text = input,
                            Type = ActivityTypes.Message
                        };

                        await client.Conversations.PostActivityAsync(conversation.ConversationId, message);
                    }
                }
            }
        }

        private static async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            while (true)
            {
                var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activitySet?.Watermark;

                var activities = from x in activitySet.Activities
                                 where x.From.Id == botId
                                 select x;

                foreach (Activity activity in activities)
                {

                    /*
                     * Here we pass all incoming activities to the processor which will run it through
                     * all registered handlers for handling.  We are returned a boolean which indicates
                     * whether further processing should be suppressed (or not e.g. a message needs to be
                     * shown to the user.
                     * 
                     * In a real world scenario, all attachement handling would benefit from following a
                     * chained handler pattern but leaving the orignial code in-place to remain consistant
                     * with the underlying sample.
                     */

                    if (!processor.Process(activity))
                    { 
                        Console.WriteLine(activity.Text);

                        if (activity.Attachments != null)
                        {
                            foreach (Attachment attachment in activity.Attachments)
                            {
                                switch (attachment.ContentType)
                                {
                                    case "application/vnd.microsoft.card.hero":
                                        RenderHeroCard(attachment);
                                        break;

                                    case "image/png":
                                        Console.WriteLine($"Opening the requested image '{attachment.ContentUrl}'");

                                        Process.Start(attachment.ContentUrl);
                                        break;
                                }
                            }
                        }

                        Console.Write("Command> ");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }

        private static void RenderHeroCard(Attachment attachment)
        {
            const int Width = 70;
            Func<string, string> contentLine = (content) => string.Format($"{{0, -{Width}}}", string.Format("{0," + ((Width + content.Length) / 2).ToString() + "}", content));

            var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());

            if (heroCard != null)
            {
                Console.WriteLine("/{0}", new string('*', Width + 1));
                Console.WriteLine("*{0}*", contentLine(heroCard.Title));
                Console.WriteLine("*{0}*", new string(' ', Width));
                Console.WriteLine("*{0}*", contentLine(heroCard.Text));
                Console.WriteLine("{0}/", new string('*', Width + 1));
            }
        }
    }
}
