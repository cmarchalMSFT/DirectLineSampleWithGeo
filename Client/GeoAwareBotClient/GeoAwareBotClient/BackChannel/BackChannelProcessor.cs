using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoAwareBotClient.BackChannel
{
    public class BackChannelProcessor
    {
        private  List<IBackChannelHandler> _backChannelHandlers = new List<IBackChannelHandler>();
        private DirectLineClient _client;
        private string _conversationId;
        private static ConcurrentQueue<Activity> _backChannelMessageQueue = new ConcurrentQueue<Activity>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="conversationId">The conversation ID</param>
        public BackChannelProcessor(DirectLineClient client, string conversationId)
        {
            _client = client;
            _conversationId = conversationId;
        }

        /// <summary>
        /// Add a handler to the handler pipeline
        /// </summary>
        /// <param name="handler">The handler to add</param>
        public void RegisterBackChannelHandler(IBackChannelHandler handler)
        {
            _backChannelHandlers.Add(handler);
        }

        /// <summary>
        /// Start the background message pump to pass messages to the bot when device activity is complete
        /// </summary>
        public void Start()
        {
            new System.Threading.Thread(async () => await MessagePump()).Start();
        }

        /// <summary>
        /// Process a message that has been identified for the channel pipeline
        /// </summary>
        /// <param name="activity">The activity to process</param>
        /// <returns></returns>
        public bool Process(Activity activity)
        {
            var handled = false;

            foreach (var backChannelHandler in _backChannelHandlers)
            {
                handled = handled & backChannelHandler.ProcessMessage(activity);
            }

            return handled;
        }

        /// <summary>
        /// Enqueiue a new message to be sent to the bot
        /// </summary>
        /// <param name="message">The message to add</param>
        public static void EnqueueMessage(Activity message)
        {
            _backChannelMessageQueue.Enqueue(message);
        }

        /// <summary>
        /// Fetch any messages and send them to the bot
        /// </summary>
        /// <returns></returns>
        private async Task MessagePump()
        {
            while (true)
            {
                Activity msg;

                if (_backChannelMessageQueue.TryDequeue(out msg))
                {
                    await _client.Conversations.PostActivityAsync(_conversationId, msg);
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
