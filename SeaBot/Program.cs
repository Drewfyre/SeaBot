using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeaBot.Classes;

namespace SeaBot
{
    public class Program
    {
        private DiscordSocketClient _Client;
        private CommandManager _cManager;
        private GuildManager _mManager;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        private async Task StartAsync()
        {
            this._Client = new DiscordSocketClient();
            this._mManager = new GuildManager(this._Client);          

            await this._Client.LoginAsync(TokenType.Bot, "MzI4MTU0ODM0NjY0NTU0NDk2.DC_x8A._OXC0yj45ZXOsGDAEFofi2s9Ztg");
            await this._Client.StartAsync();

            this._cManager = new CommandManager();
            await this._cManager.InitializeAsync(this._Client);

            //await this._Client.StopAsync();
            //await this._Client.LogoutAsync();
                        
            await Task.Delay(-1);
        }

        
    }
}