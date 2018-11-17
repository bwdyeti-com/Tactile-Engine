#if DEBUG && WINDOWS
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Debug_Monitor
{
    class DebugMonitorRankingPage : DebugMonitorPage
    {
        public DebugMonitorRankingPage()
        {
            // Labels
            DebugStringDisplay par_label = new DebugStringDisplay(
                () => "Par", 80, text_color: "Yellow");
            par_label.loc = new Vector2(120, 0);
            DebugDisplays.Add(par_label);
            DebugStringDisplay score_label = new DebugStringDisplay(
                () => "Current", 80, text_color: "Yellow");
            score_label.loc = new Vector2(152, 0);
            DebugDisplays.Add(score_label);
            DebugStringDisplay projection_label = new DebugStringDisplay(
                () => "Projection", 80, text_color: "Yellow");
            projection_label.loc = new Vector2(192, 0);
            DebugDisplays.Add(projection_label);

            // Current turn
            DebugIntDisplay turn = new DebugIntDisplay(
                () => !Global.map_exists ? -1 : Global.game_state.turn,
                "Turn", 5, 56);
            turn.loc = new Vector2(0, 16);
            DebugDisplays.Add(turn);
            DebugIntDisplay turn_par = new DebugIntDisplay(
                () => Global.game_state.chapter == null ? -1 :
                    Global.game_state.chapter.Ranking_Turns,
                "", 5, 0);
            turn_par.loc = new Vector2(104, 16);
            DebugDisplays.Add(turn_par);
            DebugIntDisplay turn_rank = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null)
                        return 0;
                    return new Game_Ranking().turns;
                },
                "", 3, 0);
            turn_rank.loc = new Vector2(160, 16);
            DebugDisplays.Add(turn_rank);
            DebugIntDisplay turn_projection = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null || !Global.map_exists)
                        return 0;
                    var ranking = Global.game_state.calculate_ranking_progress();
                    if (ranking == null)
                        return 0;
                    return ranking.turns;
                },
                "", 3, 0);
            turn_projection.loc = new Vector2(200, 16);
            DebugDisplays.Add(turn_projection);

            // Damage taken
            DebugIntDisplay damage_taken = new DebugIntDisplay(
                () => !Global.map_exists ?
                    -1 : Global.game_system.chapter_damage_taken,
                "Dmg taken", 5, 56);
            damage_taken.loc = new Vector2(0, 32);
            DebugDisplays.Add(damage_taken);
            DebugIntDisplay damage_par = new DebugIntDisplay(
                () => Global.game_state.chapter == null ? -1 :
                    Global.game_state.chapter.Ranking_Combat,
                "", 5, 0);
            damage_par.loc = new Vector2(104, 32);
            DebugDisplays.Add(damage_par);
            DebugIntDisplay damage_rank = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null)
                        return 0;
                    return new Game_Ranking().combat;
                },
                "", 3, 0);
            damage_rank.loc = new Vector2(160, 32);
            DebugDisplays.Add(damage_rank);
            DebugIntDisplay damage_projection = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null || !Global.map_exists)
                        return 0;
                    var ranking = Global.game_state.calculate_ranking_progress();
                    if (ranking == null)
                        return 0;
                    return ranking.combat;
                },
                "", 3, 0);
            damage_projection.loc = new Vector2(200, 32);
            DebugDisplays.Add(damage_projection);

            // Exp gained
            DebugIntDisplay exp_gained = new DebugIntDisplay(
                () => !Global.map_exists ?
                    -1 : Global.game_system.chapter_exp_gain,
                "Experience", 5, 56);
            exp_gained.loc = new Vector2(0, 48);
            DebugDisplays.Add(exp_gained);
            DebugIntDisplay exp_par = new DebugIntDisplay(
                () => Global.game_state.chapter == null ? -1 :
                    Global.game_state.chapter.Ranking_Exp,
                "", 5, 0);
            exp_par.loc = new Vector2(104, 48);
            DebugDisplays.Add(exp_par);
            DebugIntDisplay exp_rank = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null)
                        return 0;
                    return new Game_Ranking().exp;
                },
                "", 3, 0);
            exp_rank.loc = new Vector2(160, 48);
            DebugDisplays.Add(exp_rank);
            DebugIntDisplay exp_projection = new DebugIntDisplay(
                () =>
                {
                    if (Global.game_state.chapter == null || !Global.map_exists)
                        return 0;
                    var ranking = Global.game_state.calculate_ranking_progress();
                    if (ranking == null)
                        return 0;
                    return ranking.exp;
                },
                "", 3, 0);
            exp_projection.loc = new Vector2(200, 48);
            DebugDisplays.Add(exp_projection);

            // Completion
            DebugIntDisplay completion = new DebugIntDisplay(
                () => !Global.map_exists ?
                    -1 : Global.game_system.chapter_completion,
                "Completion", 5, 56);
            completion.loc = new Vector2(0, 64);
            DebugDisplays.Add(completion);
            DebugIntDisplay completion_par = new DebugIntDisplay(
                () => Global.game_state.chapter == null ? -1 :
                    Global.game_state.chapter.Ranking_Completion,
                "", 5, 0);
            completion_par.loc = new Vector2(104, 64);
            DebugDisplays.Add(completion_par);

            // Deaths
            DebugIntDisplay deaths = new DebugIntDisplay(
                () => !Global.map_exists ?
                    -1 : Global.game_system.chapter_deaths,
                "Death", 5, 56);
            deaths.loc = new Vector2(0, 80);
            DebugDisplays.Add(deaths);
        }
    }
}
#endif