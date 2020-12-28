using System.Collections.Generic;
using Tactile.ConfigData;

namespace Tactile.Constants
{
    public enum Animation_Modes { Full, Player_Only, Map, Solo }
    public enum Message_Speeds { Slow, Normal, Fast, Max }
    public enum Hp_Gauge_Modes { Basic, Advanced, Injured, Off }
    public enum Options
    {
        Animation_Mode, Game_Speed, Text_Speed, Combat_Window, Unit_Window, Enemy_Window, Terrain_Window, Objective_Window,
        Grid, Range_Preview, Hp_Gauges, Controller, Subtitle_Help, Autocursor, Auto_Turn_End, Window_Color
    }

    public class OptionsConfig
    {
        public readonly static OptionsData[] OPTIONS_DATA = new OptionsData[] {
            new OptionsData { Label = "Animation", Options = new OptionsSetting[] {
                new OptionsSetting( 3, "1", "Show animations"),
                new OptionsSetting(18, "2", "Show animations on the player turn"),
                new OptionsSetting(30, "Off", "Turn off combat animation"),
                new OptionsSetting(53, "Solo", "Set animation for each unit manually") }},
            new OptionsData { Label = "Game Speed", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Norm", "Set unit movement speed"),
                new OptionsSetting(31, "Fast", "Set unit movement speed (fast)") }},
            new OptionsData { Label = "Text Speed",Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Slow", "Set message speed (slow)"),
                new OptionsSetting(31, "Norm", "Set message speed"),
                new OptionsSetting(62, "Fast", "Set message speed (fast)"),
                new OptionsSetting(93, "Max", "Set message speed (autoscroll)") }},
            new OptionsData { Label = "Combat", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Basic", "Show basic Combat Info window"),
                new OptionsSetting(31, "Detail", "Show detailed Combat Info window"),
                new OptionsSetting(70, "OFF", "Turn Combat Info window off") }},
            new OptionsData { Label = "Unit", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Detail", "Show detailed unit window"),
                new OptionsSetting(31, "Panel", "Show normal unit window"),
                new OptionsSetting(62, "Burst", "Show unit window with tail"),
                new OptionsSetting(93, "OFF", "Turn unit window off") }},
            new OptionsData { Label = "Enemy Data", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Basic", "Show basic enemy window"),
                new OptionsSetting(31, "Detail", "Show detailed enemy window"),
                new OptionsSetting(70, "OFF", "Turn enemy window off") }},
            new OptionsData { Label = "Terrain", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Turn Terrain window on or off"),
                new OptionsSetting(23, "OFF", "Turn Terrain window on or off") }},
            new OptionsData { Label = "Show Objective", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Set Chapter Goal display"),
                new OptionsSetting(23, "OFF", "Set Chapter Goal display") }},
            new OptionsData { Label = "Grid",
                Gauge = true, GaugeMin = 0, GaugeMax = 16, GaugeInterval = 1,
                GaugeWidth = 72, GaugeOffset = 24,
                Options = new OptionsSetting[] {
                new OptionsSetting( 0, "{0}", "Set grid display") }},
            new OptionsData { Label = "Range Preview", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Set move range preview display"),
                new OptionsSetting(23, "OFF", "Set move range preview display") }},
            new OptionsData { Label = "HP Gauges", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "Basic", "Set HP Gauge display"),
                new OptionsSetting(31, "Advanced", "Set HP Gauge display"),
                new OptionsSetting(80, "Injured", "Set HP Gauge display (injured only)"),
                new OptionsSetting(120, "OFF", "Set HP Gauge display") }},
            new OptionsData { Label = "Controller", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Show controller help"),
                new OptionsSetting(23, "OFF", "Turn controller help off"),
                new OptionsSetting(46, "Vintage", "Remove blank tiles around map") }},
            new OptionsData { Label = "Subtitle Help", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Set Easy/Help Scroll display"),
                new OptionsSetting(23, "OFF", "Set Easy/Help Scroll display") }},
            new OptionsData { Label = "Autocursor", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Set cursor to start on main hero"),
                new OptionsSetting(23, "OFF", "Set cursor to start on main hero"), }},
                //new OptionsSetting(46, "Madelyn", "Set cursor to start on main hero")
            new OptionsData { Label = "Autoend Turns", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "ON", "Set turn to end automatically"),
                new OptionsSetting(23, "OFF", "Set turn to end automatically"),
                new OptionsSetting(46, "Prompt", "Opens menu after last unit has moved") }},
            new OptionsData { Label = "Window Color", Options = new OptionsSetting[] {
                new OptionsSetting( 0, "1", "Change window color"),
                new OptionsSetting(18, "2", "Change window color"),
                new OptionsSetting(33, "3", "Change window color"),
                new OptionsSetting(48, "4", "Change window color") }}
        };
    }
}
