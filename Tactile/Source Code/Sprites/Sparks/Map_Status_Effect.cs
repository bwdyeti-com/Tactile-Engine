using System.Collections.Generic;

namespace Tactile.Graphics.Map
{
    class Map_Status_Effect : Map_Effect
    {
        protected static List<int> STATUS_IDS;
        public static List<int> status_ids
        {
            get { return STATUS_IDS; }
            set
            {
                if (STATUS_IDS == null)
                    STATUS_IDS = value;
            }
        }

        public Map_Status_Effect(int type, int id)
        {
            Type = type;
            Id = id;
            int i = 0;//Global.game_map.status_anim_count;
            for(;;)
            {
                if (frame_time_max >= i)
                {
                    Frame_Time = i;
                    break;
                }
                else
                {
                    i -= frame_time_max;
                    if (data.animation_data.Count <= Frame + 1)
                        Frame = 0;
                    else
                        Frame++;
                }
            }
        }

        public override void update()
        {
            // Update image
            if (Frame_Time >= frame_time_max)
            {
                if (data.animation_data.Count <= Frame + 1)
                {
                    // switch to new animation here if needed
                    Frame = 0;
                    process_frame(Frame);
                    Frame_Time = 0;
                }
                else
                {
                    Frame++;
                    process_frame(Frame);
                    Frame_Time = 0;
                }
            }
            Frame_Time++;
            // Update sounds/effects
            Timer++;
            process_timing(Timer);
        }
    }
}
