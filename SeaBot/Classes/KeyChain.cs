using System;
using System.Collections.Generic;
using System.Text;

namespace SeaBot.Classes
{
    public static class KeyChain
    {
        public static string DBConnectionString
        {
            get { return "Server=seabotdb.crvavxhhmjyl.eu-central-1.rds.amazonaws.com;Port=3306;DataBase=SeabotDB;Uid=root;Pwd=QFSRFdy63u;"; }
        }
    }
}
