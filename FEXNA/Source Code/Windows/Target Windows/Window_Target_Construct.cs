using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Windows.Target
{
    enum ConstructionModes { Assemble, Reload, Reclaim }
    class Window_Target_Construct : Window_Target_Location
    {
        protected List<Vector2> AssembleTargets;
        public ConstructionModes Mode { get; private set; }

        #region Accessors
        protected override int window_width
        {
            get { return 0; }
        }
        #endregion

        public Window_Target_Construct(int unit_id, ConstructionModes mode, Vector2 loc)
        {
            Mode = mode;
            initialize(loc);
            Unit_Id = unit_id;
            List<Vector2> targets;
            switch (Mode)
            {
                case ConstructionModes.Assemble:
                default:
                    targets = get_unit().assemble_targets();
                    break;
                case ConstructionModes.Reload:
                    targets = get_unit().reload_targets();
                    break;
                case ConstructionModes.Reclaim:
                    targets = get_unit().reclaim_targets();
                    break;
            }
            AssembleTargets = sort_targets(targets);
            Targets = new List<Vector2>();
            for (int i = 0; i < AssembleTargets.Count; i++)
                Targets.Add(AssembleTargets[i]);
            this.index = 0;
            Temp_Index = this.index;
            cursor_move_to(AssembleTargets[this.index]);

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
