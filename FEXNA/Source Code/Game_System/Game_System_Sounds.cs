using System.Collections.Generic;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif
using FEXNA_Library;

namespace FEXNA
{
    public enum System_Sounds { Open, Confirm, Cancel, Buzzer, Unit_Select, Cursor_Move, Menu_Move1, Menu_Move2,
        Help_Open, Help_Close, Minimap_Open, Minimap_Close, Status_Page_Change, Status_Character_Change, Turn_Change, Chapter_Transition,
        Talk_Boop, Help_Talk_Boop, Worldmap_Talk_Boop, Press_Start, FOW_Surprise, Formation_Change, Gain, Loss, Crowd_Cheer_End,
        HP_Recovery, Exp_Gain, Level_Up_Stat, Support_Gain }
    partial class Game_System
    {
        public void play_se(System_Sounds sound, bool priority = true, Maybe<float> pitch = default(Maybe<float>))
        {
            if (!priority && Global.Audio.playing_system_sound())
                return;

            string sound_name;
            switch (sound)
            {
                case System_Sounds.Open:
                    sound_name = "Open";
                    break;
                case System_Sounds.Confirm:
                    sound_name = "Confirm";
                    break;
                case System_Sounds.Cancel:
                    sound_name = "Cancel";
                    break;
                case System_Sounds.Buzzer:
                    sound_name = "Buzzer";
                    break;
                case System_Sounds.Unit_Select:
                    sound_name = "Unit_Select";
                    break;
                case System_Sounds.Cursor_Move:
                    sound_name = "Cursor_Move";
                    break;
                case System_Sounds.Menu_Move1:
                    sound_name = "Menu_Move1";
                    break;
                case System_Sounds.Menu_Move2:
                    sound_name = "Menu_Move2";
                    break;
                case System_Sounds.Help_Open:
                    sound_name = "Help_Open";
                    break;
                case System_Sounds.Help_Close:
                    sound_name = "Help_Close";
                    break;
                case System_Sounds.Minimap_Open:
                    sound_name = "Minimap_Open";
                    break;
                case System_Sounds.Minimap_Close:
                    sound_name = "Minimap_Close";
                    break;
                case System_Sounds.Status_Page_Change:
                    sound_name = "Status_Page_Change";
                    break;
                case System_Sounds.Status_Character_Change:
                    sound_name = "Status_Character_Change";
                    break;
                case System_Sounds.Turn_Change:
                    sound_name = "Turn_Change";
                    break;
                case System_Sounds.Chapter_Transition:
                    sound_name = "Chapter Transition";
                    break;
                case System_Sounds.Talk_Boop:
                    sound_name = "Talk_Boop";
                    break;
                case System_Sounds.Help_Talk_Boop:
                    sound_name = "Help_Talk_Boop";
                    break;
                case System_Sounds.Worldmap_Talk_Boop:
                    sound_name = "Worldmap_Talk_Boop";
                    break;
                case System_Sounds.Press_Start:
                    sound_name = "Press_Start";
                    break;
                case System_Sounds.FOW_Surprise:
                    sound_name = "FOW_Surprise";
                    break;
                case System_Sounds.Formation_Change:
                    sound_name = "Formation_Change";
                    break;
                case System_Sounds.Gain:
                    Global.Audio.play_se("System Sounds", "Gain", pitch, duckBgm: true); // Inconsistency~ //Debug
                    return;
                case System_Sounds.Loss:
                    Global.Audio.play_se("System Sounds", "Loss", pitch, duckBgm: true); // Inconsistency~ //Debug
                    return;
                case System_Sounds.Crowd_Cheer_End:
                    Global.Audio.play_system_se("Battle Sounds", "Crowd_Cheer_End", priority, pitch); // Inconsistency~ //Debug
                    return;
                case System_Sounds.HP_Recovery:
                    sound_name = "HP_Recovery";
                    break;
                case System_Sounds.Exp_Gain:
                    sound_name = "Exp_Gain";
                    break;
                case System_Sounds.Level_Up_Stat:
                    sound_name = "Level_Up_Stat";
                    break;
                case System_Sounds.Support_Gain:
                    sound_name = "Support_Gain";
                    break;
                default:
                    return;
            }
            play_se(sound_name, priority, pitch);
            //Global.Audio.play_system_se("System Sounds", sound_name, priority); //Debug
        }
        internal void play_system_se(string sound)
        {
            play_se(sound);
        }
        private void play_se(string sound,
            bool priority = true,
            Maybe<float> pitch = default(Maybe<float>))
        {
            Global.Audio.play_system_se("System Sounds", sound, priority, pitch);
        }

        public void cancel_sound()
        {
            Global.Audio.cancel_system_sound();
        }
    }
}
