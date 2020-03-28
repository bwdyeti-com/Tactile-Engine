using System;
using Microsoft.Xna.Framework;
using FEXNA_Library;

namespace FEXNA
{
    enum Stereoscopic_Mode { Left, Center, Right }
    abstract class Stereoscopic_Graphic_Object : Graphic_Object
    {
#if DEBUG
        const bool STEREO_TEST_ACTIVE = false;
        static int Stereoscopic_Test_Timer;
#endif

        protected static Stereoscopic_Mode Stereoscopic_View = Stereoscopic_Mode.Center;
        public static Stereoscopic_Mode stereoscopic_view { set { Stereoscopic_View = value; } }
        private static float STEREOSCOPIC_OFFSET
        {
            get
            {
                return STEREO_DEGREE * (Stereoscopic_View == Stereoscopic_Mode.Center ?
                    0 : (Stereoscopic_View == Stereoscopic_Mode.Left ? -1 : 1));
            }
        }
        static float STEREO_DEGREE
        {
            get
            {
                int stereoscopicLevel = Global.gameSettings.Graphics.StereoscopicLevel;
                return stereoscopicLevel / 5f;
                return stereoscopicLevel == 0 ? (1 / 8f) : //Debug
                    (float)Math.Pow(2, stereoscopicLevel - 2) / 2f;
            }
        }

        public static Vector2 graphic_draw_offset(Maybe<float> stereo_offset)
        {
            if (stereo_offset.IsNothing)
                return Vector2.Zero;
#if DEBUG
            if (STEREO_TEST_ACTIVE)
            {
                int timer = Stereoscopic_Test_Timer < 30 ? Stereoscopic_Test_Timer : 60 - Stereoscopic_Test_Timer;
                return new Vector2((stereo_offset + (timer / 5f)) * STEREOSCOPIC_OFFSET, 0);
            }
#endif
            return new Vector2(stereo_offset * STEREOSCOPIC_OFFSET, 0);
        }

        public static void update_stereoscopy()
        {
#if DEBUG
            if (STEREO_TEST_ACTIVE)
                Stereoscopic_Test_Timer = (Stereoscopic_Test_Timer + 1) % 60;
#endif
        }

        protected Maybe<float> Stereo_Offset = new Maybe<float>();

        /// <summary>
        /// Copies the stereoscopy of this object onto another object
        /// </summary>
        /// <param name="other"></param>
        protected void copy_stereo(Stereoscopic_Graphic_Object other)
        {
            other.Stereo_Offset = Stereo_Offset;
        }

        #region Accessors
        // Remove virtual from this, and everything that overrides it to assign stereo
        // values to properties should instead adjust the draw_offset of those properties//Debug
        public virtual float stereoscopic { set { Stereo_Offset = value; } }
        #endregion

        protected virtual Vector2 stereo_offset()
        {
            return graphic_draw_offset(Stereo_Offset);
        }
        protected Vector2 draw_vector()
        {
            return draw_offset + stereo_offset();
        }
    }
}
