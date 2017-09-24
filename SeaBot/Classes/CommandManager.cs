using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SeaBot.Classes
{
    public class CommandManager
    {
        private DiscordSocketClient _Client;
        private CommandService _Service;
        
        
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            this._Client = client;           
            
            this._Service = new CommandService();
            await this._Service.AddModulesAsync(Assembly.GetEntryAssembly());

            this._Client.MessageReceived += _Client_MessageReceived;
        }

        private async Task _Client_MessageReceived(SocketMessage arg)
        {            
            var message = arg as SocketUserMessage;
            if (message == null)
                return;

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(this._Client.CurrentUser, ref argPos)))
                return;

            var context = new SocketCommandContext(this._Client, message);
            var result = await this._Service.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
