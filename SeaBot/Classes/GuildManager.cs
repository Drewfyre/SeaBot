using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Linq;
using SeaBot.Classes.Modules;
using System.Data.Common;
using System.Collections.Generic;

namespace SeaBot.Classes
{
    public class GuildManager
    {
        private SocketGuild _Guild;
        private static IMessageChannel _GeneralChat;
        private static IMessageChannel _SpyChannel;
        private SeadogHandler _SeadogHandler;
        private DiscordSocketClient _Client;


        public GuildManager(DiscordSocketClient client)
        {
            this._Client = client;
            this._Client.GuildAvailable += this._Client_LoggedIn;
        }

        public async Task _Client_LoggedIn(SocketGuild arg1)
        {
            await Task.Run(async () =>
            {
                if (arg1.Name == "Schwarze Schar")
                {
                    this._Guild = arg1;
                    _GeneralChat = (this._Client.GetChannel(186909200465657856) as IMessageChannel);
                    _SpyChannel = (this._Client.GetChannel(328180507885961218) as IMessageChannel);


                    await this.InitUsers();

                    this._Client.GuildMemberUpdated += _Client_GuildMemberUpdated;
                    this._Client.UserJoined += _Client_UserJoined;

                    await SendMsg("Bot running!", true);
                }
            });
        }

        private async Task _Client_UserJoined(SocketGuildUser arg)
        {
            await Task.Run(() =>
            {
                this._SeadogHandler.Users.Add(arg);
            });
        }

        public async Task _Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            await this._SeadogHandler.UpdateSeadogAsync(arg1, arg2);
        }

        private async Task InitUsers()
        {
            await Task.Run(async () =>
            {
                int CurrentGuildUserCount = this._Guild.Users.Count;
                List<SocketGuildUser> Users = new List<SocketGuildUser>();

                using (MySqlConnection dbConn = new MySqlConnection(KeyChain.DBConnectionString))
                {
                    await dbConn.OpenAsync();

                    MySqlCommand Command = new MySqlCommand("SELECT user_id FROM users;", dbConn);
                    using (DbDataReader rst = await Command.ExecuteReaderAsync())
                    {
                        if (rst.HasRows)
                        {
                            while (rst.Read())
                            {
                                Users.Add(this._Guild.Users.First(USER => USER.Id == ulong.Parse(rst["user_id"].ToString())));
                            }
                        }
                    }
                    

                    foreach (SocketGuildUser user in this._Guild.Users.Where(USER => !Users.Contains(USER)))
                    {
                        try
                        {
                            Command = new MySqlCommand($"INSERT INTO users (user_id, username, discriminator, mention, active) VALUES ({user.Id}, '{user.Nickname}', {user.Discriminator}, '{user.Mention}', 1)", dbConn);
                            Command.ExecuteNonQuery();
                            Users.Add(user);
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine($"[{DateTime.Now}] Error: {exc.Message}");
                        }
                    }

                    
                    this._SeadogHandler = new SeadogHandler();
                    await this._SeadogHandler.InitializeAsync(Users);
                }
            });
        }

        public static async Task SendMsg(string msg)
        {
            await SendMsg(msg, false);
        }

        public static async Task SendMsg(string msg, bool silent)
        {
            if(silent)
            {
                await _SpyChannel.SendMessageAsync(msg);
            }
            else
            {
                await _GeneralChat.SendMessageAsync(msg);
            }
        }
    }
}
