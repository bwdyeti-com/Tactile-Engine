using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEXNA.Menus.Preparations
{
    class PromotionFadeMenu : ScreenFadeMenu
    {
        const int PROMOTION_TIME = 8;

        private int ActorId, ItemIndex;

        public PromotionFadeMenu(int actorId, int itemIndex)
            : base(PROMOTION_TIME, 0, 0, true)
        {
            ActorId = actorId;
            ItemIndex = itemIndex;
        }

        public void CallPromotion()
        {
            Global.game_system.Preparations_Actor_Id = ActorId;
            Global.game_map.add_actor_unit(Constants.Team.PLAYER_TEAM, Config.OFF_MAP, ActorId, "");
            Global.game_state.call_item(Global.game_map.last_added_unit.id, ItemIndex);
            Global.game_temp.preparations_item_index = ItemIndex;
        }

        public static ScreenFadeMenu PromotionEndFade()
        {
            return new ScreenFadeMenu(0, 0, PROMOTION_TIME, false);
        }
    }
}
