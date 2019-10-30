using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA_Library;
using FEXNAWeaponExtension;

namespace FEXNA.Windows.Target
{
    enum ConstructionModes { Assemble, Reload, Reclaim }
    class Window_Target_Construct : Window_Target_Location
    {
        protected List<Vector2> AssembleTargets;
        public ConstructionModes Mode { get; private set; }
        private Maybe<int> WeaponId;

        #region Accessors
        protected override int window_width
        {
            get { return 0; }
        }
        #endregion

        public Window_Target_Construct(int unit_id, ConstructionModes mode, Vector2 loc, Maybe<int> weaponId = default(Maybe<int>))
        {
            Mode = mode;
            initialize(loc);
            Unit_Id = unit_id;
            WeaponId = weaponId;
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

        protected override void refresh()
        {
            RefreshSiegeRange();
        }

        protected override void reset_cursor()
        {
            base.reset_cursor();

            RefreshSiegeRange();
        }

        private void RefreshSiegeRange()
        {
            if (Mode == ConstructionModes.Assemble)
            {
                var weapon = Global.data_weapons[WeaponId];
                Global.game_temp.temp_attack_range = Global.game_map.get_unit_range(
                    new HashSet<Vector2> { Global.player.loc },
                    weapon.Min_Range,
                    weapon.Max_Range,
                    weapon.range_blocked_by_walls());
                Global.game_map.range_start_timer = 0;
            }
            else
                Global.game_temp.temp_attack_range.Clear();
        }
        
        public override void draw(SpriteBatch sprite_batch) { }
    }
}
