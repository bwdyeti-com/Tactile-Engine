using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Tactile
{
    public enum Face_Color_Keys
    {
        Outline, Skin1, Skin2, Skin3, Skin4, Skin5, Main1, Main2, Main3, Main4, Main5,
        Hair1, Hair2, Hair3, Tert1, Tert2, Tert3, Tert4
    }
    public class Face_Recolor
    {
        #region Country Color Data
        public readonly static Dictionary<string, Dictionary<Face_Color_Keys, Color>> COUNTRY_COLORS =
            new Dictionary<string, Dictionary<Face_Color_Keys, Color>>
        {
            #region Default
            {"Default", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96)},
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88)},
                    { Face_Color_Keys.Skin2,   new Color(184, 120,  56)},
                    { Face_Color_Keys.Skin3,   new Color(232, 176,  88)},
                    { Face_Color_Keys.Skin4,   new Color(240, 224, 128)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 200)},
                    { Face_Color_Keys.Main1,   new Color(112,  72,  96)},
                    { Face_Color_Keys.Main2,   new Color(144,  80, 112)},
                    { Face_Color_Keys.Main3,   new Color(168,  88, 136)},
                    { Face_Color_Keys.Main4,   new Color(184, 120, 152)},
                    { Face_Color_Keys.Main5,   new Color(208, 168, 176)},
                    { Face_Color_Keys.Hair1,   new Color(176, 120,  32)},
                    { Face_Color_Keys.Hair2,   new Color(208, 168,  32)},
                    { Face_Color_Keys.Hair3,   new Color(248, 208,  64)},
                    { Face_Color_Keys.Tert1,   new Color(112, 104, 120)},
                    { Face_Color_Keys.Tert2,   new Color(144, 136, 152)},
                    { Face_Color_Keys.Tert3,   new Color(168, 168, 176)},
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224)}
                }
            },
            #endregion
            #region Bern
            {"Bern", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Skin2,   new Color(184, 120,  56)},
                    { Face_Color_Keys.Skin3,   new Color(232, 176,  88)},
                    { Face_Color_Keys.Skin4,   new Color(240, 224, 128)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 200)},
                    { Face_Color_Keys.Main1,   new Color(112,  72,  96)},
                    { Face_Color_Keys.Main2,   new Color(144,  80, 112)},
                    { Face_Color_Keys.Main3,   new Color(168,  88, 136)},
                    { Face_Color_Keys.Main4,   new Color(184, 120, 152)},
                    { Face_Color_Keys.Main5,   new Color(208, 168, 176)},
                    { Face_Color_Keys.Hair1,   new Color(176, 120,  32)},
                    { Face_Color_Keys.Hair2,   new Color(208, 168,  32)},
                    { Face_Color_Keys.Hair3,   new Color(248, 208,  64)},
                    { Face_Color_Keys.Tert1,   new Color(112, 104, 120)},
                    { Face_Color_Keys.Tert2,   new Color(144, 136, 152)},
                    { Face_Color_Keys.Tert3,   new Color(168, 168, 176)},
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224)}
                }
            },
            #endregion
            #region Etruria
            {"Etruria", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(192, 120,  64)},
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88)},
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216)},
                    { Face_Color_Keys.Main1,   new Color( 88,  72,  96) },
                    { Face_Color_Keys.Main2,   new Color( 96,  88, 104) },
                    { Face_Color_Keys.Main3,   new Color(112, 104, 128) },
                    { Face_Color_Keys.Main4,   new Color(128, 120, 152) },
                    { Face_Color_Keys.Main5,   new Color(152, 152, 168) },
                    { Face_Color_Keys.Hair1,   new Color(176, 152, 56) },
                    { Face_Color_Keys.Hair2,   new Color(216, 208, 48) },
                    { Face_Color_Keys.Hair3,   new Color(248, 248, 104) },
                    { Face_Color_Keys.Tert1,   new Color(112, 104, 128) },
                    { Face_Color_Keys.Tert2,   new Color(152, 144, 160) },
                    { Face_Color_Keys.Tert3,   new Color(192, 176, 192) },
                    { Face_Color_Keys.Tert4,   new Color(224, 212, 224) }
                }
            },
            #endregion
            #region Sacae
            {"Sacae", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(200, 120,  64) },
                    { Face_Color_Keys.Skin3,   new Color(240, 168,  72) },
                    { Face_Color_Keys.Skin4,   new Color(248, 224, 104) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 192) },
                    { Face_Color_Keys.Main1,   new Color( 96,  80,  72) },
                    { Face_Color_Keys.Main2,   new Color(112,  88,  72) },
                    { Face_Color_Keys.Main3,   new Color(144, 104,  64) },
                    { Face_Color_Keys.Main4,   new Color(176, 136,  88) },
                    { Face_Color_Keys.Main5,   new Color(192, 168, 128) },
                    { Face_Color_Keys.Hair1,   new Color( 64,  96,  72) },
                    { Face_Color_Keys.Hair2,   new Color( 64, 128,  56) },
                    { Face_Color_Keys.Hair3,   new Color(104, 160,  84) },
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136) },
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168) },
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200) },
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224) }
                }
            },
            #endregion
            #region Western
            {"Western", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color(120,  56,  64)},
                    { Face_Color_Keys.Main2,   new Color(152,  56,  64)},
                    { Face_Color_Keys.Main3,   new Color(192,  64,  64)},
                    { Face_Color_Keys.Main4,   new Color(200, 104,  96)},
                    { Face_Color_Keys.Main5,   new Color(208, 136, 120)},
                    { Face_Color_Keys.Hair1,   new Color( 88,  80,  72)},
                    { Face_Color_Keys.Hair2,   new Color(120,  96,  64)},
                    { Face_Color_Keys.Hair3,   new Color(152, 120,  80)},
                    { Face_Color_Keys.Tert1,   new Color(112, 112, 104)},
                    { Face_Color_Keys.Tert2,   new Color(136, 136, 128)},
                    { Face_Color_Keys.Tert3,   new Color(176, 176, 168)},
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 208)},
                }
            },
            #endregion
            #region Ilia
            {"Ilia", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96)},
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88)},
                    { Face_Color_Keys.Skin2,   new Color(192, 120,  64)},
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88)},
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216)},
                    { Face_Color_Keys.Main1,   new Color( 96,  72, 120)},
                    { Face_Color_Keys.Main2,   new Color(128,  80, 144)},
                    { Face_Color_Keys.Main3,   new Color(152,  96, 176)},
                    { Face_Color_Keys.Main4,   new Color(184, 128, 192)},
                    { Face_Color_Keys.Main5,   new Color(216, 168, 208)},
                    { Face_Color_Keys.Hair1,   new Color( 64,  96, 128)},
                    { Face_Color_Keys.Hair2,   new Color( 48, 128, 152)},
                    { Face_Color_Keys.Hair3,   new Color( 64, 160, 192)},
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136)},
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168)},
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200)},
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224)}
                }
            },
            #endregion
            #region Bandit
            {"Bandit", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Skin2,   new Color(184, 104,  48)},
                    { Face_Color_Keys.Skin3,   new Color(224, 144,  72)},
                    { Face_Color_Keys.Skin4,   new Color(248, 200,  96)},
                    { Face_Color_Keys.Skin5,   new Color(248, 240, 160)},
                    { Face_Color_Keys.Main1,   new Color( 88,  80,  72)},
                    { Face_Color_Keys.Main2,   new Color(104,  96,  88)},
                    { Face_Color_Keys.Main3,   new Color(120, 112,  88)},
                    { Face_Color_Keys.Main4,   new Color(144, 136, 104)},
                    { Face_Color_Keys.Main5,   new Color(168, 160, 136)},
                    { Face_Color_Keys.Hair1,   new Color( 96,  88,  64)},
                    { Face_Color_Keys.Hair2,   new Color(144, 120,  56)},
                    { Face_Color_Keys.Hair3,   new Color(184, 152,  72)},
                    { Face_Color_Keys.Tert1,   new Color( 96,  96,  96)},
                    { Face_Color_Keys.Tert2,   new Color(112, 112, 104)},
                    { Face_Color_Keys.Tert3,   new Color(144, 136, 128)},
                    { Face_Color_Keys.Tert4,   new Color(184, 176, 168)},
                }
            },
            #endregion
            #region Ostia
            {"Ostia", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color( 72,  64, 112) },
                    { Face_Color_Keys.Main2,   new Color( 72,  72, 144) },
                    { Face_Color_Keys.Main3,   new Color( 80,  80, 176) },
                    { Face_Color_Keys.Main4,   new Color(120, 128, 200) },
                    { Face_Color_Keys.Main5,   new Color(152, 168, 208) },
                    { Face_Color_Keys.Hair1,   new Color( 88,  88, 112) },
                    { Face_Color_Keys.Hair2,   new Color( 96, 104, 152) },
                    { Face_Color_Keys.Hair3,   new Color(128, 128, 184) },
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136) },
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168) },
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200) },
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224) }
                }
            },
            #endregion
            #region Laus
            {"Laus", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color(112,  72,  88) },
                    { Face_Color_Keys.Main2,   new Color(144,  72,  88) },
                    { Face_Color_Keys.Main3,   new Color(184,  80,  88) },
                    { Face_Color_Keys.Main4,   new Color(208, 112, 112) },
                    { Face_Color_Keys.Main5,   new Color(216, 152, 144) },
                    { Face_Color_Keys.Hair1,   new Color(112,  88,  64) },
                    { Face_Color_Keys.Hair2,   new Color(144, 128,  64) },
                    { Face_Color_Keys.Hair3,   new Color(184, 176,  64) },
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136) },
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168) },
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200) },
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224) }
                }
            },
            #endregion
            #region Thria
            {"Thria", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color( 64,  88, 120) },
                    { Face_Color_Keys.Main2,   new Color( 72, 120, 152) },
                    { Face_Color_Keys.Main3,   new Color( 96, 144, 176) },
                    { Face_Color_Keys.Main4,   new Color(128, 176, 200) },
                    { Face_Color_Keys.Main5,   new Color(152, 200, 208) },
                    { Face_Color_Keys.Hair1,   new Color( 88,  72, 120) },
                    { Face_Color_Keys.Hair2,   new Color(104,  88, 144) },
                    { Face_Color_Keys.Hair3,   new Color(136, 120, 176) },
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136) },
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168) },
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200) },
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224) }
                }
            },
            #endregion
            #region Tania
            {"Tania", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(200, 112,  56) },
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88) },
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216) },
                    { Face_Color_Keys.Main1,   new Color(112,  64,  64) },
                    { Face_Color_Keys.Main2,   new Color(144,  72,  48) },
                    { Face_Color_Keys.Main3,   new Color(192, 104,  56) },
                    { Face_Color_Keys.Main4,   new Color(208, 144,  80) },
                    { Face_Color_Keys.Main5,   new Color(216, 176, 128) },
                    { Face_Color_Keys.Hair1,   new Color(184, 144,  48) },
                    { Face_Color_Keys.Hair2,   new Color(224, 192,  80) },
                    { Face_Color_Keys.Hair3,   new Color(248, 240, 104) },
                    { Face_Color_Keys.Tert1,   new Color(128, 128, 136) },
                    { Face_Color_Keys.Tert2,   new Color(160, 160, 168) },
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200) },
                    { Face_Color_Keys.Tert4,   new Color(216, 216, 224) }
                }
            },
            #endregion
            #region Tuscana
            {"Tuscana", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color( 72,  80, 104) },
                    { Face_Color_Keys.Main2,   new Color( 88,  88, 128) },
                    { Face_Color_Keys.Main3,   new Color(120, 120, 160) },
                    { Face_Color_Keys.Main4,   new Color(160, 152, 200) },
                    { Face_Color_Keys.Main5,   new Color(192, 184, 224) },
                    { Face_Color_Keys.Hair1,   new Color( 80,  40, 120) },
                    { Face_Color_Keys.Hair2,   new Color( 96,  56, 144) },
                    { Face_Color_Keys.Hair3,   new Color(128,  88, 176) },
                    { Face_Color_Keys.Tert1,   new Color(104, 120, 112) },
                    { Face_Color_Keys.Tert2,   new Color(136, 152, 136) },
                    { Face_Color_Keys.Tert3,   new Color(176, 184, 160) },
                    { Face_Color_Keys.Tert4,   new Color(224, 224, 208) }
                }
            },
            #endregion
            #region Pherae
            {"Pherae", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(200, 112,  56) },
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88) },
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216) },
                    { Face_Color_Keys.Main1,   new Color( 80,  80, 128) },
                    { Face_Color_Keys.Main2,   new Color( 80,  88, 176) },
                    { Face_Color_Keys.Main3,   new Color( 88, 112, 232) },
                    { Face_Color_Keys.Main4,   new Color(112, 144, 248) },
                    { Face_Color_Keys.Main5,   new Color(176, 200, 232) },
                    { Face_Color_Keys.Hair1,   new Color(128,  56,  72) },
                    { Face_Color_Keys.Hair2,   new Color(168,  48,  56) },
                    { Face_Color_Keys.Hair3,   new Color(200,  56,  64) },
                }
            },
            #endregion
            #region Caelin
            {"Caelin", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Main1,   new Color( 24,  96,  96)},
                    { Face_Color_Keys.Main2,   new Color( 40, 120, 104)},
                    { Face_Color_Keys.Main3,   new Color( 48, 152,  96)},
                    { Face_Color_Keys.Main4,   new Color( 72, 176, 112)},
                    { Face_Color_Keys.Main5,   new Color(128, 192, 152)},
                    { Face_Color_Keys.Hair1,   new Color(104, 128,  96)},
                    { Face_Color_Keys.Hair2,   new Color(144, 168,  88)},
                    { Face_Color_Keys.Hair3,   new Color(200, 200,  88)},
                    { Face_Color_Keys.Tert1,   new Color(128, 120, 136)},
                    { Face_Color_Keys.Tert2,   new Color(160, 152, 168)},
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200)},
                    { Face_Color_Keys.Tert4,   new Color(216, 224, 208)},
                }
            },
            #endregion
            #region Rebel
            {"Rebel", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(200, 112,  56) },
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88) },
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216) },
                    { Face_Color_Keys.Main1,   new Color(112,  64,  88) },
                    { Face_Color_Keys.Main2,   new Color(144,  72,  80) },
                    { Face_Color_Keys.Main3,   new Color(168,  80,  80) },
                    { Face_Color_Keys.Main4,   new Color(192, 112, 112) },
                    { Face_Color_Keys.Main5,   new Color(216, 152, 152) },
                    { Face_Color_Keys.Hair1,   new Color(128,  56,  72) },
                    { Face_Color_Keys.Hair2,   new Color(168,  48,  56) },
                    { Face_Color_Keys.Hair3,   new Color(200,  56,  64) },
                    { Face_Color_Keys.Tert1,   new Color( 96,  96,  96) },
                    { Face_Color_Keys.Tert2,   new Color(112, 112, 104) },
                    { Face_Color_Keys.Tert3,   new Color(144, 136, 128) },
                    { Face_Color_Keys.Tert4,   new Color(184, 176, 168) },
                }
            },
            #endregion
            #region Killer
            { "Killer", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(184, 120,  56) },
                    { Face_Color_Keys.Skin3,   new Color(232, 176,  88) },
                    { Face_Color_Keys.Skin4,   new Color(240, 224, 128) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 200) },
                    { Face_Color_Keys.Main1,   new Color(120,  80,  40) },
                    { Face_Color_Keys.Main2,   new Color(136,  96,  56) },
                    { Face_Color_Keys.Main3,   new Color(160, 128,  72) },
                    { Face_Color_Keys.Main4,   new Color(192, 160,  72) },
                    { Face_Color_Keys.Main5,   new Color(216, 184, 104) },
                    { Face_Color_Keys.Hair1,   new Color(104,  56,  56) },
                    { Face_Color_Keys.Hair2,   new Color(144,  64,  56) },
                    { Face_Color_Keys.Hair3,   new Color(176,  80,  64) },
                    { Face_Color_Keys.Tert1,   new Color(104, 120, 112) },
                    { Face_Color_Keys.Tert2,   new Color(136, 152, 136) },
                    { Face_Color_Keys.Tert3,   new Color(176, 184, 160) },
                    { Face_Color_Keys.Tert4,   new Color(224, 224, 208) }
                }
            },
            #endregion
            #region Member
            {"Member", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(184, 120,  56) },
                    { Face_Color_Keys.Skin3,   new Color(232, 176,  88) },
                    { Face_Color_Keys.Skin4,   new Color(240, 224, 128) },
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 200) },
                    { Face_Color_Keys.Main1,   new Color(104,  88,  96) },
                    { Face_Color_Keys.Main2,   new Color(128, 104, 104) },
                    { Face_Color_Keys.Main3,   new Color(160, 136, 136) },
                    { Face_Color_Keys.Main4,   new Color(200, 176, 168) },
                    { Face_Color_Keys.Main5,   new Color(224, 208, 200) },
                    { Face_Color_Keys.Hair1,   new Color(184, 144,  48) },//{ Face_Color_Keys.Hair1,   new Color(176, 136,  48) },
                    { Face_Color_Keys.Hair2,   new Color(224, 192,  80) },//{ Face_Color_Keys.Hair2,   new Color(224, 200,  40) },
                    { Face_Color_Keys.Hair3,   new Color(248, 240, 104) },//{ Face_Color_Keys.Hair3,   new Color(248, 240,  96) },
                    { Face_Color_Keys.Tert1,   new Color(104, 120, 112) },
                    { Face_Color_Keys.Tert2,   new Color(136, 152, 136) },
                    { Face_Color_Keys.Tert3,   new Color(176, 184, 160) },
                    { Face_Color_Keys.Tert4,   new Color(224, 224, 208) }
                }
            },
            #endregion
            #region Inmate
            {"Inmate", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96) },
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88) },
                    { Face_Color_Keys.Skin2,   new Color(192, 120,  64)},
                    { Face_Color_Keys.Skin3,   new Color(232, 160,  88)},
                    { Face_Color_Keys.Skin4,   new Color(248, 216, 152)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 216)},
                    { Face_Color_Keys.Main1,   new Color( 88,  80,  96) },
                    { Face_Color_Keys.Main2,   new Color(104,  96, 104) },
                    { Face_Color_Keys.Main3,   new Color(120, 112, 120) },
                    { Face_Color_Keys.Main4,   new Color(136, 128, 128) },
                    { Face_Color_Keys.Main5,   new Color(168, 160, 152) },
                    { Face_Color_Keys.Hair1,   new Color( 96,  80,  64) },
                    { Face_Color_Keys.Hair2,   new Color(128, 104,  64) },
                    { Face_Color_Keys.Hair3,   new Color(168, 136,  88) },
                    { Face_Color_Keys.Tert1,   new Color( 96,  96,  96) },
                    { Face_Color_Keys.Tert2,   new Color(112, 112, 104) },
                    { Face_Color_Keys.Tert3,   new Color(144, 136, 128) },
                    { Face_Color_Keys.Tert4,   new Color(184, 176, 168) }
                }
            },
            #endregion
            #region Gladiator
            {"Gladiator", new Dictionary<Face_Color_Keys, Color>
                {
                    { Face_Color_Keys.Outline, new Color( 88,  64,  96)},
                    { Face_Color_Keys.Skin1,   new Color(112,  88,  88)},
                    { Face_Color_Keys.Skin2,   new Color(200, 120,  64)},
                    { Face_Color_Keys.Skin3,   new Color(240, 176,  72)},
                    { Face_Color_Keys.Skin4,   new Color(248, 224, 104)},
                    { Face_Color_Keys.Skin5,   new Color(248, 248, 192)},
                    { Face_Color_Keys.Main1,   new Color( 56,  96,  88)},
                    { Face_Color_Keys.Main2,   new Color( 72, 120,  96)},
                    { Face_Color_Keys.Main3,   new Color( 80, 152,  88)},
                    { Face_Color_Keys.Main4,   new Color(104, 176, 104)},
                    { Face_Color_Keys.Main5,   new Color(160, 192, 144)},
                    { Face_Color_Keys.Hair1,   new Color( 96,  88,  64)},
                    { Face_Color_Keys.Hair2,   new Color(144, 120,  56)},
                    { Face_Color_Keys.Hair3,   new Color(184, 152,  72)},
                    { Face_Color_Keys.Tert1,   new Color(128, 120, 136)},
                    { Face_Color_Keys.Tert2,   new Color(160, 152, 168)},
                    { Face_Color_Keys.Tert3,   new Color(192, 192, 200)},
                    { Face_Color_Keys.Tert4,   new Color(216, 224, 208)},
                }
            }
            #endregion
        };
        #endregion
    }
}
