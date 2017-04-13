using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoAwareBotServer.BackChannel
{
    public interface IBackChannelHandler
    {
        bool ProcessMessage(IMessageActivity activity);

        Task<bool> Initiate(IDialogContext context);
    }
}
