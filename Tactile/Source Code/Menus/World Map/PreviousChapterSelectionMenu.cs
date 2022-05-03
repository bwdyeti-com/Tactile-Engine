using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;
using Tactile.Graphics.Windows;
using Tactile.Windows;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Command;
using TactileLibrary;

namespace Tactile.Menus.Worldmap
{
    class PreviousChapterSelectionMenu : BaseMenu, IHasCancelButton
    {
        private string ChapterId;
        private List<string> ProgressionIds;
        private Dictionary<string, List<string>> ValidPreviousChapters;
        private Dictionary<string, int> PreviousChapterIndices;
        private Vector2 Loc;

        private SystemWindowHeadered Window;
        private TextSprite Header;
        private StatusWindowDivider Divider;
        private UINodeSet<CommandUINode> Items;
        private UICursor<CommandUINode> UICursor;
        private Dictionary<Page_Arrow, int> LeftArrows, RightArrows;
        private Button_Description CancelButton;

        internal Dictionary<string, int> previous_chapter_indices { get { return new Dictionary<string, int>(PreviousChapterIndices); } }

        public override bool HidesParent { get { return false; } }

        internal PreviousChapterSelectionMenu(
            Vector2 centerLoc,
            string chapterId,
            WorldmapMenuData menuData,
            IHasCancelButton menu = null)
        {
            ChapterId = chapterId;
            ProgressionIds = menuData.ValidPreviousChapters.Keys.ToList();
            ValidPreviousChapters = menuData.ValidPreviousChapters;
            PreviousChapterIndices = menuData.UsablePreviousChapterIndices;

            Window = new SystemWindowHeadered();
            Window.width = 104;
            Window.height = 32 + 16 * (ValidPreviousChapters.Count + 1) + 4;
            Window.offset = new Vector2(0, 16);

            Loc = centerLoc -
                (new Vector2(Window.width, Window.height) - Window.offset) / 2;

            Header = new TextSprite();
            Header.draw_offset = new Vector2(8, -8);
            Header.SetFont(Config.UI_FONT, Global.Content, "Yellow");
            Header.text = ValidPreviousChapters.Count > 1 ? "Previous Chapters" : "Previous Chapter";

            Divider = new StatusWindowDivider();
            Divider.draw_offset = new Vector2(8, Window.height - 44);
            Divider.SetWidth(Window.width - 16);

            LeftArrows = new Dictionary<Page_Arrow,int>();
            RightArrows = new Dictionary<Page_Arrow,int>();

            // Center, then adjust left to account for map sprite
            int x = ((Window.width / 2) / 8 * 8) - 16;
            List<CommandUINode> nodes = new List<CommandUINode>();
            for (int i = 0; i < ProgressionIds.Count; i++)
            {
                int y = i * 16 + 8;

                var text = new TextSprite();
                text.SetFont(Config.UI_FONT, Global.Content, "White");
                text.text = GetListName(i);
                var node = new MapSpriteUINode("", text, 56);
                refresh_map_sprite(node, i);
                node.loc = new Vector2(x, y);
                nodes.Add(node);

                // Add arrows for this set of chapters,
                // if there's more than one choice
                if (chapter_list(i).Count > 1)
                {
                    var left_arrow = new Page_Arrow();
                    left_arrow.loc = new Vector2(8, y);
                    left_arrow.ArrowClicked += LeftArrow_ArrowClicked;
                    LeftArrows.Add(left_arrow, i);

                    var right_arrow = new Page_Arrow();
                    right_arrow.loc = new Vector2(Window.width - 8, y);
                    right_arrow.mirrored = true;
                    right_arrow.ArrowClicked += RightArrow_ArrowClicked;
                    RightArrows.Add(right_arrow, i);
                }
            }
            
            // Add confirm choice
            var confirmText = new TextSprite(
                Config.UI_FONT, Global.Content, "White",
                new Vector2(4, 0),
                "Confirm");
            var confirm = new TextUINode("", confirmText, 56);
            confirm.loc = new Vector2(x, nodes.Count * 16 + 8 + 4);
            nodes.Add(confirm);

            Items = new UINodeSet<CommandUINode>(nodes);
            Items.WrapVerticalSameColumn = true;
            Items.CursorMoveSound = System_Sounds.Menu_Move1;
            Items.HorizontalCursorMoveSound = System_Sounds.Menu_Move2;

            Items.AngleMultiplier = 2f;
            Items.TangentDirections = new List<CardinalDirections> { CardinalDirections.Left, CardinalDirections.Right };
            Items.refresh_destinations();

            Items.set_active_node(confirm);

            UICursor = new UICursor<CommandUINode>(Items);
            UICursor.draw_offset = new Vector2(-12, 0);
            //UICursor.ratio = new int[] { 1, 3 }; //Debug

            CreateCancelButton(menu);
        }

        internal void activate(Vector2 cursorLoc)
        {
            UICursor.force_loc((cursorLoc - UICursor.draw_offset) - Loc);
            UICursor.set_loc(Items.ActiveNode.loc);
        }

        private List<string> chapter_list(int index)
        {
            return ValidPreviousChapters[ProgressionIds[index]];
        }
        private int chapter_index(int index)
        {
            return PreviousChapterIndices[ProgressionIds[index]];
        }
        private TactileLibrary.Data_Chapter chapter(int index)
        {
            int chapter_index = this.chapter_index(index);
            List<string> list = chapter_list(index);
            return Global.data_chapters[list[chapter_index]];
        }
        
        #region IHasCancelButton
        public bool HasCancelButton { get { return CancelButton != null; } }
        public Vector2 CancelButtonLoc { get { return CancelButton.loc; } }
        #endregion

        #region Cancel
        private void CreateCancelButton(IHasCancelButton menu)
        {
            if (menu != null && menu.HasCancelButton)
            {
                CreateCancelButton(
                    (int)menu.CancelButtonLoc.X,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
            else
            {
                CreateCancelButton(
                    16,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
        }
        private void CreateCancelButton(int x, float depth = 0)
        {
            CancelButton = Button_Description.button(Inputs.B, x);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = depth;
        }

        protected virtual bool CanceledTriggered(bool active)
        {
            bool cancel = active && Global.Input.triggered(Inputs.B);
            if (CancelButton != null)
            {
                cancel |= CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap);
            }
            return cancel;
        }
        #endregion

        protected override void UpdateMenu(bool active)
        {
            update_ui(active);

            Window.update();
            Header.update();
            foreach (var arrow in LeftArrows.Keys)
                arrow.update();
            foreach (var arrow in RightArrows.Keys)
                arrow.update();
        }

        private void update_ui(bool input)
        {
            if (input)
            {
                foreach (var arrow in LeftArrows.Keys)
                    arrow.UpdateInput(-Loc);
                foreach (var arrow in RightArrows.Keys)
                    arrow.UpdateInput(-Loc);
            }

            int index = Items.ActiveNodeIndex;
            Items.Update(input, -(Loc));
            bool moved = index != Items.ActiveNodeIndex;

            UICursor.update();

            if (CancelButton != null)
                CancelButton.Update(input);
            bool cancel = CanceledTriggered(input);

            if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                OnCanceled(new EventArgs());
            }
            else if (input)
            {
                if (Global.Input.triggered(Inputs.Left))
                    change_index(-1);
                else if (Global.Input.triggered(Inputs.Right))
                    change_index(1);


                var selected = Items.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                // Select event if on Confirm
                if (selected.IsSomething && selected == ValidPreviousChapters.Count)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    OnSelected(new EventArgs());
                }
            }
        }

        protected override void UpdateAncillary()
        {
            if (CancelButton != null)
            {
                if (Input.ControlSchemeSwitched)
                    CreateCancelButton(this);
            }
        }

        private void LeftArrow_ArrowClicked(object sender, System.EventArgs e)
        {
            var arrow = sender as Page_Arrow;
            Items.set_active_node(Items[LeftArrows[arrow]]);
            change_index(-1);
        }
        private void RightArrow_ArrowClicked(object sender, System.EventArgs e)
        {
            var arrow = sender as Page_Arrow;
            Items.set_active_node(Items[RightArrows[arrow]]);
            change_index(1);
        }

        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> PreviousChapterChanged;
        protected void OnPreviousChapterChanged(EventArgs e)
        {
            if (PreviousChapterChanged != null)
                PreviousChapterChanged(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }

        private void change_index(int move)
        {
            int index = Items.ActiveNodeIndex;
            // Return if confirm choice active
            if (index >= ValidPreviousChapters.Count)
                return;

            int count = chapter_list(index).Count;
            if (count > 1)
            {
                Global.game_system.play_se(System_Sounds.Menu_Move2);
                PreviousChapterIndices[ProgressionIds[index]] =
                    (PreviousChapterIndices[ProgressionIds[index]] + count + move) % count;

                (Items.ActiveNode as TextUINode).set_text(GetListName(index));
                refresh_map_sprite(Items.ActiveNode as MapSpriteUINode, index);

                var arrows = move < 0 ? LeftArrows : RightArrows;
                var arrow = arrows.FirstOrDefault(x => x.Value == index).Key;
                if (arrow != null)
                    arrow.twirl();

                OnPreviousChapterChanged(new EventArgs());

            }
        }

        private string GetListName(int index)
        {
            return chapter(index).ListName;
        }

        private int refresh_map_sprite(MapSpriteUINode node, int index)
        {
            int team = Constants.Team.PLAYER_TEAM;
            if (Global.data_chapters[ChapterId].Battalion != chapter(index).Battalion)
                team = Constants.Team.CITIZEN_TEAM;
            string lord_map_sprite = Global.save_file.lord_map_sprite(
                ProgressionIds[index], chapter(index).Id);
            node.set_map_sprite(lord_map_sprite, team);
            return team;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            CancelButton.Draw(spriteBatch);

            Window.draw(spriteBatch, -Loc);
            Divider.draw(spriteBatch, -Loc);
            Header.draw(spriteBatch, -Loc);
            foreach (var arrow in LeftArrows.Keys)
                arrow.draw(spriteBatch, -Loc);
            foreach (var arrow in RightArrows.Keys)
                arrow.draw(spriteBatch, -Loc);
            Items.Draw(spriteBatch, -Loc);
            UICursor.draw(spriteBatch, -Loc);
            spriteBatch.End();
        }
    }
}
