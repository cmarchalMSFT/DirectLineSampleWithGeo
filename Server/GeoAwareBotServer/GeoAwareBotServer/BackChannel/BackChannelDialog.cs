using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GeoAwareBotServer.BackChannel
{
    [Serializable]
    public abstract class BackChannelDialog<T> : IDialog<T>
    {
        string _contentType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channelContentType">The unique content type for this dialog</param>
        public BackChannelDialog(string channelContentType)
        {
            _contentType = channelContentType;
        }

        /// <summary>
        /// Start the dialog
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task StartAsync(IDialogContext context)
        {
            //Call the base method to initiate the dialog with the client device
            await this.StartDialog(context);

            //Process the resulting communiction
            context.Wait(this.ProcessDialog);
        }

        /// <summary>
        /// Configures an activity with the reusable schema for communication with the client
        /// </summary>
        /// <param name="activity">An activity precreated in the activity chain</param>
        /// <param name="content">The information to relay to the user</param>
        /// <returns></returns>
        protected IMessageActivity BuildBackChannelActivity(IMessageActivity activity, string content)
        {

            activity.Text = content;

            activity.Attachments = new List<Attachment> { new Attachment
                                        {
                    ContentType = _contentType,
                    Content = string.Empty
                    }

            };

            return activity;
        }

        /// <summary>
        /// Parses the activity schema for the sequence number in a multi phase dialog
        /// </summary>
        /// <param name="activity">The activity returned from the client</param>
        /// <returns></returns>
        protected int GetChannelSequenceNumber(IMessageActivity activity)
        {
            int sequence = 0;

            var elements = activity.Attachments[0].ContentType.Split(new char[] {'/'});
            int.TryParse(elements.Last(), out sequence);

            return sequence;
        }

        /// <summary>
        /// Get data from the reusable schema
        /// </summary>
        /// <param name="activity">The activity returned from the client</param>
        /// <returns></returns>
        protected object GetChannelData(IMessageActivity activity)
        {
            return activity.Attachments[0].Content;

        }

        /// <summary>
        /// Called in a deriving class to handle initating communication
        /// </summary>
        /// <param name="context">The current dialog context</param>
        /// <returns></returns>
        protected abstract Task StartDialog(IDialogContext context);

        /// <summary>
        /// Called in a deriving class to handle ongoing communication
        /// </summary>
        /// <param name="context">The current dialog context</param>
        /// <param name="result">The activity to process</param>
        /// <returns></returns>
        protected abstract Task ProcessDialog(IDialogContext context, IAwaitable<IMessageActivity> result);
    }
}