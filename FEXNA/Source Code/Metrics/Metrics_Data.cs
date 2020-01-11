using System;
using System.IO;
using System.Net.NetworkInformation;

namespace FEXNA.Metrics
{
    public class Metrics_Data
    {
        private string Chapter;
        private DateTime StartTime, GameplayStartTime;
        private Difficulty_Modes Difficulty;
        private Mode_Styles Style;
        private int PlayTime,
            RankTurns, RankCombat, RankExp, RankCompletion, RankSurvival,
            Deployed, DeployedLvl, Battalion, BattalionLvl;
        private Gameplay_Metrics Gameplay;

        #region Serialization
        internal void write(BinaryWriter writer)
        {
            writer.Write(Chapter);
            writer.Write(StartTime.ToBinary());
            writer.Write((int)Difficulty);
            writer.Write((int)Style);
            writer.Write(PlayTime);
            writer.Write(RankTurns);
            writer.Write(RankCombat);
            writer.Write(RankExp);
            writer.Write(RankCompletion);
            writer.Write(RankSurvival);
            writer.Write(Deployed);
            writer.Write(DeployedLvl);
            writer.Write(Battalion);
            writer.Write(BattalionLvl);

            Version version = Global.RUNNING_VERSION;
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Build);
            writer.Write(version.Revision);

            writer.Write(GameplayStartTime.ToBinary());

            Gameplay.write(writer);

            writer.Write(UserIdentifier);
            writer.Write(DateTime.Now.ToBinary());
            writer.Write(0);
        }
        #endregion

        internal Metrics_Data(Gameplay_Metrics gameplay)
        {
            Chapter = Global.game_system.chapter_id;
            StartTime = Global.game_system.chapter_start_time;
            Difficulty = Global.game_system.Difficulty_Mode;
            Style = Global.game_system.Style;
            PlayTime = Global.game_system.chapter_play_time;
            RankTurns = Global.game_system.chapter_turn;
            RankCombat = Global.game_system.chapter_damage_taken;
            RankExp = Global.game_system.chapter_exp_gain;
            RankCompletion = Global.game_system.chapter_completion;
            RankSurvival = Global.game_system.chapter_deaths;
            Deployed = Global.game_system.deployed_unit_count;
            DeployedLvl = Global.game_system.deployed_unit_avg_level;
            Battalion = Global.battalion.actors.Count;
            BattalionLvl = Global.battalion.average_level;

            GameplayStartTime = Global.game_system.gameplay_start_time;

            Gameplay = gameplay;
        }

        internal string query_string()
        {
            int start_time = (int)(StartTime - new DateTime(1970, 1, 1)).TotalSeconds;
            int gameplay_start_time = (int)(GameplayStartTime - new DateTime(1970, 1, 1)).TotalSeconds;
            string result = string.Format(
                "chapter={0}&starttime={1}&difficulty={2}&style={3}&" +
                "playtime={4}&rankturns={5}&rankcombat={6}&rankexp={7}&" +
                "rankcompletion={8}&ranksurvival={9}&deployed={10}&deployedlvl={11}&" +
                "battalion={12}&battalionlvl={13}&gameplaystarttime={14}",
                Chapter, start_time, (int)Difficulty, (int)Style, PlayTime,
                RankTurns, RankCombat, RankExp, RankCompletion, RankSurvival,
                Deployed, DeployedLvl, Battalion, BattalionLvl, gameplay_start_time);
            return result;
        }

        internal string gameplay_string()
        {
            return Gameplay.query_string();
        }

        public static string UserIdentifier
        {
            get
            {
                return HashString(Environment.UserName + GetMacAddress());
            }
        }

        private static string HashString(string _value)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(_value);
            data = x.ComputeHash(data);
            string ret = "";
            for (int i = 0; i < data.Length; i++) ret += data[i].ToString("x2").ToLower();
            return ret;
        }

        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// </summary>
        /// <returns>The MAC address.</returns>
        internal static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine(
                    "Found MAC Address: " + nic.GetPhysicalAddress() +
                    " Type: " + nic.NetworkInterfaceType);

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    Console.WriteLine("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }
    }
}
