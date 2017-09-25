using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SeaBot.Classes.Modules 
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private CommandService _Service;

        [Command("Seadog when?")]
        public async Task SeadogWhenCommand()
        {            
            await SeadogHandler.sGetSeadogTimers(Context.User.Mention);
        }

        [Command("Predict Seadog")]
        public async Task PredictSeadog(string duration)
        {
            await SeadogHandler.PredictNextOnline(Context.User, uint.Parse(duration));
        }

        [Command("help")]
        public async Task HelpCommand()
        {
            if (this._Service != null)
            {
                string s = "Available commands are: \r\n\r\n";

                foreach (var cmd in this._Service.Commands)
                {
                    if (cmd.Name != "SetSeadogTime") // TODO: implement permissions
                    {
                        s += "- " + cmd.Name + "\r\n";  
                    }
                }

                var chann = await Context.User.GetOrCreateDMChannelAsync();
                await chann.SendMessageAsync(s);
            }
        }

        [Command("SetSeadogTime")]
        public async Task SetSeadogTimeCommand(string date, string time)
        {
            if (Context.User.Discriminator == "5356") // TODO: Use permissions
            {
                await SeadogHandler.SetLastOnline(DateTime.Parse(date + " " + time)); 
            }
        }

        public Commands(CommandService service) : base()
        {
            this._Service = service;
        }
        
    }
}
