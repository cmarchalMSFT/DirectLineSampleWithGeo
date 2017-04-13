using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoAwareBotClient.BackChannel
{
    public interface IBackChannelHandler
    {
        bool ProcessMessage(Activity activity);
    }
}
