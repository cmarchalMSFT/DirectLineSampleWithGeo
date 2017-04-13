using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace GeoAwareBotServer.BackChannel
{
    [Serializable]
    public class GeoBackChannelDialog<T> : BackChannelDialog<T>
    {
        /// <summary>
        /// Constructor with call to base class with conversation content type
        /// </summary>
        public GeoBackChannelDialog() : base("app/request/geo") { }

        /// <summary>
        /// Starts the dialog with the client device
        /// </summary>
        /// <param name="context">The current dialog context</param>
        /// <returns></returns>
        protected override  async Task StartDialog(IDialogContext context)
        {
            var message = context.MakeMessage();
            
            await context.PostAsync(this.BuildBackChannelActivity(message, "Your device is sending me your location..."));
        }

        /// <summary>
        /// Continue the dialog with the client devide
        /// </summary>
        /// <param name="context">The current dialog context</param>
        /// <param name="result">The activity to process</param>
        /// <returns></returns>
        protected override async Task ProcessDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            //This is a single sequence conversation
            switch (this.GetChannelSequenceNumber(activity))
            {
                case 1:
                    context.Done(this.GetChannelData(activity));
                    break;
                default:
                    context.Wait(this.ProcessDialog);
                    break;
            }

        }

    }
}