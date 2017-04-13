using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GeoAwareBotClient.BackChannel
{
    public abstract class BackChannelHandler : IBackChannelHandler
    {

        string _user;
        string _contentType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">Client user name</param>
        /// <param name="channelContentType">The unique content type for this dialog</param>
        public BackChannelHandler(string user, string channelContentType)
        {
            _user = user;
            _contentType = channelContentType;
        }

        /// <summary>
        /// Process a message and handle if this handler is the target
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public bool ProcessMessage(Activity activity)
        {
            if(activity.Attachments != null && activity.Attachments.Count == 1)
            {
                if (activity.Attachments[0].ContentType.Contains(_contentType))
                {
                    return HandleMessage(activity);
                }
            }

            return false;
        }

        /// <summary>
        /// Configures an activity with the reusable schema for communication with the client
        /// </summary>
        /// <param name="content">The information to relay to the bot</param>
        /// <param name="sequence">The sequence number in the conversation</param>
        /// <returns></returns>
        protected Activity BuildBackChannelActivity(string content, int sequence)
        {
            Activity message = new Activity
            {
                From = new ChannelAccount(_user),
                Text = string.Empty,
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { new Attachment
                                        {
                    ContentType = _contentType + '/' + sequence,
                    Content = content
                }
            },
            };

            return message;
        }

        /// <summary>
        /// Called in a deriving class to process the message
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public abstract bool HandleMessage(Activity activity);

    }
}
