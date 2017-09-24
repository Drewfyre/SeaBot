using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SeaBot.Classes.Modules
{
    public class SeadogHandler
    {
        private static SeadogHandler _singleton;
        
        private List<SocketGuildUser> _Users;
        private Dictionary<ulong, DateTime> _LastOnlineTimes = new Dictionary<ulong, DateTime>();

        public List<SocketGuildUser> Users
        {
            get { return this._Users; }
        }

        private DateTime _UserPredictedNextLogin(ulong id)
        {
            return this._LastOnlineTimes[id].AddHours(this.GetAvgSleepTimeAsync(id).Result);
        }
   

        public async Task InitializeAsync(List<SocketGuildUser> Users)
        {
            this._Users = Users;
            _singleton = this;
        }

        public async Task UpdateSeadogAsync(SocketGuildUser PreviousState, SocketGuildUser CurrentState)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (!this._LastOnlineTimes.ContainsKey(PreviousState.Id))
                    {
                        this._LastOnlineTimes.Add(PreviousState.Id, DateTime.Now);
                    }

                    if (PreviousState.Status == Discord.UserStatus.Offline && CurrentState.Status == Discord.UserStatus.Online)
                    {
                        TimeSpan diff = DateTime.Now - this._LastOnlineTimes[PreviousState.Id];
                        if (diff.TotalHours > 5) // only if offline for more than 5 hours
                        {
                            try
                            {
                                using (MySqlConnection dbConn = new MySqlConnection(KeyChain.DBConnectionString))
                                {
                                    await dbConn.OpenAsync();

                                    MySqlCommand Command = new MySqlCommand("INSERT INTO user_timers(user_id, went_offline, came_online, duration) VALUES(" + PreviousState.Id + ", '" + this._LastOnlineTimes[PreviousState.Id].ToString("yyyy-MM-dd hh:mm:ss") + "', '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "', " + Math.Round(diff.TotalHours, 2) + ")", dbConn);
                                    await Command.ExecuteNonQueryAsync();
                                }
                            }
                            catch (Exception exc)
                            {
                                this.Log(exc.Message);
                            }
                        }
                        await GuildManager.SendMsg(PreviousState.Mention
                                                                + " has come online!\r\n\r\n"
                                                                + Format.Code("# Predicted login time\r\n"
                                                                + this._UserPredictedNextLogin(PreviousState.Id).ToString()
                                                                + "\r\n\r\n"
                                                                + "# Actual login time\r\n"
                                                                + DateTime.Now.ToString()
                                                                + "\r\n\r\n"
                                                                + "# Sleeptime\r\n"
                                                                + Math.Round(diff.TotalHours, 2) + " hours!", "Markdown"), (PreviousState.Id == 106880697158905856 ? false : true));

                    }
                    else if (PreviousState.Status == Discord.UserStatus.Online && CurrentState.Status == Discord.UserStatus.Offline)
                    {
                        if (this._LastOnlineTimes.ContainsKey(PreviousState.Id))
                        {
                            this._LastOnlineTimes[PreviousState.Id] = DateTime.Now;
                        }
                        else
                        {
                            this._LastOnlineTimes.Add(PreviousState.Id, DateTime.Now);
                        }
                        await GuildManager.SendMsg(PreviousState.Mention
                                                            + " has gone offline!\r\n"
                                                            + Format.Code("# Average sleep time\r\n"
                                                            + this.GetAvgSleepTimeAsync(PreviousState.Id)
                                                            + "\r\n\r\n"
                                                            + "# Predicted next login\r\n"
                                                            + this._UserPredictedNextLogin(PreviousState.Id).ToString(), "Markdown"), (PreviousState.Id == 106880697158905856 ? false : true));
                    }
                }
                catch (Exception exc)
                {
                    this.Log(exc.Message);
                }
            });
        }

        public async Task<double> GetAvgSleepTimeAsync(ulong UserID)
        {
            await Task.Run(async () =>
            {
                try
                {
                    using (MySqlConnection dbConn = new MySqlConnection(KeyChain.DBConnectionString))
                    {
                        await dbConn.OpenAsync();

                        MySqlCommand Command = new MySqlCommand("SELECT AVG(duration) AS averagesleep FROM user_timers WHERE user_id = " + UserID, dbConn);
                        DbDataReader rst = await Command.ExecuteReaderAsync();

                        if (rst.HasRows)
                        {
                            rst.Read();
                            double avg = 0;
                            bool success = double.TryParse(rst["averagesleep"].ToString(), out avg);
                            if (success && avg != 0)
                            {
                                return avg;
                            }
                            else
                            {
                                return 8f;
                            }
                        }
                        return 8f;
                    }
                }
                catch (Exception exc)
                {
                    this.Log(exc.Message);
                    return 8f;
                }
            });

            return 8f;
        }

        public async Task GetSeadogTimers(string sender)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (this._LastOnlineTimes.ContainsKey(106880697158905856))
                    {
                        if (this.Users.First(USER => USER.Id == 106880697158905856).Status == Discord.UserStatus.Offline)
                        {
                            await GuildManager.SendMsg(sender
                                                                    + ", Seadog is currently offline!\r\n\r\n"
                                                                    + Format.Code("# Last seen online\r\n"
                                                                    + this._LastOnlineTimes[106880697158905856].ToString()
                                                                    + "\r\n\r\n"
                                                                    + "# Average sleep time\r\n"
                                                                    + this.GetAvgSleepTimeAsync(106880697158905856)
                                                                    + "\r\n\r\n"
                                                                    + "# Expected to wake up\r\n"
                                                                    + this._UserPredictedNextLogin(106880697158905856).ToString(), "Markdown"));
                        }
                        else
                        {
                            await GuildManager.SendMsg(sender + ", Seadog is currently online!");
                        }
                    }
                    else
                    {
                        await GuildManager.SendMsg(sender + ", I haven't gathered any information on him yet. I'm sorry!");
                    }
                }
                catch (Exception exc)
                {
                    this.Log(exc.Message);
                }
            });
        }

        public static async Task sGetSeadogTimers(string sender)
        {
            if (_singleton != null)
                await _singleton.GetSeadogTimers(sender);
        }

        public static async Task SetLastOnline(DateTime msg)
        {
            await Task.Run(() =>
            {
                if (_singleton != null)
                    if (_singleton._LastOnlineTimes.ContainsKey(106880697158905856))
                    {
                        _singleton._LastOnlineTimes[106880697158905856] = msg;
                    }
                    else
                    {
                        _singleton._LastOnlineTimes.Add(106880697158905856, msg);
                    }
            });
        }

        private async void Log(string msg)
        {
            await GuildManager.SendMsg("An error occured, please see the log for info.", true);
            Console.WriteLine($"[{DateTime.Now}] Error: {msg}");
        }
    }
}
