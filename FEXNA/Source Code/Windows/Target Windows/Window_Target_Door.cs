using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.Target
{
    class Window_Target_Door : Window_Target_Location
    {
        protected List<Vector2> Door_Targets;

        #region Accessors
        protected override int window_width
        {
            get { return 0; }
        }
        #endregion

        public Window_Target_Door(int unit_id, Vector2 loc)
        {
            initialize(loc);
            Unit_Id = unit_id;
            List<Vector2> targets = get_unit().door_targets();
            Door_Targets = sort_targets(targets);
            Targets = new List<Vector2>();
            for (int i = 0; i < Door_Targets.Count; i++)
                Targets.Add(Door_Targets[i]);
            this.index = 0;
            Temp_Index = this.index;
            cursor_move_to(Door_Targets[this.index]);

            Global.player.instant_move = true;
            Global.player.update_movement();
            refresh();
            index = this.index;
        }

        protected override void set_images() { }

        protected override void refresh() { }

        public override void draw(SpriteBatch sprite_batch) { }
    }
}
