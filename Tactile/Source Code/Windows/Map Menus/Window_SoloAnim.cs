using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.ConfigData;
using Tactile.Graphics.Map;
using Tactile.Graphics.Text;
using TactileStringExtension;

namespace Tactile.Windows.Map
{
    class Window_SoloAnim : Window_Unit
    {
        const int Y_PER_ROW = 16;
        const int ROWS_AT_ONCE = (Config.WINDOW_HEIGHT - 72) / Y_PER_ROW;
        const int OFFSET_Y = -(16 - Y_PER_ROW) / 2;

        private int Sort = 0;
        private bool SortUp = true;

        #region Accessor Overrides
        protected override int _get_page()
        {
            return 0;
        }
        protected override void _set_page(int value) { }

        protected override int sort
        {
            get { return Sort; }
            set { Sort = value; }
        }
        protected override bool sort_up
        {
            get { return SortUp; }
            set { SortUp = value; }
        }

        protected override UnitScreenData[] DataSet { get { return UnitScreenConfig.SOLO_ANIM_DATA; } }

        public override bool HidesParent { get { return false; } }
        #endregion

        public Window_SoloAnim() : base()
        {
            Show_Page_Number = false;
        }

        #region Initialization
        protected override void InitializePositioning()
        {
            Y_Per_Row = Y_PER_ROW;
            Rows_At_Once = ROWS_AT_ONCE;
            Offset_Y = OFFSET_Y;
            Unit_Scissor_Rect = new Rectangle(-12, BASE_Y, 80 + Window_Unit.data_width + 16, ROWS_AT_ONCE * Y_PER_ROW);
            Header_Scissor_Rect = new Rectangle(76, BASE_Y - 16, Window_Unit.data_width, 16);
            Data_Scissor_Rect = new Rectangle(76, BASE_Y, Window_Unit.data_width, ROWS_AT_ONCE * Y_PER_ROW);
        }

        protected override bool DeterminePreparations()
        {
            return false;
        }

        protected override void refresh_banner_text()
        {
            Banner_Text.src_rect = new Rectangle(0, 16 * 7, 96, 16);
            Page_Number.text = (page + 1).ToString();
        }
        #endregion

        #region Update
        protected override void update_input(bool active)
        {
            // Change page
            if (!HeaderActive && Global.Input.triggered(Inputs.Left))
            {
                ChangeAnimation(false);
            }
            else if (!HeaderActive && Global.Input.triggered(Inputs.Right))
            {
                ChangeAnimation(true);
            }

            base.update_input(active);
        }
        #endregion

        protected override void UnitSelect()
        {
            ChangeAnimation(true);
        }

        private void ChangeAnimation(bool right)
        {
            if (right)
                this.actor.individual_animation++;
            else
                this.actor.individual_animation--;

            RefreshRow(UnitNodes.ActiveNodeIndex);

            if (this.actor.individual_animation == (int)Constants.Animation_Modes.Map)
                Global.game_system.play_se(System_Sounds.Cancel);
            else
                Global.game_system.play_se(System_Sounds.Confirm);
        }
    }
}
