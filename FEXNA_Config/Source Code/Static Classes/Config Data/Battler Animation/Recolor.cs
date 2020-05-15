using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA
{
    enum Color_Keys { Main1, Main2, Main3, Main4, Main5, Main6, Main7, Scnd1, Scnd2, Scnd3, Scnd4, Tert1, Tert2, Tert3, Tert4, Hair1, Hair2, Hair3,
        Flap1, Drag1, Drag2, Drag3, Drag4, Trim1, Trim2, Trim3, Llnc1, Llnc2, Llnc3 }
    public class Recolor
    {
        #region Country Color Data
        readonly static Dictionary<string, Dictionary<Color_Keys, Color>> COUNTRY_COLORS = new Dictionary<string, Dictionary<Color_Keys, Color>>
        {
            #region Bern
            { "Bern", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(56, 16, 128) }, // Main color
                    { Color_Keys.Main2, new Color(80, 32, 160) },
                    { Color_Keys.Main3, new Color(112, 48, 184) },
                    { Color_Keys.Main4, new Color(160, 72, 248) },
                    { Color_Keys.Main5, new Color(184, 104, 248) },
                    { Color_Keys.Main6, new Color(208, 152, 248) },
                    { Color_Keys.Main7, new Color(240, 208, 248) },
                    { Color_Keys.Scnd1, new Color(112, 80, 32) }, // Secondary
                    { Color_Keys.Scnd2, new Color(200, 152, 32) },
                    { Color_Keys.Scnd3, new Color(248, 216, 56) },
                    { Color_Keys.Scnd4, new Color(248, 240, 136) },
                    { Color_Keys.Tert1, new Color(80, 88, 96) }, // Tertiary
                    { Color_Keys.Tert2, new Color(136, 144, 144) },
                    { Color_Keys.Tert3, new Color(192, 200, 200) },
                    { Color_Keys.Hair1, new Color(176, 96, 16) }, // Hair
                    { Color_Keys.Hair2, new Color(232, 160, 48) },
                    { Color_Keys.Hair3, new Color(248, 192, 112) },
                    { Color_Keys.Flap1, new Color(184, 144, 32) }, // General flap/Paladin shield
                    { Color_Keys.Drag1, new Color(104, 64, 40) }, // Dragon
                    { Color_Keys.Drag2, new Color(184, 112, 48) },
                    { Color_Keys.Drag3, new Color(224, 168, 80) },
                    { Color_Keys.Drag4, new Color(232, 208, 152) },
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(104, 40, 128) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(184, 96, 192) },
                    { Color_Keys.Llnc3, new Color(232, 240, 216) }
                }
            },
            #endregion
            #region Etruria
            { "Etruria", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(56, 56, 48) }, // Main color
                    { Color_Keys.Main2, new Color(72, 72, 64) },
                    { Color_Keys.Main3, new Color(104, 96, 88) },
                    { Color_Keys.Main4, new Color(144, 136, 120) },
                    { Color_Keys.Main5, new Color(184, 176, 160) },
                    { Color_Keys.Main6, new Color(216, 208, 200) },
                    { Color_Keys.Main7, new Color(240, 232, 224) },
                    { Color_Keys.Scnd1, new Color(112, 80, 32) }, // Secondary
                    { Color_Keys.Scnd2, new Color(200, 152, 32) },
                    { Color_Keys.Scnd3, new Color(248, 216, 56) },
                    { Color_Keys.Scnd4, new Color(248, 240, 136) },
                    { Color_Keys.Tert1, new Color(104, 112, 120) }, // Tertiary
                    { Color_Keys.Tert2, new Color(160, 168, 168) },
                    { Color_Keys.Tert3, new Color(216, 224, 224) },
                    { Color_Keys.Hair1, new Color(144, 112, 32) },//{ Color_Keys.Hair1, new Color(176, 144, 16) }, // Hair
                    { Color_Keys.Hair2, new Color(216, 176, 32) },//{ Color_Keys.Hair2, new Color(224, 184, 24) },
                    { Color_Keys.Hair3, new Color(248, 248, 80) },
                    { Color_Keys.Flap1, new Color(184, 144, 32) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(72, 72, 56) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(128, 128, 104) },
                    { Color_Keys.Llnc3, new Color(232, 240, 216) }
                }
            },
            #endregion
            #region Sacae
            { "Sacae", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(96, 40, 32) }, // Main color
                    { Color_Keys.Main2, new Color(120, 64, 40) },
                    { Color_Keys.Main3, new Color(144, 80, 48) },
                    { Color_Keys.Main4, new Color(176, 104, 56) },
                    { Color_Keys.Main5, new Color(208, 144, 88) },//[232, 184, 128) },
                    { Color_Keys.Main6, new Color(224, 176, 120) },
                    { Color_Keys.Main7, new Color(240, 224, 176) },
                    { Color_Keys.Scnd1, new Color(72, 96, 24) }, // Secondary
                    { Color_Keys.Scnd2, new Color(120, 144, 32) },
                    { Color_Keys.Scnd3, new Color(192, 208, 80) },
                    { Color_Keys.Scnd4, new Color(224, 232, 120) },
                    { Color_Keys.Tert1, new Color(184, 128, 32) }, // Tertiary
                    { Color_Keys.Tert2, new Color(232, 160, 64) },
                    { Color_Keys.Tert3, new Color(248, 216, 144) },
                    { Color_Keys.Hair1, new Color(40, 104, 48) }, // Hair
                    { Color_Keys.Hair2, new Color(64, 176, 88) },
                    { Color_Keys.Hair3, new Color(120, 216, 144) },
                    { Color_Keys.Flap1, new Color(120, 144, 32) } // General flap/Paladin shield
                }
            },
            #endregion
            #region Western
            { "Western", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(104, 24, 40) }, // Main color
                    { Color_Keys.Main2, new Color(136, 24, 32) },
                    { Color_Keys.Main3, new Color(184, 32, 32) },//[176, 24, 32) },
                    { Color_Keys.Main4, new Color(224, 72, 56) },//[232, 56, 32) },
                    { Color_Keys.Main5, new Color(240, 104, 96) },
                    { Color_Keys.Main6, new Color(240, 152, 136) },
                    { Color_Keys.Main7, new Color(248, 208, 200) },
                    { Color_Keys.Scnd1, new Color(80, 56, 56) }, // Secondary
                    { Color_Keys.Scnd2, new Color(112, 80, 80) },
                    { Color_Keys.Scnd3, new Color(144, 112, 104) },
                    { Color_Keys.Scnd4, new Color(184, 152, 144) },
                    { Color_Keys.Tert1, new Color(112, 120, 136) }, // Tertiary
                    { Color_Keys.Tert2, new Color(168, 176, 176) },
                    { Color_Keys.Tert3, new Color(224, 232, 232) },
                    { Color_Keys.Hair1, new Color(96, 72, 32) }, // Hair
                    { Color_Keys.Hair2, new Color(144, 104, 48) },
                    { Color_Keys.Hair3, new Color(184, 136, 64) },
                    { Color_Keys.Flap1, new Color(112, 80, 80) } // General flap/Paladin shield
                }
            },
            #endregion
            #region Ilia
            { "Ilia", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(56, 56, 96) }, // Main color
                    { Color_Keys.Main2, new Color(72, 80, 144) },
                    { Color_Keys.Main3, new Color(88, 104, 176) },
                    { Color_Keys.Main4, new Color(112, 136, 200) },
                    { Color_Keys.Main5, new Color(136, 160, 216) },
                    { Color_Keys.Main6, new Color(168, 184, 232) },
                    { Color_Keys.Main7, new Color(216, 232, 248) },
                    { Color_Keys.Scnd1, new Color(64, 48, 88) }, // Secondary
                    { Color_Keys.Scnd2, new Color(112, 64, 176) },
                    { Color_Keys.Scnd3, new Color(152, 104, 200) },
                    { Color_Keys.Scnd4, new Color(200, 184, 232) },
                    { Color_Keys.Tert1, new Color(112, 120, 136) }, // Tertiary
                    { Color_Keys.Tert2, new Color(168, 176, 176) },
                    { Color_Keys.Tert3, new Color(224, 232, 232) },
                    { Color_Keys.Hair1, new Color(32, 56, 136) }, // Hair
                    { Color_Keys.Hair2, new Color(56, 80, 192) },
                    { Color_Keys.Hair3, new Color(80, 112, 232) },
                    { Color_Keys.Flap1, new Color(112, 64, 176) } // General flap/Paladin shield
                }
            },
            #endregion
            #region Bandit
            { "Bandit", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(80, 56, 48) }, // Main color
                    { Color_Keys.Main2, new Color(104, 72, 56) },
                    { Color_Keys.Main3, new Color(120, 88, 72) },
                    { Color_Keys.Main4, new Color(144, 112, 88) },
                    { Color_Keys.Main5, new Color(152, 136, 112) },
                    { Color_Keys.Main6, new Color(176, 168, 144) },
                    { Color_Keys.Main7, new Color(224, 216, 176) },
                    { Color_Keys.Scnd1, new Color(104, 64, 40) }, // Secondary
                    { Color_Keys.Scnd2, new Color(168, 120, 64) },
                    { Color_Keys.Scnd3, new Color(208, 176, 96) },
                    { Color_Keys.Scnd4, new Color(224, 208, 160) },
                    { Color_Keys.Tert1, new Color(72, 80, 88) }, // Tertiary
                    { Color_Keys.Tert2, new Color(120, 128, 128) },
                    { Color_Keys.Tert3, new Color(176, 184, 184) },
                    { Color_Keys.Hair1, new Color(88, 64, 48) }, // Hair
                    { Color_Keys.Hair2, new Color(128, 104, 64) },
                    { Color_Keys.Hair3, new Color(152, 136, 96) },
                    { Color_Keys.Flap1, new Color(168, 120, 64) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(104, 72, 48) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(152, 120, 80) },
                    { Color_Keys.Llnc3, new Color(224, 216, 184) }
                }
            },
            #endregion
            #region Ostia
            { "Ostia", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(40, 32, 120) }, // Main color
                    { Color_Keys.Main2, new Color(40, 40, 168) },
                    { Color_Keys.Main3, new Color(56, 64, 240) },
                    { Color_Keys.Main4, new Color(72, 96, 248) },
                    { Color_Keys.Main5, new Color(104, 136, 248) },
                    { Color_Keys.Main6, new Color(128, 176, 248) },
                    { Color_Keys.Main7, new Color(192, 224, 248) },
                    { Color_Keys.Scnd1, new Color(80, 88, 80) }, // Secondary
                    { Color_Keys.Scnd2, new Color(112, 128, 104) },
                    { Color_Keys.Scnd3, new Color(184, 192, 160) },
                    { Color_Keys.Scnd4, new Color(248, 248, 240) },
                    { Color_Keys.Tert1, new Color(24, 32, 144) }, // Tertiary
                    { Color_Keys.Tert2, new Color(40, 104, 224) },
                    { Color_Keys.Tert3, new Color(112, 200, 232) },
                    { Color_Keys.Tert4, new Color(184, 224, 232) },
                    { Color_Keys.Hair1, new Color(48, 72, 184) }, // Hair
                    { Color_Keys.Hair2, new Color(72, 112, 224) },
                    { Color_Keys.Hair3, new Color(128, 160, 240) },
                    { Color_Keys.Flap1, new Color(112, 128, 104) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(56, 48, 136) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(104, 112, 184) },
                    { Color_Keys.Llnc3, new Color(232, 240, 216) }
                }
            },
            #endregion
            #region Laus
            { "Laus", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(120, 16, 72) }, // Main color
                    { Color_Keys.Main2, new Color(136, 40, 88) },
                    { Color_Keys.Main3, new Color(168, 56, 104) },
                    { Color_Keys.Main4, new Color(200, 80, 120) },
                    { Color_Keys.Main5, new Color(216, 112, 136) },
                    { Color_Keys.Main6, new Color(240, 152, 160) },
                    { Color_Keys.Main7, new Color(248, 216, 216) },
                    { Color_Keys.Scnd1, new Color(112, 64, 40) }, // Secondary
                    { Color_Keys.Scnd2, new Color(184, 112, 48) },
                    { Color_Keys.Scnd3, new Color(224, 168, 96) },
                    { Color_Keys.Scnd4, new Color(232, 216, 168) },
                    { Color_Keys.Tert1, new Color(24, 32, 144) }, // Tertiary
                    { Color_Keys.Tert2, new Color(40, 104, 224) },
                    { Color_Keys.Tert3, new Color(112, 200, 232) },
                    { Color_Keys.Tert4, new Color(184, 224, 232) },
                    { Color_Keys.Hair1, new Color(88, 72, 48) }, // Hair
                    { Color_Keys.Hair2, new Color(144, 120, 64) },
                    { Color_Keys.Hair3, new Color(192, 176, 88) },
                    { Color_Keys.Flap1, new Color(184, 112, 48) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) }
                }
            },
            #endregion
            #region Thria
            { "Thria", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(16, 72, 112) }, // Main color
                    { Color_Keys.Main2, new Color(32, 96, 136) },
                    { Color_Keys.Main3, new Color(48, 112, 160) },
                    { Color_Keys.Main4, new Color(56, 144, 184) },
                    { Color_Keys.Main5, new Color(80, 184, 200) },
                    { Color_Keys.Main6, new Color(136, 208, 200) },
                    { Color_Keys.Main7, new Color(184, 224, 216) },
                    { Color_Keys.Scnd1, new Color(80, 88, 80) }, // Secondary
                    { Color_Keys.Scnd2, new Color(112, 128, 104) },
                    { Color_Keys.Scnd3, new Color(184, 192, 160) },
                    { Color_Keys.Scnd4, new Color(248, 248, 240) },
                    { Color_Keys.Tert1, new Color(24, 32, 144) }, // Tertiary
                    { Color_Keys.Tert2, new Color(40, 104, 224) },
                    { Color_Keys.Tert3, new Color(112, 200, 232) },
                    { Color_Keys.Tert4, new Color(184, 224, 232) },
                    { Color_Keys.Hair1, new Color(64, 64, 160) }, // Hair
                    { Color_Keys.Hair2, new Color(112, 104, 208) },
                    { Color_Keys.Hair3, new Color(152, 144, 232) },
                    { Color_Keys.Flap1, new Color(112, 128, 104) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) }
                }
            },
            #endregion
            #region Tania
            { "Tania", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(112, 48, 16) }, // Main color
                    { Color_Keys.Main2, new Color(144, 72, 24) },
                    { Color_Keys.Main3, new Color(192, 96, 32) },
                    { Color_Keys.Main4, new Color(240, 136, 40) },
                    { Color_Keys.Main5, new Color(248, 176, 72) },
                    { Color_Keys.Main6, new Color(248, 216, 128) },
                    { Color_Keys.Main7, new Color(248, 240, 184) },
                    { Color_Keys.Scnd1, new Color(56, 72, 104) }, // Secondary
                    { Color_Keys.Scnd2, new Color(72, 128, 152) },
                    { Color_Keys.Scnd3, new Color(136, 208, 192) },
                    { Color_Keys.Scnd4, new Color(232, 248, 240) },
                    { Color_Keys.Tert1, new Color(24, 32, 144) }, // Tertiary
                    { Color_Keys.Tert2, new Color(40, 104, 224) },
                    { Color_Keys.Tert3, new Color(112, 200, 232) },
                    { Color_Keys.Tert4, new Color(184, 224, 232) },
                    { Color_Keys.Hair1, new Color(144, 104, 32) }, // Hair
                    { Color_Keys.Hair2, new Color(208, 168, 40) },
                    { Color_Keys.Hair3, new Color(232, 224, 104) },
                    { Color_Keys.Flap1, new Color(72, 128, 152) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) }
                }
            },
            #endregion
            #region Rebel
            { "Rebel", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(80, 48, 56) }, // Main color
                    { Color_Keys.Main2, new Color(104, 56, 64) },
                    { Color_Keys.Main3, new Color(144, 72, 72) },
                    { Color_Keys.Main4, new Color(176, 88, 80) },
                    { Color_Keys.Main5, new Color(192, 112, 96) },
                    { Color_Keys.Main6, new Color(200, 144, 128) },
                    { Color_Keys.Main7, new Color(232, 208, 200) },
                    { Color_Keys.Scnd1, new Color(104, 64, 56) }, // Secondary
                    { Color_Keys.Scnd2, new Color(168, 120, 64) },
                    { Color_Keys.Scnd3, new Color(208, 176, 96) },
                    { Color_Keys.Scnd4, new Color(224, 208, 160) },
                    { Color_Keys.Tert1, new Color(64, 72, 104) }, // Tertiary
                    { Color_Keys.Tert2, new Color(104, 128, 168) },
                    { Color_Keys.Tert3, new Color(144, 184, 200) },
                    { Color_Keys.Tert4, new Color(192, 216, 224) },
                    { Color_Keys.Hair1, new Color(112, 32, 32) }, // Hair
                    { Color_Keys.Hair2, new Color(160, 48, 40) },
                    { Color_Keys.Hair3, new Color(208, 72, 56) },
                    { Color_Keys.Flap1, new Color(168, 120, 64) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) }
                }
            },
            #endregion
            #region Member
            { "Member", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(80, 64, 72) }, // Main color
                    { Color_Keys.Main2, new Color(112, 88, 88) },
                    { Color_Keys.Main3, new Color(144, 112, 112) },
                    { Color_Keys.Main4, new Color(176, 136, 128) },
                    { Color_Keys.Main5, new Color(192, 160, 144) },
                    { Color_Keys.Main6, new Color(216, 192, 176) },
                    { Color_Keys.Main7, new Color(240, 232, 224) },
                    { Color_Keys.Scnd1, new Color(136, 72, 40) }, // Secondary
                    { Color_Keys.Scnd2, new Color(192, 104, 40) },
                    { Color_Keys.Scnd3, new Color(224, 152, 80) },
                    { Color_Keys.Scnd4, new Color(232, 192, 136) },
                    { Color_Keys.Tert1, new Color(104, 88, 32) }, // Tertiary
                    { Color_Keys.Tert2, new Color(184, 160, 40) },
                    { Color_Keys.Tert3, new Color(224, 224, 96) },
                    { Color_Keys.Hair1, new Color(144, 104, 32) },//176, 112, 16) }, // Hair
                    { Color_Keys.Hair2, new Color(208, 168, 40) },//224, 184, 56) },
                    { Color_Keys.Hair3, new Color(232, 224, 104) },//240, 232, 104) },
                    { Color_Keys.Flap1, new Color(192, 104, 40) } // General flap/Paladin shield
                }
            },
            #endregion
            #region Killer
            { "Killer", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(88, 56, 24) }, // Main color
                    { Color_Keys.Main2, new Color(120, 80, 32) },
                    { Color_Keys.Main3, new Color(144, 104, 40) },
                    { Color_Keys.Main4, new Color(176, 136, 72) },
                    { Color_Keys.Main5, new Color(200, 160, 104) },
                    { Color_Keys.Main6, new Color(216, 184, 128) },
                    { Color_Keys.Main7, new Color(232, 216, 168) },

                    /*{ Color_Keys.Main1, new Color(64, 56, 40) }, // Main color
                    { Color_Keys.Main2, new Color(80, 72, 56) },
                    { Color_Keys.Main3, new Color(112, 96, 80) },
                    { Color_Keys.Main4, new Color(152, 136, 112) },
                    { Color_Keys.Main5, new Color(192, 176, 152) },
                    { Color_Keys.Main6, new Color(224, 208, 192) },
                    { Color_Keys.Main7, new Color(240, 232, 216) },*/
                    
                    { Color_Keys.Scnd1, new Color(104, 64, 40) }, // Secondary
                    { Color_Keys.Scnd2, new Color(168, 120, 64) },
                    { Color_Keys.Scnd3, new Color(208, 176, 96) },
                    { Color_Keys.Scnd4, new Color(224, 208, 160) },

                    { Color_Keys.Tert1, new Color(104, 88, 32) }, // Tertiary
                    { Color_Keys.Tert2, new Color(184, 160, 40) },
                    { Color_Keys.Tert3, new Color(224, 224, 96) },
                    { Color_Keys.Tert4, new Color(240, 240, 184) },
                    { Color_Keys.Hair1, new Color(96, 48, 48) }, // Hair
                    { Color_Keys.Hair2, new Color(136, 64, 56) },
                    { Color_Keys.Hair3, new Color(184, 88, 72) },
                    { Color_Keys.Flap1, new Color(192, 104, 40) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(72, 72, 56) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(128, 128, 104) },
                    { Color_Keys.Llnc3, new Color(232, 240, 216) }
                }
            },
            #endregion
            #region Inmate
            { "Inmate", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(56, 56, 48) }, // Main color
                    { Color_Keys.Main2, new Color(72, 72, 64) },
                    { Color_Keys.Main3, new Color(104, 96, 88) },
                    { Color_Keys.Main4, new Color(144, 136, 120) },
                    { Color_Keys.Main5, new Color(184, 176, 160) },
                    { Color_Keys.Main6, new Color(216, 208, 200) },
                    { Color_Keys.Main7, new Color(240, 232, 224) },
                    { Color_Keys.Scnd1, new Color(112, 64, 40) }, // Secondary
                    { Color_Keys.Scnd2, new Color(168, 88, 48) },
                    { Color_Keys.Scnd3, new Color(216, 128, 72) },
                    { Color_Keys.Scnd4, new Color(224, 184, 136) },
                    { Color_Keys.Tert1, new Color(144, 64, 48) }, // Tertiary
                    { Color_Keys.Tert2, new Color(192, 104, 56) },
                    { Color_Keys.Tert3, new Color(216, 152, 104) },
                    { Color_Keys.Hair1, new Color(88, 72, 56) }, // Hair
                    { Color_Keys.Hair2, new Color(144, 112, 64) },
                    { Color_Keys.Hair3, new Color(184, 160, 88) },
                    { Color_Keys.Flap1, new Color(184, 144, 32) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(72, 72, 56) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(128, 128, 104) },
                    { Color_Keys.Llnc3, new Color(232, 240, 216) }
                }
            },
            #endregion
            #region Gladiator
            { "Gladiator", new Dictionary<Color_Keys, Color>
                {
                    { Color_Keys.Main1, new Color(24, 64, 56) }, // Main color
                    { Color_Keys.Main2, new Color(40, 88, 64) },
                    { Color_Keys.Main3, new Color(48, 120, 56) },
                    { Color_Keys.Main4, new Color(64, 160, 72) },
                    { Color_Keys.Main5, new Color(88, 200, 88) },
                    { Color_Keys.Main6, new Color(144, 208, 112) },
                    { Color_Keys.Main7, new Color(216, 232, 168) },
                    { Color_Keys.Scnd1, new Color(80, 88, 80) }, // Secondary
                    { Color_Keys.Scnd2, new Color(112, 128, 104) },
                    { Color_Keys.Scnd3, new Color(184, 192, 160) },
                    { Color_Keys.Scnd4, new Color(248, 248, 240) },
                    { Color_Keys.Tert1, new Color(72, 80, 88) }, // Tertiary
                    { Color_Keys.Tert2, new Color(120, 128, 128) },
                    { Color_Keys.Tert3, new Color(176, 184, 184) },
                    { Color_Keys.Hair1, new Color(88, 64, 48) }, // Hair
                    { Color_Keys.Hair2, new Color(128, 104, 64) },
                    { Color_Keys.Hair3, new Color(152, 136, 96) },
                    { Color_Keys.Flap1, new Color(112, 128, 104) }, // General flap/Paladin shield
                    { Color_Keys.Trim1, new Color(144, 80, 16) }, // Trim for Sorc/Warlock/etc
                    { Color_Keys.Trim2, new Color(240, 160, 48) },
                    { Color_Keys.Trim3, new Color(248, 240, 48) },
                    { Color_Keys.Llnc1, new Color(96, 88, 64) }, // Lieutenant lance/trim
                    { Color_Keys.Llnc2, new Color(136, 136, 96) },
                    { Color_Keys.Llnc3, new Color(216, 224, 192) }
                }
            }
            #endregion
        };
        #endregion

        // Country colors, in order of standardization: //Debug
        // Gladiator, Bandit, Western
        // Laus, Rebel, Tania (about as standard as above other than weirdness inherited from ostia)
        // Sacae
        // Bern, Etruria, Inmate
        // Ostia, Thria (saturated blue is hard to standardize, as is secondary being white)
        // Member, Ilia

        public readonly static Dictionary<string, string> SPRITE_RENAME_LIST = new Dictionary<string, string>
        {
            { "Tory_P", "Bern" },
            { "Steers", "Pirate" },
            { "Kris", "Etruria" },
            { "James", "Etruria" },
            { "Caphel", "Etruria" },
            { "Gavrilo", "Etruria" },
            { "Haster", "Etruria" },
            
            { "Vak", "Bandit" },
            { "Connor", "Ostia" },

            { "Alec", "Member" },
            { "Stephen", "Member" },
            
            { "Etruria_Hemda", "Etruria" },
            { "Merc_Bern", "Bern" },
            { "Merc_Ilia", "Ilia" },
            { "Merc_IliaM", "Ilia" },
            { "Merc_Bern2", "Bern" },
            { "Merc_Etruria", "Etruria" },
            { "Merc_Rebel", "Rebel" },
            { "Merc_Sacae", "Sacae" },
            { "Merc_Western", "Western" },
            { "Junior_Oldest", "Ostia" },
            { "Junior_Middle", "Ostia" },
            { "Junior_Youngest", "Ostia" },
            { "Citizen_Etruria", "Etruria" },
            { "Bandit_Falsaron", "Bandit" },
            { "Pirate", "Bandit" },
            { "Pirate_Rebel", "Rebel" },
            { "Traitor", "Ostia" },
            { "Immortal", "Bern" }
        };

        public readonly static Dictionary<string, string> RAMP_DEFAULT_OTHER_NAMES = new Dictionary<string, string>
        {
            { "Armor", "Main" },
            { "Clothes", "Main" },
            { "Assassin Main", "Main" },
            { "Cleric Robes", "Main" },
            { "Justice Main", "Main" },
            { "Nomad Main", "Main" },
            { "Rogue Main", "Main" },
            { "Valkyrie Mane", "Main" },

            { "Paladin Shield", "Secondary" },
            { "General Flaps", "Secondary" },
            { "Nomad Clothes", "Secondary" },
            { "Bishop Staff", "Secondary" },
            { "Draco", "Secondary" },
            { "Lieutenant Trim", "Secondary" },

            { "Assassin Tertiary", "Tertiary" },
            { "Justice Chainmail", "Tertiary" },
            { "Ribbon", "Tertiary" },
            { "Scholar Pages", "Tertiary" },
            { "SageF Trim", "Tertiary" },
            { "Draco Secondary", "Tertiary" },

            { "Bishop Shawl", "Corsair Shirt" },
            { "Warlock Trim", "Gold Trim" },

            { "Nomad Hair", "Hair" },
        };

        static Dictionary<string, List<Color>> Recolor_Data = new Dictionary<string, List<Color>>();
        public static Dictionary<string, List<Color>> data { get { return Recolor_Data; } }

        /*public static void add_recolor(string name, Texture2D texture) //Yeti
        {
            if (texture.Width < 16)
                return;

            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(data);
            Recolor_Data.Add(name, new List<Color>());
            for (int x = 0; x < 16; x++)
                if (data[x].A > 0)
                    Recolor_Data[name].Add(data[x]);
        }*/

        public static bool has_key(int key, int gender, string name)
        {
            return get_color(key, gender, name, 0) != null;
        }

        public static Color? get_color(int key, int gender, string name, int color)
        {
            return get_color(key, gender, name, color, false);
        }
        protected static Color? get_color(int key, int gender, string name, int color, bool key_test)
        {
            for (int i = 0; i < 10; i++)
            {
                if (SPRITE_RENAME_LIST.ContainsKey(name))
                    name = SPRITE_RENAME_LIST[name];
                else
                    break;
            }
            bool key_found = true;
            try
            { 
            switch (key)
            {
                #region 3: Recruit
                case 3:
                    switch (name)
                    {
                        case "Marcus":
                            switch (color)
                            {
                                case 0: return new Color(120, 48, 24); // Main color
                                case 1: return new Color(208, 112, 40);
                                case 2: return new Color(224, 168, 96);
                                case 3: return new Color(88, 40, 112); // Hair
                                case 4: return new Color(168, 88, 176);
                                case 5: return new Color(224, 144, 208);
                            }
                            break;
                    }
                    break;
                #endregion
                #region 6: Journeyman
                case 6:
                    switch (name)
                    {
                        case "Shen":
                            switch (color)
                            {
                                case 0: return new Color(88, 24, 32); // Clothes
                                case 1: return new Color(144, 32, 24);
                                case 2: return new Color(208, 64, 40);
                                case 3: return new Color(80, 64, 48); // Hair
                                case 4: return new Color(128, 96, 64);
                                case 5: return new Color(160, 128, 80);
                            }
                            break;
                    }
                    break;
                #endregion
                #region 8: Page
                case 8:
                    switch (name)
                    {
                        case "Magnus":
                            switch (color)
                            {
                                case 0: return new Color(136, 112, 88); // Cloak
                                case 1: return new Color(184, 160, 120);
                                case 2: return new Color(224, 216, 176);
                                case 3: return new Color(32, 104, 32); // Clothes
                                case 4: return new Color(72, 152, 64);
                                case 5: return new Color(120, 208, 88);
                                case 6: return new Color(64, 56, 56); // Hair
                                case 7: return new Color(96, 88, 80);
                                case 8: return new Color(144, 128, 112);
                            }
                            break;
                    }
                    break;
                #endregion

                #region 16: Myrmidon
                case 16:
                    switch (name)
                    {
                        case "Lloyd":
                            switch (color)
                            {
                                case 0: return new Color(56, 72, 16); // Main color
                                case 1: return new Color(96, 120, 40);
                                case 2: return new Color(168, 200, 80);
                                case 3: return new Color(208, 216, 136);
                                case 4: return new Color(144, 96, 24); // Hair
                                case 5: return new Color(208, 176, 32);
                                case 6: return new Color(232, 232, 72);
                            }
                            break;
                        case "Edeleisse":
                            switch (color)
                            {
                                case 0: return new Color(40, 40, 64);
                                case 1: return new Color(64, 72, 128);
                                case 2: return new Color(88, 128, 184);
                                case 3: return new Color(144, 184, 208);
                                case 4: return new Color(72, 48, 80);
                                case 5: return new Color(112, 80, 112);
                                case 6: return new Color(176, 128, 160);

                                case 8: return new Color(88, 120, 152); // 80 112 136?
                                case 9: return new Color(136, 168, 176); // 112 144 160?
                                case 10: return new Color(184, 200, 200); // 168 184 184?
                                case 11: return new Color(216, 224, 208); // 216 216 192?

                                case 12: return new Color(88, 56, 48); // 72 56 48?
                                case 13: return new Color(152, 120, 96); // 144 112 88?
                                case 14: return new Color(200, 176, 152); // 192 160 128?
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 17: Thief
                case 17:
                    switch (name)
                    {
                        case "Leonard":
                            switch (color)
                            {
                                case 0: return new Color(104, 96, 0); // Cloak
                                case 1: return new Color(168, 160, 0);
                                case 2: return new Color(224, 224, 32);
                                case 3: return new Color(88, 24, 192); // Pants
                                case 4: return new Color(144, 88, 216);
                                case 5: return new Color(40, 96, 96); // Hair
                                case 6: return new Color(72, 176, 160);
                                case 7: return new Color(120, 200, 192);

                                case 8: return new Color(40, 40, 40); // Outline
                                case 9: return new Color(64, 96, 128); // Shirt/sword
                                case 10: return new Color(120, 152, 184);
                                case 11: return new Color(176, 208, 240);
                                case 12: return new Color(248, 248, 248);
                                case 13: return new Color(248, 184, 128); // Skin
                                case 14: return new Color(248, 248, 208);
                            }
                            break;
                        case "Melanie":
                            switch (color)
                            {
                                case 0: return new Color(72, 80, 152); // Cloak
                                case 1: return new Color(96, 112, 192);
                                case 2: return new Color(136, 160, 240);
                                case 3: return new Color(96, 152, 144); // Leggings
                                case 4: return new Color(160, 216, 200);
                                case 5: return new Color(104, 56, 152); // Hair
                                case 6: return new Color(168, 88, 224);
                                case 7: return new Color(208, 152, 240);

                                case 8: return new Color(40, 40, 40); // Outline
                                case 9: return new Color(64, 96, 128); // Shirt/sword
                                case 10: return new Color(120, 152, 184);
                                case 11: return new Color(176, 208, 240);
                                case 12: return new Color(248, 248, 248);
                                case 13: return new Color(248, 184, 128); // Skin
                                case 14: return new Color(248, 248, 208);
                            }
                            break;
                        case "Matthew":
                            switch (color)
                            {
                                case 0: return new Color(128, 48, 16);
                                case 1: return new Color(184, 64, 48);
                                case 2: return new Color(248, 80, 72);
                                case 3: return new Color(40, 96, 136);
                                case 4: return new Color(80, 176, 176);
                                case 5: return new Color(184, 96, 8);
                                case 6: return new Color(248, 176, 88);
                                case 7: return new Color(248, 232, 104);

                                case 8: return new Color(40, 40, 40);
                                case 9: return new Color(64, 96, 128);
                                case 10: return new Color(120, 152, 184);
                                case 11: return new Color(176, 208, 240);
                                case 12: return new Color(248, 248, 248);
                                case 13: return new Color(248, 184, 128);
                                case 14: return new Color(248, 248, 208);
                            }
                            break;
                        case "Ilia":
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair3];

                                case 8: return new Color(40, 40, 40);
                                case 9: return new Color(64, 96, 128);
                                case 10: return new Color(120, 152, 184);
                                case 11: return new Color(176, 208, 240);
                                case 12: return new Color(248, 248, 248);
                                case 13: return new Color(248, 184, 128);
                                case 14: return new Color(248, 248, 208);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair3];

                                    case 8: return new Color(40, 40, 40);
                                    case 9: return new Color(64, 96, 128);
                                    case 10: return new Color(120, 152, 184);
                                    case 11: return new Color(176, 208, 240);
                                    case 12: return new Color(248, 248, 248);
                                    case 13: return new Color(248, 184, 128);
                                    case 14: return new Color(248, 248, 208);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 19: Archer
                case 19:
                    switch (name)
                    {
                        case "Toni":
                            switch (color)
                            {
                                case 0: return new Color(112, 48, 16); // Main color
                                case 1: return new Color(192, 88, 8);
                                case 2: return new Color(232, 144, 48);
                                case 3: return new Color(240, 200, 120);
                                case 4: return new Color(208, 176, 120); // Quiver
                                case 5: return new Color(248, 232, 160);
                                case 6: return new Color(32, 96, 48); // Hair
                                case 7: return new Color(32, 144, 64);
                                case 8: return new Color(80, 200, 112);
                            }
                            break;
                        case "Louise":
                            switch (color)
                            {
                                case 0: return new Color(136, 40, 80);
                                case 1: return new Color(200, 56, 120);
                                case 2: return new Color(224, 120, 144);
                                case 3: return new Color(240, 184, 192);
                                case 4: return new Color(208, 176, 120);
                                case 5: return new Color(248, 232, 160);
                                case 6: return new Color(168, 104, 8);
                                case 7: return new Color(240, 184, 64);
                                case 8: return new Color(248, 248, 136);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Sacae":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Western":
                        case "Laus":
                        case "Rebel":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return new Color(208, 176, 120);
                                    case 5: return new Color(248, 232, 160);
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 20: Crossbowman, 27: Mercenary
                case 20:
                case 27:
                    switch (name)
                    {
                        case "Harken":
                            switch (color)
                            {
                                case 0: return new Color(16, 112, 136); // Main color
                                case 1: return new Color(32, 168, 152);
                                case 2: return new Color(80, 224, 200);
                                case 3: return new Color(144, 112, 56); // Hair
                                case 4: return new Color(184, 176, 40);
                                case 5: return new Color(240, 240, 48);
                                //case 7: return new Color(96, 128, 160); // Sword
                                //case 8: return new Color(152, 184, 216);
                                //case 9: return new Color(192, 224, 232);
                                //case 10: return new Color(248, 248, 248); // White
                                //case 11: return new Color(112, 80, 48); // Skin
                                //case 12: return new Color(176, 112, 64);
                                //case 13: return new Color(248, 192, 144);
                                //case 14: return new Color(248, 240, 184);
                            }
                            break;
                        case "Saka":
                            switch (color)
                            {
                                case 0: return new Color(40, 72, 88);
                                case 1: return new Color(64, 120, 144);
                                case 2: return new Color(88, 160, 192);
                                case 3: return new Color(64, 48, 72);
                                case 4: return new Color(112, 80, 112);
                                case 5: return new Color(168, 120, 152);

                                case 7: return new Color(88, 120, 152); // 80 112 136?
                                case 8: return new Color(136, 168, 176); // 112 144 160?
                                case 9: return new Color(184, 200, 200); // 168 184 184?
                                case 10: return new Color(216, 224, 208); // 216 216 192?

                                case 11: return new Color(88, 56, 48); // 72 56 48?
                                case 12: return new Color(112, 80, 64); // 112 80 64?
                                case 13: return new Color(152, 120, 96); // 144 112 88?
                                case 14: return new Color(200, 176, 152); // 192 160 128?
                            }
                            break;
                        case "Linus":
                            switch (color)
                            {
                                case 0: return new Color(120, 32, 40);
                                case 1: return new Color(192, 40, 40);
                                case 2: return new Color(224, 104, 96);
                                case 3: return new Color(160, 96, 32);
                                case 4: return new Color(216, 168, 64);
                                case 5: return new Color(232, 208, 104);
                            }
                            break;
                        case "Anoleis":
                            switch (color)
                            {
                                case 0: return new Color(48, 48, 96);
                                case 1: return new Color(88, 72, 160);
                                case 2: return new Color(144, 120, 200);
                                case 3: return new Color(136, 96, 32);
                                case 4: return new Color(200, 160, 32);
                                case 5: return new Color(248, 224, 88);
                            }
                            break;
                        case "Ilia":
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 22: Soldier
                case 22:
                    switch (name)
                    {
                        case "Bennet":
                            switch (color)
                            {
                                case 0: return new Color(96, 32, 8); // Armor
                                case 1: return new Color(176, 88, 16);
                                case 2: return new Color(240, 152, 32);
                                case 3: return new Color(80, 48, 24); // Cloth
                                case 4: return new Color(120, 88, 48);
                                case 5: return new Color(184, 136, 72);
                            }
                            break;
                        case "Maleficent":
                            switch (color)
                            {
                                case 0: return new Color(64, 56, 56);
                                case 1: return new Color(96, 88, 104);
                                case 2: return new Color(136, 120, 144);
                                case 3: return new Color(104, 48, 24);
                                case 4: return new Color(184, 96, 32);
                                case 5: return new Color(232, 160, 40);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 23: Dracoknight
                case 23:
                    switch (name)
                    {
                        case "Vaida":
                            switch (color)
                            {
                                case 0: return new Color(64, 64, 64); // Dragon
                                case 1: return new Color(88, 88, 88);
                                case 2: return new Color(112, 112, 112);
                                case 3: return new Color(144, 144, 144);
                                case 4: return new Color(120, 064, 128); // Armor
                                case 5: return new Color(216, 144, 192);
                                case 6: return new Color(208, 160, 128); // Wing/Eye
                                case 7: return new Color(184, 136, 112); // Lance head
                                case 8: return new Color(216, 192, 160);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Drag1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Drag2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Drag3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Drag4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 6: return new Color(184, 168, 248);
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                            }
                            break;
                        case "Ostia":
                        case "Laus":
                        case "Thria":
                        case "Tania":
                        case "Rebel":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; //[8, 96, 128);
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2]; //[16, 120, 136);
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3]; //[48, 160, 184);
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd4]; //[64, 208, 216);
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 6: return new Color(184, 168, 248);
                                case 7: return new Color(136, 136, 188);
                                case 8: return new Color(208, 208, 152);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; //[8, 96, 128);
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2]; //[16, 120, 136);
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3]; //[48, 160, 184);
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd4]; //[64, 208, 216);
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 6: return new Color(184, 168, 248);
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2]; //[136, 136, 188);
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3]; //[208, 208, 152);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 25: Brigand
                case 25:
                    switch (name)
                    {
                        case "Gado":
                            switch (color)
                            {
                                case 0: return new Color(72, 80, 160); // Clothes
                                case 1: return new Color(104, 128, 192);
                                case 2: return new Color(144, 184, 224);
                                case 3: return new Color(96, 64, 40); // Hair
                                case 4: return new Color(176, 120, 56);
                            }
                            break;
                        case "Utsu":
                            switch (color)
                            {
                                case 0: return new Color(104, 56, 24);
                                case 1: return new Color(168, 104, 40);
                                case 2: return new Color(224, 168, 48);
                                case 3: return new Color(192, 160, 88);
                                case 4: return new Color(240, 240, 64);
                                case 10: return new Color(88, 64, 56); // Skin
                                case 11: return new Color(112, 80, 64);
                                case 12: return new Color(144, 112, 88);
                                case 13: return new Color(184, 152, 104);
                                case 14: return new Color(240, 216, 144);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 24: Fighter
                case 24:
                    switch (name)
                    {
                        case "Shen":
                            switch (color)
                            {
                                case 0: return new Color(88, 24, 32); // Clothes
                                case 1: return new Color(144, 32, 24);
                                case 2: return new Color(208, 64, 40);
                                case 3: return new Color(80, 64, 48); // Hair
                                case 4: return new Color(128, 96, 64);
                                case 6: return new Color(96, 96, 104); // Axe
                                case 7: return new Color(136, 144, 152);
                                case 8: return new Color(200, 208, 208);
                            }
                            break;
                        case "Brendan":
                            switch (color)
                            {
                                case 0: return new Color(72, 80, 56);
                                case 1: return new Color(136, 128, 64);
                                case 2: return new Color(208, 184, 112);
                                case 3: return new Color(112, 96, 80);
                                case 4: return new Color(176, 152, 120);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2]; // Clothes
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1]; // Hair
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 26: Pirate
                case 26:
                    switch (name)
                    {
                        case "Fargus":
                            switch (color)
                            {
                                case 0: return new Color(88, 64, 104); // Pants
                                case 1: return new Color(136, 72, 160);
                                case 2: return new Color(192, 128, 200);
                                case 3: return new Color(96, 104, 168); // Hair
                                case 4: return new Color(136, 160, 200);
                                case 5: return new Color(136, 112, 120); // Shirt/Axe
                                case 6: return new Color(176, 160, 160);
                                case 7: return new Color(224, 208, 200);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return new Color(112, 144, 176);
                                case 6: return new Color(152, 184, 216);
                                case 7: return new Color(192, 224, 232);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 5: return new Color(112, 144, 176);
                                    case 6: return new Color(152, 184, 216);
                                    case 7: return new Color(192, 224, 232);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 49: Corsair
                case 49:
                    switch (name)
                    {
                        case "Fargus":
                            switch (color)
                            {
                                case 0: return new Color(88, 64, 104); // Clothes
                                case 1: return new Color(136, 88, 160);
                                case 2: return new Color(192, 128, 200);
                                case 3: return new Color(224, 176, 232);
                                case 4: return new Color(248, 232, 248);
                                case 5: return new Color(136, 112, 120); // Axe
                                case 6: return new Color(176, 160, 160);
                                case 7: return new Color(224, 208, 200);
                            }
                            break;
                        case "Tesla":
                            switch (color)
                            {
                                case 0: return new Color(16, 104, 40);
                                case 1: return new Color(40, 160, 56);
                                case 2: return new Color(104, 216, 112);
                                case 3: return new Color(136, 240, 128);
                                case 4: return new Color(240, 248, 232);
                                case 5: return new Color(120, 144, 168);
                                case 6: return new Color(160, 192, 200);
                                case 7: return new Color(208, 224, 216);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return new Color(248, 248, 248);
                                case 5: return new Color(112, 144, 176);
                                case 6: return new Color(152, 184, 216);
                                case 7: return new Color(192, 224, 232);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                    case 4: return new Color(248, 248, 248);
                                    case 5: return new Color(112, 144, 176);
                                    case 6: return new Color(152, 184, 216);
                                    case 7: return new Color(192, 224, 232);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 29: Nomad
                case 29:
                    switch (gender)
                    {
                        // Male
                        case 0:
                        case 2:
                            switch (name)
                            {
                                case "Hassar":
                                    switch (color)
                                    {
                                        case 0: return new Color(120, 40, 56); // Bow/harness
                                        case 1: return new Color(184, 56, 64);
                                        case 2: return new Color(240, 96, 80);
                                        case 3: return new Color(240, 160, 128);
                                        case 4: return new Color(96, 64, 56); // Dark border
                                        case 5: return new Color(80, 120, 152); // Shirt
                                        case 6: return new Color(80, 176, 160);
                                        case 7: return new Color(32, 104, 88); // Hair
                                        case 8: return new Color(40, 160, 88);
                                        case 9: return new Color(104, 120, 72); // Skin
                                        case 10: return new Color(144, 144, 96);
                                        case 11: return new Color(200, 192, 120);
                                    }
                                    break;
                                case "Bern":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1]; // Bow/harness
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; // Dark border
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2]; // Shirt
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return new Color(152, 64, 16); // Hair
                                        case 8: return new Color(232, 136, 32);
                                        case 9: return new Color(144, 112, 88); // Skin
                                        case 10: return new Color(192, 144, 104);
                                        case 11: return new Color(232, 192, 136);
                                    }
                                    break;
                                case "Etruria":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return new Color(56, 112, 56);
                                        case 8: return new Color(128, 152, 88);
                                        case 9: return new Color(104, 120, 72);
                                        case 10: return new Color(144, 144, 96);
                                        case 11: return new Color(200, 192, 120);
                                    }
                                    break;
                                case "Sacae":
                                case "Gladiator":
                                    switch (color)
                                    {
                                        case 0: return new Color(24, 96, 32);
                                        case 1: return new Color(40, 144, 32);
                                        case 2: return new Color(136, 208, 80);
                                        case 3: return new Color(224, 232, 120);
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return new Color(104, 120, 72);
                                        case 10: return new Color(144, 144, 96);
                                        case 11: return new Color(200, 192, 120);
                                    }
                                    break;
                            }
                            break;
                        default:
                            key_found = false;
                            break;
                    }
                    break;
                #endregion
                #region 30: Cohort
                case 30:
                    switch (name)
                    {
                        case "Darin":
                            switch (color)
                            {
                                case 0: return new Color(40, 72, 32); // Armor
                                case 1: return new Color(80, 96, 64);
                                case 2: return new Color(136, 144, 72);
                                case 3: return new Color(184, 192, 72);
                                case 4: return new Color(80, 72, 72); // Cloth/metal
                                case 5: return new Color(112, 104, 104);
                                case 6: return new Color(160, 152, 152);
                                case 7: return new Color(216, 200, 192);
                            }
                            break;
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return new Color(64, 72, 72);
                                case 5: return new Color(96, 104, 104);
                                case 6: return new Color(144, 152, 152);
                                case 7: return new Color(192, 208, 200);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return new Color(64, 72, 72);
                                case 5: return new Color(96, 104, 104);
                                case 6: return new Color(144, 152, 152);
                                case 7: return new Color(192, 208, 200);
                            }
                            break;
                        case "Ostia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return new Color(64, 72, 72);
                                case 5: return new Color(96, 104, 104);
                                case 6: return new Color(144, 152, 152);
                                case 7: return new Color(192, 208, 200);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return new Color(64, 72, 72);
                                    case 5: return new Color(96, 104, 104);
                                    case 6: return new Color(144, 152, 152);
                                    case 7: return new Color(192, 208, 200);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 31: Vanguard
                case 31:
                    switch (name)
                    {
                        case "Marcus":
                            switch (color)
                            {
                                case 0: return new Color(144, 48, 0); // Armor
                                case 1: return new Color(200, 96, 16);
                                case 2: return new Color(224, 120, 32);
                                case 3: return new Color(232, 168, 72);
                                case 4: return new Color(48, 96, 216); // Cloth
                                case 5: return new Color(112, 184, 248);
                                case 6: return new Color(168, 88, 176); // Hair
                                case 7: return new Color(224, 144, 208);
                            }
                            break;
                        case "Abelia":
                            switch (color)
                            {
                                case 0: return new Color(64, 80, 56);
                                case 1: return new Color(96, 120, 56);
                                case 2: return new Color(144, 168, 72);
                                case 3: return new Color(192, 200, 104);//176, 184, 96); //Debug
                                case 4: return new Color(168, 112, 64);
                                case 5: return new Color(216, 176, 120);
                                case 6: return new Color(192, 168, 72);
                                case 7: return new Color(232, 232, 120);
                            }
                            break;
                        case "Sarathi":
                            switch (color)
                            {
                                case 0: return new Color(64, 40, 104);
                                case 1: return new Color(80, 48, 144);
                                case 2: return new Color(120, 72, 184);
                                case 3: return new Color(160, 104, 208);
                                case 4: return new Color(76, 104, 96);
                                case 5: return new Color(104, 152, 120);
                                case 6: return new Color(152, 104, 40);
                                case 7: return new Color(216, 152, 40);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ostia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 32: Gendarme
                case 32:
                    switch (name)
                    {
                        case "Isadora":
                            switch (color)
                            {
                                case 0: return new Color(64, 64, 120); // Armor
                                case 1: return new Color(128, 128, 192);
                                case 2: return new Color(192, 192, 240);
                                case 3: return new Color(32, 80, 184); // Hair
                                case 4: return new Color(88, 136, 248);
                            }
                            break;
                        case "Chester":
                            switch (color)
                            {
                                case 0: return new Color(112, 32, 40);
                                case 1: return new Color(200, 64, 40);
                                case 2: return new Color(224, 144, 104);
                                case 3: return new Color(120, 40, 48);
                                case 4: return new Color(200, 56, 64);
                            }
                            break;
                        case "Mahki":
                            switch (color)
                            {
                                case 0: return new Color(56, 32, 80);
                                case 1: return new Color(96, 64, 104);
                                case 2: return new Color(144, 112, 120);
                                case 3: return new Color(80, 136, 152);
                                case 4: return new Color(136, 208, 184);
                                case 6: return new Color(152, 72, 88); // Sword
                                case 7: return new Color(184, 128, 128);
                                case 8: return new Color(232, 200, 192);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 33: Phalanx
                case 33:
                    switch (name)
                    {
                        case "Dom":
                            switch (color)
                            {
                                case 0: return new Color(80, 24, 0); // Armor
                                case 1: return new Color(152, 24, 0);
                                case 2: return new Color(184, 72, 8);
                                case 3: return new Color(224, 128, 32);
                                case 4: return new Color(64, 32, 72); // Cloth
                                case 5: return new Color(88, 48, 112);
                                case 6: return new Color(80, 48, 8); // Skin
                                case 7: return new Color(136, 96, 32);
                                case 8: return new Color(192, 152, 64);
                                case 9: return new Color(216, 216, 136);
                            }
                            break;
                        case "Mazda":
                            switch (color)
                            {
                                case 0: return new Color(64, 56, 64);
                                case 1: return new Color(96, 80, 88);
                                case 2: return new Color(144, 120, 128);
                                case 3: return new Color(176, 168, 168);
                                case 4: return new Color(104, 64, 104);
                                case 5: return new Color(168, 104, 152);
                                case 6: return new Color(112, 80, 48);
                                case 7: return new Color(176, 112, 64);
                                case 8: return new Color(248, 192, 144);
                                case 9: return new Color(248, 240, 184);
                            }
                            break;
                        case "Darin":
                            switch (color)
                            {
                                case 0: return new Color(40, 56, 32);
                                case 1: return new Color(80, 96, 48);
                                case 2: return new Color(128, 136, 72);
                                case 3: return new Color(184, 192, 72);
                                case 4: return new Color(56, 48, 40);
                                case 5: return new Color(88, 80, 56);
                                case 6: return new Color(112, 80, 48);
                                case 7: return new Color(176, 112, 64);
                                case 8: return new Color(248, 192, 144);
                                case 9: return new Color(248, 240, 184);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return new Color(152, 168, 224);
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 6: return new Color(112, 80, 48);
                                case 7: return new Color(176, 112, 64);
                                case 8: return new Color(248, 192, 144);
                                case 9: return new Color(248, 240, 184);
                            }
                            break;
                        case "Ostia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 6: return new Color(112, 80, 48);
                                case 7: return new Color(176, 112, 64);
                                case 8: return new Color(248, 192, 144);
                                case 9: return new Color(248, 240, 184);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 6: return new Color(112, 80, 48);
                                    case 7: return new Color(176, 112, 64);
                                    case 8: return new Color(248, 192, 144);
                                    case 9: return new Color(248, 240, 184);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 34: Lieutenant
                case 34:
                    switch (name)
                    {
                        case "Wallace":
                            switch (color)
                            {
                                case 0: return new Color(112, 112, 112); // Armor
                                case 1: return new Color(160, 160, 160);
                                case 2: return new Color(200, 200, 200);
                                case 3: return new Color(224, 224, 224);
                                case 4: return new Color(88, 104, 136); // Lance
                                case 5: return new Color(152, 160, 184);
                                case 6: return new Color(224, 232, 232);
                                case 7: return new Color(152, 112, 80); // Leather
                                case 8: return new Color(216, 152, 104);
                            }
                            break;
                        case "Beck":
                            switch (color)
                            {
                                case 0: return new Color(88, 80, 112);
                                case 1: return new Color(128, 128, 168);
                                case 2: return new Color(184, 192, 208);
                                case 3: return new Color(224, 232, 240);
                                case 4: return new Color(104, 72, 120);
                                case 5: return new Color(168, 136, 176);
                                case 6: return new Color(232, 216, 240);
                                case 7: return new Color(152, 112, 80);
                                case 8: return new Color(216, 152, 104);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Llnc1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Llnc2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Llnc3];
                                case 7: return new Color(152, 112, 80);
                                case 8: return new Color(216, 152, 104);
                            }
                            break;
                        case "Ostia":
                        case "Bandit":
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Llnc1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Llnc2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Llnc3];
                                case 7: return new Color(152, 112, 80);
                                case 8: return new Color(216, 152, 104);
                            }
                            break;
                    }
                    break;
                #endregion
                #region 35: Zweihander, 65: Hero
                case 35:
                case 65:
                    switch (name)
                    {
                        case "Harken":
                            switch (color)
                            {
                                case 0: return new Color(16, 112, 136); // Main color
                                case 1: return new Color(32, 168, 152);
                                case 2: return new Color(80, 224, 200);
                                case 3: return new Color(144, 112, 56); // Hair
                                case 4: return new Color(184, 176, 40);
                                case 5: return new Color(240, 240, 48);
                            }
                            break;
                        case "Linus":
                            switch (color)
                            {
                                case 0: return new Color(120, 32, 40);
                                case 1: return new Color(192, 40, 40);
                                case 2: return new Color(224, 104, 96);
                                case 3: return new Color(160, 96, 32);
                                case 4: return new Color(216, 168, 64);
                                case 5: return new Color(232, 208, 104);
                            }
                            break;
                        case "Celeste":
                            switch (color)
                            {
                                case 0: return new Color(32, 80, 96); // Main color
                                case 1: return new Color(56, 128, 136);
                                case 2: return new Color(72, 168, 136);
                                case 3: return new Color(96, 80, 120); // Hair
                                case 4: return new Color(136, 128, 136);
                                case 5: return new Color(192, 184, 192);
                                case 7: return new Color(56, 88, 112); // Pants/Sword
                                case 8: return new Color(112, 160, 184);
                                case 9: return new Color(192, 216, 240);
                            }
                            break;
                        case "Anoleis":
                            switch (color)
                            {
                                case 0: return new Color(48, 48, 96);
                                case 1: return new Color(88, 72, 160);
                                case 2: return new Color(144, 120, 200);
                                case 3: return new Color(136, 96, 32);
                                case 4: return new Color(200, 160, 32);
                                case 5: return new Color(248, 224, 88);
                            }
                            break;
                        case "Boston":
                            switch (color)
                            {
                                case 0: return new Color(104, 52, 52);
                                case 1: return new Color(168, 80, 64);
                                case 2: return new Color(208, 136, 112);
                                case 3: return new Color(72, 64, 88);
                                case 4: return new Color(104, 80, 128);
                                case 5: return new Color(168, 152, 176);
                            }
                            break;
                        case "Hartmut":
                            switch (color)
                            {
                                case 0: return new Color(88, 48, 136);
                                case 1: return new Color(136, 72, 152);
                                case 2: return new Color(192, 128, 192);
                                case 3: return new Color(136, 104, 80);
                                case 4: return new Color(192, 176, 104);
                                case 5: return new Color(232, 232, 120);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 37: Pegasus Knight, 76: Falcoknight
                case 37:
                case 76:
                    switch (name)
                    {
                        case "Cybil":
                            switch (color)
                            {
                                case 0: return new Color(88, 64, 56); // Main color
                                case 1: return new Color(136, 96, 80);
                                case 2: return new Color(184, 144, 120);
                                case 3: return new Color(128, 168, 160); // Pegasus
                                case 4: return new Color(176, 208, 184);
                                case 5: return new Color(200, 232, 216);
                                case 6: return new Color(88, 152, 216); // Hair
                                case 7: return new Color(144, 224, 248);
                            }
                            break;
                        case "Zephyr":
                            switch (color)
                            {
                                case 0: return new Color(40, 72, 88);
                                case 1: return new Color(80, 112, 176);
                                case 2: return new Color(128, 176, 232);
                                case 3: return new Color(128, 128, 216);
                                case 4: return new Color(168, 168, 224);
                                case 5: return new Color(208, 200, 248);
                                case 6: return new Color(48, 144, 56);
                                case 7: return new Color(120, 200, 72);
                            }
                            break;
                        case "Kirin":
                            switch (color)
                            {
                                case 0: return new Color(40, 40, 56);
                                case 1: return new Color(88, 96, 112);
                                case 2: return new Color(128, 152, 152);
                                case 3: return new Color(144, 120, 112);
                                case 4: return new Color(184, 160, 144);
                                case 5: return new Color(216, 200, 176);
                                case 6: return new Color(160, 88, 224);
                                case 7: return new Color(216, 160, 240);
                                case 9: return new Color(248, 232, 216); // White
                                case 12: return new Color(232, 160, 64); // Mane
                                case 14: return new Color(248, 248, 160);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return new Color(156, 144, 240);
                                case 4: return new Color(216, 184, 248);
                                case 5: return new Color(240, 216, 248);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                        case "Member":
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 3: return new Color(136, 144, 224);
                                case 4: return new Color(160, 176, 232);
                                case 5: return new Color(216, 216, 248);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                    }
                    break;
                #endregion
                #region 38: Cleric
                case 38:
                    switch (name)
                    {
                        case "William":
                            switch (color)
                            {
                                case 0: return new Color(80, 72, 64); // Main color
                                case 1: return new Color(112, 96, 72);
                                case 2: return new Color(160, 144, 96);
                                case 3: return new Color(200, 192, 120);
                                case 4: return new Color(104, 72, 40); // Hair
                                case 5: return new Color(136, 104, 64);
                                case 6: return new Color(184, 152, 80);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return new Color(120, 96, 32);
                                case 1: return new Color(224, 168, 32);
                                case 2: return new Color(240, 216, 32);
                                case 3: return new Color(248, 240, 152);
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 39: Monk
                case 39:
                    switch (name)
                    {
                        case "Reno":
                            switch (color)
                            {
                                case 0: return new Color(168, 80, 112); // Main color
                                case 1: return new Color(208, 136, 112);
                                case 2: return new Color(224, 192, 144);
                                case 3: return new Color(240, 232, 184);
                                case 4: return new Color(184, 128, 56); // Hair
                                case 5: return new Color(240, 216, 64);
                                case 6: return new Color(248, 248, 160);
                            }
                            break;
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return new Color(120, 96, 32); // Main color
                                case 1: return new Color(224, 168, 32);
                                case 2: return new Color(240, 216, 32);
                                case 3: return new Color(248, 240, 152);
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1]; // Hair
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 40: Mage
                case 40:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "Pent":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 48, 120); // Cloak
                                        case 1: return new Color(136, 88, 192);
                                        case 2: return new Color(208, 176, 240);
                                        case 3: return new Color(112, 112, 176); // Shirt
                                        case 4: return new Color(168, 184, 224);
                                        case 5: return new Color(200, 224, 232);
                                        case 6: return new Color(144, 152, 168); // Pants
                                        case 7: return new Color(224, 224, 232);
                                        case 8: return new Color(104, 104, 200); // Hair
                                        case 9: return new Color(160, 192, 248);
                                    }
                                    break;
                                case "Sacae":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        }
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            switch (name)
                            {
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2]; // Cloak
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; // Dress
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1]; // Hair
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            key_found = false;
                            break;
                    }
                    break;
                #endregion
                #region 41: Troubadour
                case 41:
                    switch (gender)
                    {
                        // Regular
                        case 1:
                            switch (name)
                            {
                                case "Madelyn":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 56, 56); // Main color
                                        case 1: return new Color(120, 64, 72);
                                        case 2: return new Color(168, 80, 80);
                                        case 3: return new Color(232, 128, 112);
                                        case 4: return new Color(176, 96, 32); // Hair
                                        case 5: return new Color(240, 208, 56);
                                        case 6: return new Color(248, 248, 8); // Trim
                                    }
                                    break;
                                case "Bern":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(248, 248, 8);
                                    }
                                    break;
                                case "Etruria":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(248, 248, 8);
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(240, 240, 88);
                                    }
                                    break;
                                case "Ostia":
                                case "Bandit":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(240, 240, 88);
                                    }
                                    break;
                                default:
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(240, 240, 88);
                                    }
                                    break;
                            }
                            break;
                        default:
                            key_found = false;
                            break;
                    }
                    break;
                #endregion
                #region 43: Witch
                case 43:
                    switch (name)
                    {
                        case "Elaice":
                            switch (color)
                            {
                                case 0: return new Color(32, 88, 88); // Main color
                                case 1: return new Color(40, 136, 104);
                                case 2: return new Color(56, 176, 112);
                                case 3: return new Color(112, 72, 48); // Hair
                                case 4: return new Color(152, 120, 24);
                                case 5: return new Color(200, 176, 80);
                                case 6: return new Color(136, 96, 32); // Cloth
                                case 7: return new Color(208, 168, 64);
                                case 8: return new Color(224, 216, 104);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2]; // Main color
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1]; // Hair
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert1]; // Cloth
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                            }
                            break;
                        case "Sacae":
                        case "Ostia":
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 44: Sorcerer
                case 44:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "Bern":
                                case "Etruria":
                                case "Bandit":
                                case "Laus":
                                case "Tania":
                                case "Rebel":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1]; // Cloak
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; // Fringe
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim2]; // Trim
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        }
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            switch (name)
                            {
                                case "Eiry":
                                    switch (color)
                                    {
                                        case 0: return new Color(48, 48, 48); // Cloak
                                        case 1: return new Color(64, 64, 64);
                                        case 2: return new Color(80, 80, 80);
                                        case 3: return new Color(112, 112, 112);
                                        case 4: return new Color(112, 80, 104); // Headdress, front
                                        case 5: return new Color(200, 152, 184);
                                        case 6: return new Color(232, 192, 216);
                                        case 7: return new Color(240, 200, 208); // Trim
                                        case 8: return new Color(168, 8, 16); // Hair
                                        case 9: return new Color(216, 48, 16);
                                    }
                                    break;
                                case "Bern":
                                case "Etruria":
                                case "Bandit":
                                case "Laus":
                                case "Tania":
                                case "Rebel":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            key_found = false;
                            break;
                    }
                    break;
                #endregion
                #region 45: Shaman, 86: Druid
                case 45:
                case 86:
                    switch (name)
                    {
                        case "Magnus":
                            switch (color)
                            {
                                case 0: return new Color(32, 104, 32); // Robe
                                case 1: return new Color(72, 152, 64);
                                case 2: return new Color(120, 208, 88);
                                case 3: return new Color(152, 112, 72); // Pants
                                case 4: return new Color(200, 168, 96);
                                case 5: return new Color(224, 216, 152);
                                case 6: return new Color(64, 56, 56); // Hair
                                case 7: return new Color(96, 88, 80);
                                case 8: return new Color(144, 128, 112);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 47: Diviner
                case 47:
                    switch (name)
                    {
                        case "Hyde":
                            switch (color)
                            {
                                case 0: return new Color(56, 48, 64); // Robe
                                case 1: return new Color(72, 64, 96);
                                case 2: return new Color(88, 88, 136);
                                case 3: return new Color(128, 136, 184);
                                case 4: return new Color(120, 128, 152); // Armor
                                case 5: return new Color(176, 192, 208);
                                case 6: return new Color(144, 112, 32); // Hair
                                case 7: return new Color(216, 184, 24);
                                case 8: return new Color(248, 248, 104);
                            }
                            break;
                        case "Basil":
                            switch (color)
                            {
                                case 0: return new Color(56, 48, 88);
                                case 1: return new Color(64, 64, 144);
                                case 2: return new Color(80, 96, 200);
                                case 3: return new Color(112, 144, 232);
                                case 4: return new Color(152, 144, 80);
                                case 5: return new Color(208, 208, 112);
                                case 6: return new Color(80, 40, 40);
                                case 7: return new Color(136, 80, 64);
                                case 8: return new Color(184, 112, 80);
                            }
                            break;
                        case "Richard":
                            switch (color)
                            {
                                case 0: return new Color(72, 56, 56);
                                case 1: return new Color(112, 72, 64);
                                case 2: return new Color(152, 96, 72);
                                case 3: return new Color(192, 136, 96);
                                case 4: return new Color(152, 144, 136);
                                case 5: return new Color(200, 200, 192);
                                case 6: return new Color(72, 48, 128);
                                case 7: return new Color(112, 64, 192);
                                case 8: return new Color(160, 112, 240);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return new Color(152, 144, 136);
                                case 5: return new Color(200, 200, 192);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return new Color(152, 144, 136);
                                case 5: return new Color(200, 200, 192);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return new Color(152, 144, 136);
                                    case 5: return new Color(200, 200, 192);
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 48: Scholar
                case 48:
                    switch (name)
                    {
                        case "Jericho":
                            switch (color)
                            {
                                case 0: return new Color(16, 56, 120); // Main color
                                case 1: return new Color(32, 88, 160);
                                case 2: return new Color(48, 120, 192);
                                case 3: return new Color(56, 152, 208);
                                case 4: return new Color(144, 56, 88); // Hair/Cloak
                                case 5: return new Color(216, 104, 120);
                                case 6: return new Color(240, 160, 176);
                                case 7: return new Color(160, 136, 128); // Pants/Pages
                                case 8: return new Color(208, 184, 160);
                            }
                            break;
                        case "Ostia":
                        case "Laus":
                        case "Thria":
                        case "Tania":
                        case "Rebel":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Tert4];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                case 7: return new Color(144, 160, 136);
                                case 8: return new Color(200, 208, 184);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 51: Swordmaster
                case 51:
                    switch (name)
                    {
                        case "Lloyd":
                            switch (color)
                            {
                                case 0: return new Color(56, 72, 16); // Main color
                                case 1: return new Color(96, 120, 40);
                                case 2: return new Color(168, 200, 80);
                                case 3: return new Color(208, 216, 136);
                                case 4: return new Color(144, 96, 24); // Hair
                                case 5: return new Color(208, 176, 32);
                                case 6: return new Color(232, 232, 72);
                            }
                            break;
                        case "Mundus":
                            switch (color)
                            {
                                case 0: return new Color(80, 48, 32);
                                case 1: return new Color(144, 104, 48);
                                case 2: return new Color(200, 184, 96);
                                case 3: return new Color(208, 216, 136);
                                case 4: return new Color(40, 56, 40);
                                case 5: return new Color(72, 96, 32);
                                case 6: return new Color(128, 136, 56);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 52: Assassin
                case 52:
                    switch (name)
                    {
                        case "Valeria":
                            switch (color)
                            {
                                case 0: return new Color(88, 72, 40); // Cloak
                                case 1: return new Color(152, 112, 96);
                                case 2: return new Color(200, 160, 120);
                                case 3: return new Color(56, 80, 72); // Cloth
                                case 4: return new Color(72, 120, 104);
                                case 5: return new Color(112, 176, 128);
                                case 6: return new Color(120, 72, 40); // Hair
                                case 7: return new Color(200, 112, 120);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Western":
                        case "Ilia":
                        case "Bandit":
                        case "Member":
                        case "Inmate":
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Sacae":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Tert4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 53: Rogue
                case 53:
                    switch (name)
                    {
                        case "Leonard":
                            switch (color)
                            {
                                case 0: return new Color(104, 96, 8); // Leather
                                case 1: return new Color(160, 152, 8);
                                case 2: return new Color(200, 200, 32);
                                case 3: return new Color(104, 32, 160); // Cloth
                                case 4: return new Color(136, 72, 216);
                                case 5: return new Color(168, 144, 232);
                                case 6: return new Color(40, 96, 96); // Hair
                                case 7: return new Color(72, 176, 160);
                            }
                            break;
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return new Color(96, 64, 0);
                                case 4: return new Color(136, 104, 0);
                                case 5: return new Color(200, 152, 40);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Sacae":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return new Color(96, 40, 24);
                                case 1: return new Color(144, 64, 24);
                                case 2: return new Color(216, 128, 40);
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                        switch (name)
                                        {
                                            case "Bern":
                                                switch (color)
                                                {
                                                    case 0: return new Color(96, 64, 0);
                                                    case 1: return new Color(136, 104, 0);
                                                    default: return new Color(200, 152, 40);
                                                }
                                            case "Western":
                                                switch (color)
                                                {
                                                    case 0: return new Color(104, 24, 32);
                                                    case 1: return new Color(160, 56, 40);
                                                    default: return new Color(200, 104, 64);
                                                }
                                            case "Bandit":
                                            case "Member":
                                                switch (color)
                                                {
                                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                                    default: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                                }
                                            case "Gladiator":
                                                switch (color)
                                                {
                                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                                    default: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                                }
                                            default:
                                                switch (color)
                                                {
                                                    case 0: return new Color(96, 40, 24);
                                                    case 1: return new Color(144, 64, 24);
                                                    default: return new Color(216, 128, 40);
                                                }
                                        }
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 55: Longbowman
                case 55:
                    switch (name)
                    {
                        case "Toni":
                            switch (color)
                            {
                                case 0: return new Color(112, 48, 16); // Main color
                                case 1: return new Color(192, 88, 8);
                                case 2: return new Color(232, 144, 48);
                                case 3: return new Color(240, 200, 120);
                                case 4: return new Color(32, 96, 48); // Hair
                                case 5: return new Color(32, 144, 64);
                            }
                            break;
                        case "Louise":
                            switch (color)
                            {
                                case 0: return new Color(136, 40, 80); // Main color
                                case 1: return new Color(200, 56, 120);
                                case 2: return new Color(224, 120, 144);
                                case 3: return new Color(240, 184, 192);
                                case 4: return new Color(168, 104, 8); // Hair
                                case 5: return new Color(240, 184, 64);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 59: Halberdier
                case 59:
                    switch (name)
                    {
                        case "Bennet":
                            switch (color)
                            {
                                case 0: return new Color(96, 32, 8); // Armor
                                case 1: return new Color(176, 88, 16);
                                case 2: return new Color(240, 152, 32);
                                case 3: return new Color(80, 48, 24); // Cloth
                                case 4: return new Color(120, 88, 48);
                                case 5: return new Color(184, 136, 72);
                            }
                            break;
                        case "Uriel":
                            switch (color)
                            {
                                case 0: return new Color(72, 64, 48);
                                case 1: return new Color(104, 96, 80);
                                case 2: return new Color(144, 136, 120);
                                case 3: return new Color(80, 56, 40);
                                case 4: return new Color(136, 96, 40);
                                case 5: return new Color(184, 144, 72);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 61: Dragon Master
                case 61:
                    switch (name)
                    {
                        case "Vaida":
                            switch (color)
                            {
                                case 0: return new Color(64, 64, 64); // Dragon
                                case 1: return new Color(88, 88, 88);
                                case 2: return new Color(112, 112, 112);
                                case 3: return new Color(144, 144, 144);
                                case 4: return new Color(56, 104, 224); // Lance detail/Eye
                                case 5: return new Color(40, 184, 248);
                                case 6: return new Color(208, 160, 128); // Wing
                                case 7: return new Color(120, 104, 128); // Metal
                                case 8: return new Color(184, 168, 184);
                                case 9: return new Color(248, 224, 240);

                                case 10: return new Color(40, 40, 40); // Outline
                                case 11: return new Color(64, 48, 32); // Leather/Skin
                                case 12: return new Color(128, 104, 88);
                                case 13: return new Color(192, 128, 112);
                                case 14: return new Color(240, 192, 144);
                            }
                            break;
                        case "Belmont":
                            switch (color)
                            {
                                case 0: return new Color(104, 64, 0);
                                case 1: return new Color(152, 96, 0);
                                case 2: return new Color(224, 176, 0);
                                case 3: return new Color(248, 232, 104);
                                case 4: return new Color(104, 32, 32);
                                case 5: return new Color(208, 24, 24);
                                case 6: return new Color(56, 88, 80);
                                case 7: return new Color(64, 112, 88);
                                case 8: return new Color(128, 160, 152);
                                case 9: return new Color(216, 248, 232);

                                case 10: return new Color(40, 40, 40);
                                case 11: return new Color(64, 48, 32);
                                case 12: return new Color(128, 104, 88);
                                case 13: return new Color(192, 128, 112);
                                case 14: return new Color(240, 192, 144);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Drag1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Drag2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Drag3];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Drag4];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 6: return new Color(184, 168, 248);
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 9: return new Color(224, 232, 232);

                                case 10: return new Color(40, 40, 40);
                                case 11: return new Color(64, 48, 32);
                                case 12: return new Color(128, 104, 88);
                                case 13: return new Color(192, 128, 112);
                                case 14: return new Color(240, 192, 144);
                            }
                            break;
                        case "Ostia":
                        case "Laus":
                        case "Thria":
                        case "Tania":
                        case "Rebel":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; //[8, 96, 128);
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2]; //[16, 120, 136);
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3]; //[48, 160, 184);
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd4]; //[64, 208, 216);
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 6: return new Color(184, 168, 248);
                                case 7: return new Color(136, 136, 188);
                                case 8: return new Color(208, 208, 152);
                                case 9: return new Color(224, 232, 232);

                                case 10: return new Color(40, 40, 40);
                                case 11: return new Color(64, 48, 32);
                                case 12: return new Color(128, 104, 88);
                                case 13: return new Color(192, 128, 112);
                                case 14: return new Color(240, 192, 144);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; //[8, 96, 128);
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2]; //[16, 120, 136);
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3]; //[48, 160, 184);
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd4]; //[64, 208, 216);
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 6: return new Color(184, 168, 248);
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2]; //[136, 136, 188);
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3]; //[208, 208, 152);
                                    case 9: return new Color(224, 232, 232);

                                    case 10: return new Color(40, 40, 40);
                                    case 11: return new Color(64, 48, 32);
                                    case 12: return new Color(128, 104, 88);
                                    case 13: return new Color(192, 128, 112);
                                    case 14: return new Color(240, 192, 144);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 62: Berserker
                case 62:
                    switch (name)
                    {
                        case "Isaac":
                            switch (color)
                            {
                                case 0: return new Color(144, 64, 72); // Clothes
                                case 1: return new Color(184, 80, 72);
                                case 2: return new Color(216, 104, 80);
                                case 3: return new Color(144, 96, 64); // Metal
                                case 4: return new Color(208, 152, 88);
                                case 5: return new Color(232, 208, 136);
                                case 6: return new Color(104, 88, 120); // Trim
                                case 7: return new Color(128, 120, 160);
                            }
                            break;
                        case "Melios":
                            switch (color)
                            {
                                case 0: return new Color(40, 72, 72);
                                case 1: return new Color(56, 112, 80);
                                case 2: return new Color(80, 168, 88);
                                case 6: return new Color(160, 152, 144);
                                case 7: return new Color(240, 240, 224);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return new Color(192, 176, 16);
                                case 7: return new Color(248, 248, 8);
                            }
                            break;
                        case "Western":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                case 6: return new Color(192, 176, 16);
                                case 7: return new Color(248, 248, 8);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 3: return new Color(88, 120, 144);
                                case 4: return new Color(136, 168, 192);
                                case 5: return new Color(176, 208, 216);
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return new Color(88, 120, 144);
                                    case 4: return new Color(136, 168, 192);
                                    case 5: return new Color(176, 208, 216);
                                    case 6: return new Color(192, 176, 16);
                                    case 7: return new Color(248, 248, 8);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 63: Warrior
                case 63:
                    switch (name)
                    {
                        case "Brendan":
                            switch (color)
                            {
                                case 0: return new Color(64, 56, 40); // Clothes
                                case 1: return new Color(88, 72, 56);
                                case 2: return new Color(128, 104, 80);
                                case 3: return new Color(176, 136, 64); // Armor
                                case 4: return new Color(224, 208, 88);
                            }
                            break;
                        case "Shen":
                            switch (color)
                            {
                                case 0: return new Color(88, 24, 32);
                                case 1: return new Color(144, 32, 24);
                                case 2: return new Color(208, 64, 40);
                                case 3: return null; // new Color(168, 128, 96);
                                case 4: return null; // new Color(216, 200, 184);
                            }
                            break;
                        case "Bart":
                            switch (color)
                            {
                                case 0: return new Color(128, 104, 16);
                                case 1: return new Color(160, 144, 48);
                                case 2: return new Color(208, 200, 48);
                                case 3: return new Color(168, 128, 96);
                                case 4: return new Color(216, 200, 184);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                            }
                            break;
                        case "Western":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 67: Nomad Trooper
                case 67:
                    switch (name)
                    {
                        case "Hassar":
                            switch (color)
                            {
                                case 0: return new Color(120, 40, 56); // Bow/harness
                                case 1: return new Color(184, 56, 64);
                                case 2: return new Color(240, 96, 80);
                                case 3: return new Color(240, 160, 128);
                                case 4: return new Color(80, 120, 152); // Shirt
                                case 5: return new Color(80, 176, 160);
                                case 6: return new Color(32, 104, 88); // Hair
                                case 7: return new Color(40, 160, 88);
                                case 8: return new Color(112, 64, 40); // Horse
                                case 9: return new Color(168, 112, 32);
                                case 10: return new Color(184, 160, 56);
                                case 11: return new Color(240, 232, 104);
                            }
                            break;
                        case "Milo":
                            switch (color)
                            {
                                case 0: return new Color(88, 72, 64);
                                case 1: return new Color(136, 104, 96);
                                case 2: return new Color(176, 152, 136);
                                case 3: return new Color(192, 184, 176);
                                case 4: return new Color(152, 72, 48);
                                case 5: return new Color(216, 128, 64);
                                case 6: return new Color(40, 104, 48);
                                case 7: return new Color(64, 176, 88);
                                case 8: return new Color(112, 64, 40);
                                case 9: return new Color(168, 112, 32);
                                case 10: return new Color(184, 160, 56);
                                case 11: return new Color(240, 232, 104);
                            }
                            break;
                        case "Sacae":
                            switch (color)
                            {
                                case 0: return new Color(24, 96, 32);
                                case 1: return new Color(40, 144, 32);
                                case 2: return new Color(136, 208, 80);
                                case 3: return new Color(224, 232, 120);
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return new Color(112, 64, 40);
                                case 9: return new Color(168, 112, 32);
                                case 10: return new Color(184, 160, 56);
                                case 11: return new Color(240, 232, 104);
                            }
                            break;
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                case 8: return new Color(112, 64, 40);
                                case 9: return new Color(168, 112, 32);
                                case 10: return new Color(184, 160, 56);
                                case 11: return new Color(240, 232, 104);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                    case 6: return new Color(152, 64, 16);
                                    case 7: return new Color(232, 136, 32);
                                    case 8: return new Color(112, 64, 40);
                                    case 9: return new Color(168, 112, 32);
                                    case 10: return new Color(184, 160, 56);
                                    case 11: return new Color(240, 232, 104);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 70: Paladin
                case 70:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "Eagler":
                                    switch (color)
                                    {
                                        case 0: return new Color(64, 72, 80); // Armor
                                        case 1: return new Color(99, 104, 112);
                                        case 2: return new Color(112, 152, 152);
                                        case 3: return new Color(152, 176, 168);
                                        case 4: return new Color(32, 144, 128); // Shield
                                        case 5: return new Color(64, 224, 144);
                                        case 6: return new Color(136, 112, 96); // Skin
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88); // Horse's Hair
                                    }
                                    break;
                                case "Chester":
                                    switch (color)
                                    {
                                        case 0: return new Color(112, 32, 40);
                                        case 1: return new Color(160, 48, 40);
                                        case 2: return new Color(216, 80, 48);
                                        case 3: return new Color(224, 144, 104);
                                        case 4: return new Color(80, 72, 88);
                                        case 5: return new Color(112, 104, 136);
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Elbert":
                                    switch (color)
                                    {
                                        case 0: return new Color(64, 48, 136);
                                        case 1: return new Color(56, 72, 216);
                                        case 2: return new Color(104, 144, 224);
                                        case 3: return new Color(168, 192, 248);
                                        case 4: return new Color(128, 32, 40);
                                        case 5: return new Color(192, 16, 16);
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(240, 168, 96);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Horace":
                                    switch (color)
                                    {
                                        case 0: return new Color(96, 88, 64);
                                        case 1: return new Color(144, 136, 88);
                                        case 2: return new Color(176, 176, 112);
                                        case 3: return new Color(192, 200, 160);
                                        case 4: return new Color(56, 88, 184);
                                        case 5: return new Color(96, 152, 216);
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Hubart":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 64, 64);
                                        case 1: return new Color(120, 96, 88);
                                        case 2: return new Color(136, 128, 112);
                                        case 3: return new Color(168, 168, 152);
                                        case 4: return new Color(144, 128, 64);
                                        case 5: return new Color(216, 208, 72);
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Bern":
                                case "Etruria":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                case "Tania":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(240, 168, 96);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Gladiator":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            switch (name)
                            {
                                case "Abelia":
                                    switch (color)
                                    {
                                        case 0: return new Color(64, 80, 56); // Armor
                                        case 1: return new Color(96, 120, 56);
                                        case 2: return new Color(144, 168, 72);
                                        case 3: return new Color(176, 184, 96);
                                        case 4: return new Color(184, 160, 56); // Hair/Shield
                                        case 5: return new Color(232, 232, 120);
                                        case 6: return new Color(168, 112, 88); // Skin
                                        case 7: return new Color(240, 168, 96);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88); // Horse's Hair
                                    }
                                    break;
                                case "Bern":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                case "Tania":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(240, 168, 96);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                                case "Gladiator":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 6: return new Color(136, 112, 96);
                                        case 7: return new Color(200, 184, 72);
                                        case 8: return new Color(248, 248, 216);
                                        case 9: return new Color(248, 248, 88);
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                #endregion
                #region 71: Cataphract
                case 71:
                    switch (name)
                    {
                        case "Darin":
                            switch (color)
                            {
                                case 0: return new Color(64, 80, 40); // Armor
                                case 1: return new Color(120, 128, 48);
                                case 2: return new Color(160, 160, 80);
                                case 3: return new Color(64, 56, 96); // Horse/blade
                                case 4: return new Color(104, 96, 128);
                                case 5: return new Color(152, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(40, 72, 72); // Shield/cloth/axe head
                                case 8: return new Color(88, 104, 112);
                                case 9: return new Color(128, 160, 176);
                                /*case 0: return new Color(112, 32, 40); // Armor
                                case 1: return new Color(200, 64, 40);
                                case 2: return new Color(224, 144, 104);
                                case 3: return new Color(56, 56, 96); // Horse/blade
                                case 4: return new Color(96, 96, 128);
                                case 5: return new Color(144, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(56, 56, 120); // Shield/cloth/axe head
                                case 8: return new Color(96, 96, 152);
                                case 9: return new Color(144, 144, 176);*/
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return new Color(56, 56, 96);
                                case 4: return new Color(96, 96, 128);
                                case 5: return new Color(144, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(56, 56, 120);
                                case 8: return new Color(96, 96, 152);
                                case 9: return new Color(144, 144, 176);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return new Color(56, 56, 96);
                                case 4: return new Color(96, 96, 128);
                                case 5: return new Color(144, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(56, 56, 120);
                                case 8: return new Color(96, 96, 152);
                                case 9: return new Color(144, 144, 176);
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return new Color(56, 56, 96);
                                case 4: return new Color(96, 96, 128);
                                case 5: return new Color(144, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(56, 56, 120);
                                case 8: return new Color(96, 96, 152);
                                case 9: return new Color(144, 144, 176);
                            }
                            break;
                        case "Laus":
                        case "Thria":
                        case "Tania":
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return new Color(56, 56, 96);
                                case 4: return new Color(96, 96, 128);
                                case 5: return new Color(144, 144, 168);
                                case 6: return new Color(200, 192, 208);
                                case 7: return new Color(56, 56, 120);
                                case 8: return new Color(96, 96, 152);
                                case 9: return new Color(144, 144, 176);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return new Color(56, 56, 96);
                                    case 4: return new Color(96, 96, 128);
                                    case 5: return new Color(144, 144, 168);
                                    case 6: return new Color(200, 192, 208);
                                    case 7: return new Color(56, 56, 120);
                                    case 8: return new Color(96, 96, 152);
                                    case 9: return new Color(144, 144, 176);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 73: General
                case 73:
                    switch (name)
                    {
                        case "Wallace":
                            switch (color)
                            {
                                case 0: return new Color(112, 112, 112); // Armor
                                case 1: return new Color(168, 168, 168);
                                case 2: return new Color(216, 216, 216);
                                case 3: return new Color(232, 232, 232);
                                case 4: return new Color(72, 40, 0); // Trim
                                case 5: return new Color(152, 104, 40);
                                case 6: return new Color(208, 176, 112);
                                case 7: return new Color(232, 232, 184);
                                case 8: return new Color(96, 40, 16); // Flap
                                case 9: return new Color(136, 64, 16);
                            }
                            break;
                        case "Kalten":
                            switch (color)
                            {
                                case 0: return new Color(40, 40, 168);
                                case 1: return new Color(56, 64, 240);
                                case 2: return new Color(72, 96, 248);
                                case 3: return new Color(104, 136, 248);
                                case 4: return new Color(72, 40, 0);
                                case 5: return new Color(152, 104, 40);
                                case 6: return new Color(208, 176, 112);
                                case 7: return new Color(232, 232, 184);
                                case 8: return new Color(112, 80, 32);
                                case 9: return new Color(184, 144, 32);
                            }
                            break;
                        case "Bern":
                        case "Etruria":
                        case "Laus":
                        case "Thria":
                        case "Tania":
                        case "Rebel":
                        case "Inmate":
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return new Color(72, 40, 0);
                                case 5: return new Color(152, 104, 40);
                                case 6: return new Color(208, 176, 112);
                                case 7: return new Color(232, 232, 184);
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 9: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                            }
                            break;
                        case "Ostia": // throwing this in here for now but this isn't actually tested //Yeti
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 4: return new Color(72, 40, 0);
                                case 5: return new Color(152, 104, 40);
                                case 6: return new Color(208, 176, 112);
                                case 7: return new Color(232, 232, 184);
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 9: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 4: return new Color(72, 40, 0);
                                case 5: return new Color(152, 104, 40);
                                case 6: return new Color(208, 176, 112);
                                case 7: return new Color(232, 232, 184);
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 9: return COUNTRY_COLORS[name][Color_Keys.Flap1];
                            }
                            break;
                    }
                    break;
                #endregion
                #region 78: Bishop
                case 78:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "William":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 72, 64); // Robe
                                        case 1: return new Color(112, 96, 72);
                                        case 2: return new Color(144, 128, 96);
                                        case 3: return new Color(120, 136, 144); // Shawl
                                        case 4: return new Color(168, 184, 192);
                                        case 5: return new Color(216, 184, 16); // Staff
                                        case 6: return new Color(240, 232, 112);
                                        case 7: return new Color(128, 104, 40); // Hair
                                        case 8: return new Color(200, 160, 88);
                                    }
                                    break;
                                case "Elle":
                                    switch (color)
                                    {
                                        case 0: return new Color(64, 64, 104); // Robe
                                        case 1: return new Color(80, 88, 136);
                                        case 2: return new Color(104, 120, 176);
                                        case 3: return new Color(168, 144, 48); // Shawl
                                        case 4: return new Color(216, 208, 112);
                                        case 5: return new Color(192, 136, 224); // Staff
                                        case 6: return new Color(224, 192, 248);
                                        case 7: return new Color(112, 80, 56); // Hair
                                        case 8: return new Color(160, 120, 88);
                                    }
                                    break;
                                case "Bern":
                                case "Sacae":
                                case "Western":
                                case "Ilia":
                                case "Bandit":
                                case "Gladiator":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                case "Etruria":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return new Color(120, 96, 32);
                                        case 1: return new Color(224, 168, 32);
                                        case 2: return new Color(248, 224, 120);
                                        case 3: return new Color(232, 208, 32);
                                        case 4: return new Color(248, 240, 152);
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                case "Member":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Tert4];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        }
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            switch (name)
                            {
                                case "Elle":
                                    switch (color)
                                    {
                                        case 0: return new Color(64, 64, 104); // Robe
                                        case 1: return new Color(80, 88, 136);
                                        case 2: return new Color(104, 120, 176);
                                        case 3: return new Color(168, 144, 48); // Shawl
                                        case 4: return new Color(216, 208, 112);
                                        case 5: return new Color(192, 136, 224); // Staff
                                        case 6: return new Color(224, 192, 248);
                                        case 7: return new Color(112, 80, 56); // Hair
                                        case 8: return new Color(160, 120, 88);
                                        case 9: return new Color(200, 160, 120);
                                    }
                                    break;
                                case "Bern":
                                case "Sacae":
                                case "Western":
                                case "Ilia":
                                case "Bandit":
                                case "Gladiator":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Etruria":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return new Color(120, 96, 32);
                                        case 1: return new Color(224, 168, 32);
                                        case 2: return new Color(248, 224, 120);
                                        case 3: return new Color(232, 208, 32);
                                        case 4: return new Color(248, 240, 152);
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Member":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main6];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Tert4];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                #endregion
                #region 79: Sage
                case 79:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "Pent":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 48, 120); // Cloak
                                        case 1: return new Color(136, 88, 192);
                                        case 2: return new Color(208, 176, 240);
                                        case 3: return new Color(232, 224, 248);
                                        case 4: return new Color(112, 112, 176); // Robe
                                        case 5: return new Color(168, 184, 224);
                                        case 6: return new Color(200, 224, 232);
                                        case 7: return new Color(104, 104, 200); // Hair
                                        case 8: return new Color(160, 192, 248);
                                        case 9: return new Color(184, 224, 232);
                                    }
                                    break;
                                case "Sacae":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                        }
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            {
                                switch (name)
                                {
                                    case "Meredith":
                                        switch (color)
                                        {
                                            case 0: return new Color(112, 40, 48); // Cloak
                                            case 1: return new Color(160, 56, 56);
                                            case 2: return new Color(216, 80, 72);
                                            case 3: return new Color(56, 88, 80);// Robe
                                            case 4: return new Color(128, 168, 144);
                                            case 5: return new Color(192, 216, 192);
                                            case 6: return new Color(64, 112, 72); // Hair
                                            case 7: return new Color(104, 152, 104);
                                            case 8: return new Color(152, 200, 144);
                                            case 9: return new Color(224, 184, 104); // Trim
                                        }
                                        break;
                                    case "Sacae":
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                            case 9: return new Color(248, 216, 144);
                                        }
                                        break;
                                    case "Ilia":
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        }
                                        break;
                                    case "Ostia":
                                    case "Thria":
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        }
                                        break;
                                    default:
                                        if (COUNTRY_COLORS.ContainsKey(name))
                                        {
                                            switch (color)
                                            {
                                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                                case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair3];
                                                case 9: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                            }
                                        }
                                        break;
                                }
                                break;
                            }
                        default:
                            key_found = false;
                            break;
                    }
                    break;
                #endregion
                #region 80: Mage Knight
                case 80:
                    switch (name)
                    {
                        case "Bern":
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1]; // Caparison/Spaulders
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd1]; // Clothes
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert2]; // Scarf
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1]; // Hair
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 8: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 81: Valkyrie
                case 81:
                    switch (name)
                    {
                        case "Madelyn":
                            switch (color)
                            {
                                case 0: return new Color(96, 64, 64); // Main color
                                case 1: return new Color(160, 80, 88);
                                case 2: return new Color(232, 128, 112);
                                case 3: return new Color(72, 48, 56); // Horse"s Hair
                                case 4: return new Color(120, 72, 80);
                                case 5: return new Color(176, 96, 32); // Hair
                                case 6: return new Color(240, 208, 56);
                            }
                            break;
                        case "Bern":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return new Color(24, 32, 104);
                                case 4: return new Color(80, 56, 152);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Ostia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return new Color(40, 24, 96);
                                case 4: return new Color(40, 32, 136);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        case "Gladiator":
                            switch (color)
                            {
                                case 0: return new Color(40, 104, 56);
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 4: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                            }
                            break;
                        default:
                            if (false)//COUNTRY_COLORS.ContainsKey(name)) //Debug
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                    case 4: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Hair1];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Hair2];
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 84: Warlock
                case 84:
                    switch (gender)
                    {
                        // Male
                        case 0:
                            switch (name)
                            {
                                case "Roeis":
                                    switch (color)
                                    {
                                        case 0: return new Color(80, 40, 48); // Cloak
                                        case 1: return new Color(144, 56, 72);
                                        case 2: return new Color(200, 88, 88);
                                        case 3: return new Color(240, 152, 144);
                                        case 4: return new Color(64, 80, 72); // Fringe
                                        case 5: return new Color(120, 144, 136);
                                        case 6: return new Color(200, 216, 208);
                                        case 7: return new Color(104, 76, 16); // Design
                                        case 8: return new Color(176, 152, 24);
                                        case 9: return new Color(224, 224, 40);
                                    }
                                    break;
                                case "Crane":
                                    switch (color)
                                    {
                                        case 0: return new Color(56, 56, 96);
                                        case 1: return new Color(80, 80, 112);
                                        case 2: return new Color(120, 120, 144);
                                        case 3: return new Color(168, 160, 176);
                                        case 4: return new Color(112, 56, 16);
                                        case 5: return new Color(216, 144, 48);
                                        case 6: return new Color(232, 216, 88);
                                        case 7: return new Color(128, 64, 24);
                                        case 8: return new Color(176, 88, 48);
                                        case 9: return new Color(232, 128, 56);
                                    }
                                    break;
                                case "Bern":
                                case "Etruria":
                                case "Bandit":
                                case "Laus":
                                case "Tania":
                                case "Rebel":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 9: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                            case 9: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        }
                                    }
                                    break;
                            }
                            break;
                        // Female
                        case 1:
                            switch (name)
                            {
                                case "Eiry":
                                    switch (color)
                                    {
                                        case 0: return new Color(40, 40, 40); // Cloak
                                        case 1: return new Color(64, 64, 64);
                                        case 2: return new Color(80, 80, 80);
                                        case 3: return new Color(112, 112, 112);
                                        case 4: return new Color(136, 88, 104); // Fringe
                                        case 5: return new Color(216, 144, 160);
                                        case 6: return new Color(248, 192, 192);
                                        case 7: return new Color(240, 200, 128); // Design
                                        case 8: return new Color(248, 232, 192);
                                        case 9: return new Color(168, 8, 16); // Hair (unused)
                                    }
                                    break;
                                case "Bern":
                                case "Etruria":
                                case "Bandit":
                                case "Laus":
                                case "Tania":
                                case "Rebel":
                                case "Inmate":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ostia":
                                case "Thria":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd4];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Trim2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Trim3];
                                    }
                                    break;
                                case "Ilia":
                                    switch (color)
                                    {
                                        case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                        case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                        case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                        case 3: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                        case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                        case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                        case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                        case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                        case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    }
                                    break;
                                default:
                                    if (COUNTRY_COLORS.ContainsKey(name))
                                    {
                                        switch (color)
                                        {
                                            case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                            case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                            case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                            case 3: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                            case 4: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                            case 5: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                            case 6: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                            case 7: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                            case 8: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                #endregion
                #region 88: Justice
                case 88:
                    switch (name)
                    {
                        case "Hyde":
                            switch (color)
                            {
                                case 0: return new Color(72, 64, 96); // Robe
                                case 1: return new Color(88, 88, 136);
                                case 2: return new Color(128, 136, 184);
                                case 3: return new Color(120, 128, 152); // Armor
                                case 4: return new Color(176, 192, 208);
                                case 5: return new Color(96, 64, 48); // Chainmail
                                case 6: return new Color(160, 96, 56);
                                case 7: return new Color(216, 136, 64);
                                case 8: return new Color(248, 248, 16); // Trim
                            }
                            break;
                        case "Richard":
                            switch (color)
                            {
                                case 0: return new Color(112, 72, 64);
                                case 1: return new Color(152, 96, 72);
                                case 2: return new Color(192, 136, 96);
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return new Color(56, 64, 48);
                                case 6: return new Color(80, 80, 72);
                                case 7: return new Color(120, 112, 96);
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        case "Bern":
                        case "Western":
                        case "Laus":
                        case "Tania":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        case "Ilia":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main7];
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        case "Sacae":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Scnd1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Scnd2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Scnd3];
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        case "Etruria":
                        case "Inmate":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main1];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        case "Member":
                            switch (color)
                            {
                                case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                case 1: return COUNTRY_COLORS[name][Color_Keys.Main3];
                                case 2: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                case 3: return new Color(152, 144, 136);
                                case 4: return new Color(200, 200, 192);
                                case 5: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                case 8: return new Color(248, 248, 16);
                            }
                            break;
                        default:
                            if (COUNTRY_COLORS.ContainsKey(name))
                            {
                                switch (color)
                                {
                                    case 0: return COUNTRY_COLORS[name][Color_Keys.Main2];
                                    case 1: return COUNTRY_COLORS[name][Color_Keys.Main4];
                                    case 2: return COUNTRY_COLORS[name][Color_Keys.Main5];
                                    case 3: return new Color(152, 144, 136);
                                    case 4: return new Color(200, 200, 192);
                                    case 5: return COUNTRY_COLORS[name][Color_Keys.Tert1];
                                    case 6: return COUNTRY_COLORS[name][Color_Keys.Tert2];
                                    case 7: return COUNTRY_COLORS[name][Color_Keys.Tert3];
                                    case 8: return new Color(248, 248, 16);
                                }
                            }
                            break;
                    }
                    break;
                #endregion
                #region 113: Minstrel
                case 113:
                    switch (name)
                    {
                        case "Deacon":
                            switch (color)
                            {
                                case 0: return new Color(56, 56, 64); // Main color
                                case 1: return new Color(64, 88, 112);
                                case 2: return new Color(80, 144, 152);
                                case 3: return new Color(144, 200, 176);
                                case 4: return new Color(248, 248, 248); // White
                                case 5: return new Color(184, 96, 40); // Hair
                                case 6: return new Color(248, 200, 56);
                                case 7: return new Color(248, 248, 128);
                                case 8: return new Color(80, 48, 120); // Lyre
                                case 9: return new Color(136, 88, 192);
                                case 10: return new Color(208, 176, 240);
                                case 11: return new Color(80, 48, 48); // Skin
                                case 12: return new Color(208, 168, 120);
                                case 13: return new Color(232, 208, 136);
                            }
                            break;
                    }
                    break;
                #endregion
                default:
                    key_found = false;
                    break;
            }
            }
            catch (KeyNotFoundException ex)
            {
                return null;
            }
            if (key_test && key_found)
                return new Color();
            return null;
        }
    }
}
