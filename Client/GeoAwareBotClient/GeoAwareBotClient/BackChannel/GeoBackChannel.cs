using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoAwareBotClient.BackChannel
{
    public class GeoBackChannel : BackChannelHandler
    {
        public GeoBackChannel(string user) : base(user, "app/request/geo") { }

        public override bool HandleMessage(Activity activity)
        {
            /*
             * In a real world application, in the hack scenario that generated this sample, this
             * would now use device capabilities to get the current GPS location for the user.
             */

            var message = this.BuildBackChannelActivity("41.009175, 0.3825944", 1); //Bot, Spain

            BackChannelProcessor.EnqueueMessage(message);

            return true;
        }
    }
}
