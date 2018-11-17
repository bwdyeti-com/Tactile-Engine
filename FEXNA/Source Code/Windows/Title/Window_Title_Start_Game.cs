using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FEXNA.Graphics.Help;
using FEXNA.Menus;
using FEXNA.Windows.UserInterface;
using FEXNA.Windows.UserInterface.Title;
using FEXNA_Library;

namespace FEXNA
{
    class Window_Title_Start_Game : BaseMenu
    {
        internal const int PANEL_WIDTH = 240;

        private int Page, Max_Page;
        private int Move_File_Id, Move_Index, Move_Page;
        private bool Moving = false, Copying = false, WaitingForIO;
        private bool MenusHidden = false;
        public Vector2 MenuLoc { get; private set; }
        private List<Inputs> locked_inputs = new List<Inputs>();
        private Suspend_Info_Panel Suspend_Panel;
        private StartGame_Info_Panel[] Panels = new StartGame_Info_Panel[Config.SAVES_PER_PAGE];
        private UINodeSet<StartGame_Info_Panel> PanelNodes;
        private Hand_Cursor Cursor, Move_Cursor;
        private Page_Arrow Left_Page_Arrow, Right_Page_Arrow;

        private Button_Description CancelButton;

        #region Accessors
        public int file_id
        {
            get { return PanelNodes.ActiveNodeIndex + Page * Config.SAVES_PER_PAGE + 1; }
            private set
            {
                int index;
                if (value <= 0)
                {
                    index = 0;
                    Page = 0;
                }
                else
                {
                    index = (value - 1) % Config.SAVES_PER_PAGE;
                    Page = (value - 1) / Config.SAVES_PER_PAGE;
                }
                PanelNodes.set_active_node(PanelNodes[index]);

                for (int i = 0; i < Panels.Length; i++)
                    Panels[i].active = false;
                Panels[index].active = true;

                refresh_page();
                Cursor.force_loc(PanelNodes[index].loc);
            }
        }

        public int move_file_id { get { return Move_File_Id; } }
        public bool moving_file
        {
            get { return Moving; }
            set
            {
                Moving = value;
                Move_File_Id = file_id;
                Move_Index = PanelNodes.ActiveNodeIndex;
                Move_Page = Page;
                update_move_darken();
            }
        }
        public bool copying
        {
            get { return Copying; }
            set
            {
                Copying = value;
                moving_file = value;
            }
        }
        public bool waiting_for_io
        {
            get { return WaitingForIO; }
            set
            {
                WaitingForIO = value;
            }
        }

        public override bool HidesParent { get { return false; } }
        #endregion

        public Window_Title_Start_Game(int file_id)
        {
            MenuLoc = new Vector2(
                (Config.WINDOW_WIDTH - (PANEL_WIDTH - 8)) / 2,
                20);

            if (file_id == -1)
            {
                Max_Page = 0;
                file_id = 0;
            }
            else
            {
                Max_Page = Math.Min(Config.SAVE_PAGES,
                    ((Global.save_files_info.Keys.Max() - 1) / Config.SAVES_PER_PAGE) + 1);
            }

            initialize(file_id);
        }

        private void initialize(int fileId)
        {
            Cursor = new Hand_Cursor();
            Cursor.draw_offset = new Vector2(-8, 4);
            Cursor.stereoscopic = Config.TITLE_MENU_DEPTH;
            Move_Cursor = new Hand_Cursor();
            Move_Cursor.draw_offset = new Vector2(-8, 4);
            Move_Cursor.tint = new Color(160, 160, 160, 255);
            Move_Cursor.stereoscopic = Config.TITLE_MENU_DEPTH;

            for (int i = 0; i < Panels.Length; i++)
            {
                Panels[i] = new StartGame_Info_Panel(Page * Config.SAVES_PER_PAGE + i + 1, PANEL_WIDTH);
                Panels[i].stereoscopic = Config.TITLE_MENU_DEPTH;
            }

            refresh_panel_locations();
            PanelNodes = new UINodeSet<StartGame_Info_Panel>(Panels);
            PanelNodes.CursorMoveSound = System_Sounds.Menu_Move1;
            PanelNodes.WrapVerticalMove = true;

            // Page Arrows
            Left_Page_Arrow = new Page_Arrow();
            Left_Page_Arrow.loc = new Vector2(-4, 68);
            Left_Page_Arrow.stereoscopic = Config.TITLE_MENU_DEPTH - 1;
            Left_Page_Arrow.ArrowClicked += Left_Page_Arrow_ArrowClicked;
            Right_Page_Arrow = new Page_Arrow();
            Right_Page_Arrow.loc = new Vector2(PANEL_WIDTH - 4, 68);
            Right_Page_Arrow.mirrored = true;
            Right_Page_Arrow.stereoscopic = Config.TITLE_MENU_DEPTH - 1;
            Right_Page_Arrow.ArrowClicked += Right_Page_Arrow_ArrowClicked;

            create_cancel_button();

            this.file_id = fileId;
        }

        private void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        private void refresh_panel_locations()
        {
            int offset = 0;
            for (int i = 0; i < Panels.Length; i++)
            {
                Panels[i].loc = new Vector2(0, offset);
                offset += Panels[i].height - 8;
            }
        }

        public void refresh_page()
        {
            for (int i = 0; i < Panels.Length; i++)
                Panels[i].set_data(Page * Config.SAVES_PER_PAGE + i + 1);
            Left_Page_Arrow.visible = Page > 0;
            Right_Page_Arrow.visible = Page < Max_Page;

            refresh_panel_locations();
        }

        public Vector2 SelectedOptionLoc
        {
            get
            {
                return MenuLoc + PanelNodes.ActiveNode.loc +
                    new Vector2(PANEL_WIDTH + 28, -16);
            }
        }

        #region Events
        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> MoveFile;
        protected void OnMoveFile(EventArgs e)
        {
            if (MoveFile != null)
                MoveFile(this, e);
        }

        public event EventHandler<EventArgs> CopyFile;
        protected void OnCopyFile(EventArgs e)
        {
            if (CopyFile != null)
                CopyFile(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }
        #endregion

        protected override void Activate()
        {
            for (int i = 0; i < Panels.Length; i++)
                Panels[i].tint = new Color(255, 255, 255, 255);
            Cursor.visible = true;
            MenusHidden = false;
        }
        protected override void Deactivate()
        {
            for (int i = 0; i < Panels.Length; i++)
            {
                int alpha = i == PanelNodes.ActiveNodeIndex ? 255 : 160;
                Panels[i].tint = new Color(alpha, alpha, alpha, 255);
            }
            Cursor.visible = false;
        }

        public void HideMenus()
        {
            MenusHidden = true;
        }

        protected override void UpdateMenu(bool active)
        {
            // Block inputs while waiting for IO
            if (WaitingForIO)
            {
                if (!Global.copying && !Global.move_file && !Global.delete_file)
                {
                    this.waiting_for_io = false;
                    refresh_page();
                }
                else
                    active = false;
            }

            Cursor.update();
            Left_Page_Arrow.update();
            Right_Page_Arrow.update();

            if (active)
                update_input();
            update_ui(active);

            if (Moving)
                update_move_darken();
        }

        private void update_move_darken()
        {
            for (int i = 0; i < Panels.Length; i++)
            {
                if (!Moving ||
                        ((Page == Move_Page && i == Move_Index) ||
                        (i == PanelNodes.ActiveNodeIndex && !Global.save_files_info.ContainsKey(i + Page * Config.SAVES_PER_PAGE + 1))))
                    Panels[i].tint = new Color(255, 255, 255, 255);
                else
                    Panels[i].tint = new Color(160, 160, 160, 255);
            }
        }

        private void update_input()
        {
            Left_Page_Arrow.UpdateInput(-MenuLoc);
            Right_Page_Arrow.UpdateInput(-MenuLoc);

            if (!Global.Input.pressed(Inputs.Up) && !Global.Input.pressed(Inputs.Down))
            {
                if (Page > 0 && (Global.Input.repeated(Inputs.Left) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeRight)))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    Page--;
                    refresh_page();
                }
                if (Page < Max_Page && (Global.Input.repeated(Inputs.Right) ||
                    Global.Input.gesture_triggered(TouchGestures.SwipeLeft)))
                {
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
                    Page++;
                    refresh_page();
                }
            }
            if (!Global.Input.pressed(Inputs.Up))
                locked_inputs.Remove(Inputs.Up);
            if (!Global.Input.pressed(Inputs.Down))
                locked_inputs.Remove(Inputs.Down);
        }

        protected virtual void update_ui(bool input)
        {
            int index = PanelNodes.ActiveNodeIndex;
            PanelNodes.Update(input, -MenuLoc);
            if (index != PanelNodes.ActiveNodeIndex)
            {
                Panels[index].active = false;
                PanelNodes.ActiveNode.active = true;
                refresh_panel_locations();
            }

            CancelButton.Update(input);

            if (input)
            {
                if (Cursor.target_loc != PanelNodes.ActiveNode.loc)
                    Cursor.set_loc(PanelNodes.ActiveNode.loc);

                var file_index = PanelNodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (file_index.IsSomething)
                {
                    int file_id = file_index + 1 + Page * Config.SAVES_PER_PAGE;
                    if (this.file_id != file_id)
                        this.file_id = file_id;

                    if (Copying)
                        OnCopyFile(new EventArgs());
                    else if (Moving)
                        OnMoveFile(new EventArgs());
                    else
                        OnSelected(new EventArgs());
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    Global.Input.KeyPressed(Keys.Escape) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    if (Copying)
                        this.copying = false;
                    else if (Moving)
                        this.moving_file = false;
                    else
                        OnCanceled(new EventArgs());
                }
            }
        }

        private void Left_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Page > 0)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move2);
                Page--;
                refresh_page();
            }
        }

        private void Right_Page_Arrow_ArrowClicked(object sender, EventArgs e)
        {
            if (Page < Max_Page)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move2);
                Page++;
                refresh_page();
            }
        }

        internal void preview_suspend()
        {
            int save_id = Page * Config.SAVES_PER_PAGE + PanelNodes.ActiveNodeIndex + 1;
            if (Global.suspend_files_info != null &&
                Global.suspend_files_info.ContainsKey(save_id))
            {
                open_preview(Global.suspend_files_info[save_id]);
            }
        }

        internal void preview_checkpoint()
        {
            int save_id = Page * Config.SAVES_PER_PAGE + PanelNodes.ActiveNodeIndex + 1;
            if (Global.checkpoint_files_info != null &&
                Global.checkpoint_files_info.ContainsKey(save_id))
            {
                open_preview(Global.checkpoint_files_info[save_id]);
            }
        }

        private void open_preview(IO.Suspend_Info info)
        {
            Suspend_Panel = new Suspend_Info_Panel(false, info);
            Suspend_Panel.loc = MenuLoc +
                new Vector2(0, -8 + PanelNodes.ActiveNodeIndex * 24);
            Suspend_Panel.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        internal void close_preview()
        {
            Suspend_Panel = null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!MenusHidden)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                PanelNodes.Draw(spriteBatch, -MenuLoc);
                if (Moving && Move_Page == Page)
                    Move_Cursor.draw(spriteBatch, -(MenuLoc + Panels[Move_Index].loc));

                Left_Page_Arrow.draw(spriteBatch, -MenuLoc);
                Right_Page_Arrow.draw(spriteBatch, -MenuLoc);
                Cursor.draw(spriteBatch, -MenuLoc);
                spriteBatch.End();

                if (Suspend_Panel != null)
                    Suspend_Panel.Draw(spriteBatch);

                if (Cursor.visible)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    CancelButton.Draw(spriteBatch);
                    spriteBatch.End();
                }
            }
        }
    }
}
