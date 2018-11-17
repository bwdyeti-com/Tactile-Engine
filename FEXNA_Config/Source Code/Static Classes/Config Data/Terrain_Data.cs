using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    public class Terrain_Data
    {
        public readonly static Dictionary<int, Dictionary<Vector2, Vector2>> PILLAGE_TERRAIN_CHANGE = new Dictionary<int,Dictionary<Vector2,Vector2>>
        {
            #region 0: Overworld
            { 0, new Dictionary<Vector2, Vector2>
                {
                    { new Vector2(7, 25), new Vector2(7, 28) },
                    { new Vector2(7, 26), new Vector2(7, 29) },
                    { new Vector2(7, 27), new Vector2(7, 30) },
                    { new Vector2(0, 57), new Vector2(0, 60) },
                    { new Vector2(0, 58), new Vector2(0, 61) },
                    { new Vector2(0, 59), new Vector2(0, 62) },
                    { new Vector2(1, 57), new Vector2(1, 60) },
                    { new Vector2(1, 58), new Vector2(1, 61) },
                    { new Vector2(1, 59), new Vector2(1, 62) }
                }
            },
            #endregion
            #region 8: Mountain_Village
            { 8, new Dictionary<Vector2, Vector2>
                {
                    // Single Red House
                    { new Vector2( 0, 29), new Vector2(4, 23) },
                    { new Vector2( 1, 29), new Vector2(5, 23) },
                    { new Vector2( 2, 29), new Vector2(6, 23) },
                    { new Vector2( 0, 30), new Vector2(4, 24) },
                    { new Vector2( 1, 30), new Vector2(5, 24) },
                    { new Vector2( 2, 30), new Vector2(6, 24) },
                    // Red house next to red house
                    { new Vector2( 6, 27), new Vector2(0, 25) },
                    { new Vector2( 7, 27), new Vector2(1, 25) },
                    { new Vector2( 8, 27), new Vector2(2, 25) }, // Both //Yeti
                    { new Vector2( 9, 27), new Vector2(3, 25) },
                    { new Vector2(10, 27), new Vector2(4, 25) },
                    { new Vector2( 6, 28), new Vector2(0, 26) },
                    { new Vector2( 7, 28), new Vector2(1, 26) },
                    { new Vector2( 8, 28), new Vector2(2, 26) }, // Both //Yeti
                    { new Vector2( 9, 28), new Vector2(3, 26) },
                    { new Vector2(10, 28), new Vector2(4, 26) },
                    // Red house left of brown (2 chimney)
                    { new Vector2( 8, 25), new Vector2(0, 25) },
                    { new Vector2( 9, 25), new Vector2(1, 25) },
                    { new Vector2(10, 25), new Vector2(1, 23) },
                    { new Vector2( 8, 26), new Vector2(0, 26) },
                    { new Vector2( 9, 26), new Vector2(1, 26) },
                    { new Vector2(10, 26), new Vector2(1, 24) },
                    // Red house left of brown (1 chimney)
                    { new Vector2(11, 57), new Vector2(1, 23) },
                    { new Vector2(11, 58), new Vector2(1, 24) },
                    // Red house right of brown (2 chimney)
                    { new Vector2( 5, 25), new Vector2(0, 23) },
                    { new Vector2( 6, 25), new Vector2(3, 25) },
                    { new Vector2( 7, 25), new Vector2(4, 25) },
                    { new Vector2( 5, 26), new Vector2(0, 24) },
                    { new Vector2( 6, 26), new Vector2(3, 26) },
                    { new Vector2( 7, 26), new Vector2(4, 26) },
                    // Red house right of brown (1 chimney)
                    { new Vector2( 8, 29), new Vector2(2, 23) },
                    { new Vector2( 8, 30), new Vector2(2, 24) },
                    // Misc
                    { new Vector2(12, 31), new Vector2(4, 24) },
                    { new Vector2(14, 30), new Vector2(6, 23) },
                    { new Vector2(13, 31), new Vector2(4, 23) },
                    { new Vector2(14, 31), new Vector2(5, 23) },
                    { new Vector2(15, 31), new Vector2(6, 23) },
                    { new Vector2(16, 31), new Vector2(6, 24) }
                }
            },
            #endregion
            #region 9: Coastal Castle
            { 9, new Dictionary<Vector2, Vector2>
                {
                    // Single Red House
                    { new Vector2(0, 29), new Vector2(4, 23) },
                    { new Vector2(1, 29), new Vector2(5, 23) },
                    { new Vector2(2, 29), new Vector2(6, 23) },
                    { new Vector2(0, 30), new Vector2(4, 24) },
                    { new Vector2(1, 30), new Vector2(5, 24) },
                    { new Vector2(2, 30), new Vector2(6, 24) },
                    // Red house next to red house
                    { new Vector2(6, 27), new Vector2(0, 25) },
                    { new Vector2(7, 27), new Vector2(1, 25) },
                    { new Vector2(0, 59), new Vector2(2, 25) }, // Both //Yeti
                    { new Vector2(1, 59), new Vector2(3, 25) },
                    { new Vector2(2, 59), new Vector2(4, 25) },
                    { new Vector2(6, 28), new Vector2(0, 26) },
                    { new Vector2(7, 28), new Vector2(1, 26) },
                    { new Vector2(0, 60), new Vector2(2, 26) }, // Both //Yeti
                    { new Vector2(1, 60), new Vector2(3, 26) },
                    { new Vector2(2, 60), new Vector2(4, 26) },
                    // Red house left of brown (2 chimney)
                    { new Vector2(0, 57), new Vector2(0, 25) },
                    { new Vector2(1, 57), new Vector2(1, 25) },
                    { new Vector2(2, 57), new Vector2(1, 23) },
                    { new Vector2(0, 58), new Vector2(0, 26) },
                    { new Vector2(1, 58), new Vector2(1, 26) },
                    { new Vector2(2, 58), new Vector2(1, 24) },
                    // Red house left of brown (1 chimney)
                    { new Vector2(3, 57), new Vector2(1, 23) },
                    { new Vector2(3, 58), new Vector2(1, 24) },
                    // Red house right of brown (2 chimney)
                    { new Vector2(5, 25), new Vector2(0, 23) },
                    { new Vector2(6, 25), new Vector2(3, 25) },
                    { new Vector2(7, 25), new Vector2(4, 25) },
                    { new Vector2(5, 26), new Vector2(0, 24) },
                    { new Vector2(6, 26), new Vector2(3, 26) },
                    { new Vector2(7, 26), new Vector2(4, 26) },
                    // Red house right of brown (1 chimney)
                    { new Vector2(0, 61), new Vector2(2, 23) },
                    { new Vector2(0, 62), new Vector2(2, 24) },
                    // Misc
                    { new Vector2(4, 63), new Vector2(4, 24) },
                    { new Vector2(6, 62), new Vector2(6, 23) }
                }
            },
            #endregion
        };
    }
}
