using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EnumExtension;

namespace Tactile.Windows.UserInterface
{
    abstract class UINode : Stereoscopic_Graphic_Object, IUIObject
    {
        internal Vector2 Size;
        private bool LeftMouseDown, RightMouseDown, TouchPressing;
        private HashSet<Inputs> Triggers = new HashSet<Inputs>();
        private HashSet<MouseButtons> MouseTriggers = new HashSet<MouseButtons>();
        private HashSet<TouchGestures> TouchTriggers = new HashSet<TouchGestures>();
        internal float SliderValue { get; private set; }

        internal Vector2 CenterPoint { get { return HitBoxLoc(Vector2.Zero) + Size / 2; } }
        internal virtual bool Enabled { get { return true; } }
        protected virtual bool IsSlider { get { return false; } }

        protected abstract IEnumerable<Inputs> ValidInputs { get; }
        protected abstract bool RightClickActive { get; }

        public override string ToString()
        {
            return string.Format("UINode: {0}", this.loc);
        }

        public void UpdateInput(Vector2 drawOffset = default(Vector2))
        {
            Update(true, drawOffset);
        }

        internal void Update()
        {
            Update((UINodeSet<UINode>)null, ControlSet.Disabled);
        }
        internal void Update(bool input,
            Vector2 draw_offset = default(Vector2))
        {
            Update((UINodeSet<UINode>)null, input ? ControlSet.All : ControlSet.None, draw_offset);
        }
        internal void Update<T>(UINodeSet<T> nodes, bool input,
            Vector2 draw_offset = default(Vector2)) where T : UINode
        {
            Update(nodes, input ? ControlSet.All : ControlSet.None, draw_offset);
        }
        internal void Update<T>(UINodeSet<T> nodes, ControlSet input,
            Vector2 draw_offset = default(Vector2)) where T : UINode
        {
            // If wrong type
            if (!(this is T))
                return;
            // If not in node set
            if (nodes != null && !nodes.Contains(this as T))
                return;

            bool active_node = false;
            if (!input.HasFlag(ControlSet.Disabled))
            {
                mouse_off_graphic();
                if (Enabled)
                {
                    if (Input.IsControllingOnscreenMouse)
                    {
                        UpdateMouse<T>(nodes, input, draw_offset);
                    }
                    else if (Input.ControlScheme == ControlSchemes.Touch)
                    {
                        UpdateTouch<T>(nodes, input, draw_offset);
                    }
                    else if (is_active_node<T>(nodes))
                    {
                        UpdateButtons<T>(input);
                    }
                }

                active_node = is_active_node<T>(nodes);
            }
            update_graphics(active_node);
        }

        private bool is_active_node<T>(UINodeSet<T> nodes) where T : UINode
        {
            return nodes == null || nodes.ActiveNode == this;
        }

        private void UpdateMouse<T>(
            UINodeSet<T> nodes,
            ControlSet input,
            Vector2 draw_offset) where T : UINode
        {
            if (input.HasEnumFlag(ControlSet.MouseButtons))
            {
                bool input_used = update_mouse_input(
                    ref LeftMouseDown, MouseButtons.Left, draw_offset);
                if (RightClickActive && !input_used)
                    update_mouse_input(
                        ref RightMouseDown, MouseButtons.Right, draw_offset);
            }

            if (input.HasEnumFlag(ControlSet.MouseMove))
            {
                if (OnScreenBounds(draw_offset).Contains(
                    (int)Global.Input.mousePosition.X,
                    (int)Global.Input.mousePosition.Y))
                {
                    if (nodes != null)
                        nodes.MouseMove(this as T);

                    if (LeftMouseDown || RightMouseDown)
                        mouse_click_graphic();
                    else
                        mouse_highlight_graphic();
                }
            }

            if (input.HasEnumFlag(ControlSet.Mouse))
                if (IsSlider && LeftMouseDown &&
                    OnScreenBounds(draw_offset).Contains(
                        (int)Global.Input.mousePosition.X,
                        (int)Global.Input.mousePosition.Y))
                {
                    TouchTriggers.Add(TouchGestures.Scrubbing);
                    SliderValue = slide(Global.Input.mousePosition, draw_offset);
                }
        }

        private void UpdateTouch<T>(
            UINodeSet<T> nodes,
            ControlSet input,
            Vector2 draw_offset) where T : UINode
        {
            // Tap
            if (input.HasEnumFlag(ControlSet.TouchButtons) &&
                Global.Input.gesture_rectangle(TouchGestures.Tap, OnScreenBounds(draw_offset)))
            {
                if (nodes != null)
                    nodes.TouchMove(this as T, TouchGestures.Tap);
                TouchTriggers.Add(TouchGestures.Tap);
            }
            // Long Press
            else if (input.HasEnumFlag(ControlSet.TouchButtons) &&
                Global.Input.gesture_rectangle(TouchGestures.LongPress, OnScreenBounds(draw_offset)))
            {
                if (nodes != null)
                    nodes.TouchMove(this as T, TouchGestures.LongPress);
                TouchTriggers.Add(TouchGestures.LongPress);
            }
            // Movement from pressing
            else if (input.HasEnumFlag(ControlSet.TouchMove) &&
                Global.Input.touch_rectangle(
                    Services.Input.InputStates.Pressed,
                    OnScreenBounds(draw_offset),
                    false))
            {
                if (nodes != null)
                    nodes.TouchMove(this as T, TouchGestures.ShortPress);
                mouse_click_graphic();
            }

            // Slider
            if (IsSlider && input.HasEnumFlag(ControlSet.TouchButtons))
            {
                if (!Global.Input.touch_pressed(false))
                    TouchPressing = false;
                else
                {
                    bool pressed = OnScreenBounds(draw_offset).Contains(
                        (int)Global.Input.touchPressPosition.X,
                        (int)Global.Input.touchPressPosition.Y);
                    if (pressed && Global.Input.touch_triggered(false))
                        TouchPressing = true;

                    if (pressed && TouchPressing)
                    {
                        TouchTriggers.Add(TouchGestures.Scrubbing);
                        SliderValue = slide(Global.Input.touchPressPosition, draw_offset);
                    }
                }
            }
        }

        private void UpdateButtons<T>(ControlSet input) where T : UINode
        {
            foreach (Inputs key in this.ValidInputs)
                switch (key)
                {
                    // Movement
                    case Inputs.Down:
                    case Inputs.Left:
                    case Inputs.Right:
                    case Inputs.Up:
                        if (input.HasEnumFlag(ControlSet.PadMove))
                            if (Global.Input.triggered(key))
                            {
                                Triggers.Add(key);
                            }
                        break;
                    // Buttons
                    default:
                        if (input.HasEnumFlag(ControlSet.PadButtons))
                            if (Global.Input.triggered(key))
                            {
                                Triggers.Add(key);
                            }
                        break;
                }
        }

        private bool update_mouse_input(ref bool mouseDown, MouseButtons button,
            Vector2 draw_offset)
        {
            bool input_used = false;
            if (Global.Input.mouse_down_rectangle(
                button, OnScreenBounds(draw_offset), false))
            {
                input_used = true;
                mouseDown = true;
            }
            bool released = !Global.Input.mouse_pressed(button, false);

            if (mouseDown && released)
            {
                mouseDown = false;
                input_used = false;

                if (OnScreenBounds(draw_offset).Contains(
                    (int)Global.Input.mousePosition.X,
                    (int)Global.Input.mousePosition.Y))
                {
                    // Consume the input of this button
                    if (!Global.Input.consume_input(button))
                    {
                        MouseTriggers.Add(button);
                        input_used = true;
                    }
                }
            }

            return input_used;
        }

        protected abstract void update_graphics(bool activeNode);

        public bool consume_trigger(MouseButtons button)
        {
            bool result = MouseTriggers.Contains(button);
            MouseTriggers.Remove(button);
            return result;
        }
        public bool consume_trigger(Inputs input)
        {
#if DEBUG
            if (!ValidInputs.Contains(input))
                throw new ArgumentException(string.Format(
                    "Tried to test a UINode for input with \"Inputs.{0}\", a key it isn't set to process",
                    input.ToString()));
#endif
            bool result = Triggers.Contains(input);
            Triggers.Remove(input);
            return result;
        }
        public bool consume_trigger(TouchGestures gesture)
        {
            bool result = TouchTriggers.Contains(gesture);
            TouchTriggers.Remove(gesture);
            return result;
        }

        internal virtual void Activate() { }
        internal virtual void Deactivate() { }

        internal void clear_triggers()
        {
            Triggers.Clear();
            MouseTriggers.Clear();
        }

        public Rectangle OnScreenBounds(Vector2 drawOffset)
        {
            Vector2 loc = HitBoxLoc(drawOffset);
            return new Rectangle((int)loc.X, (int)loc.Y,
                (int)Size.X, (int)Size.Y);
        }

        public bool MouseOver(Vector2 drawOffset = default(Vector2))
        {
            Rectangle objectRect = OnScreenBounds(drawOffset);
            return Global.Input.mouse_in_rectangle(
                objectRect, loc - drawOffset, this.offset, 0f, false);
        }
        protected virtual Vector2 HitBoxLoc(Vector2 drawOffset)
        {
            Vector2 loc = this.loc + this.draw_offset;
            loc -= drawOffset;
            return loc;
        }

        protected virtual float slide(Vector2 inputPosition, Vector2 drawOffset)
        {
            Vector2 slide_position = (inputPosition - HitBoxLoc(drawOffset));
            Vector2 slider_area = Size;
            return slide_position.X / slider_area.X;
        }

        protected abstract void mouse_off_graphic();
        protected abstract void mouse_highlight_graphic();
        protected abstract void mouse_click_graphic();

        public abstract void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2));

#if DEBUG
        public void DrawHitbox(
            SpriteBatch spriteBatch,
            Vector2 drawOffset = default(Vector2))
        {
            Color black = new Color(0, 0, 0, 0.5f);
            Color white = new Color(0.5f, 0.5f, 0.5f, 0.25f);

            Rectangle hitbox = OnScreenBounds(drawOffset);

            spriteBatch.Draw(
                Global.Content.Load<Texture2D>(@"Graphics/White_Square"),
                new Vector2(hitbox.X, hitbox.Y), null, black, 0, Vector2.Zero,
                new Vector2(hitbox.Width / 16f, hitbox.Height / 16f),
                SpriteEffects.None, 0f);
            spriteBatch.Draw(
                Global.Content.Load<Texture2D>(@"Graphics/White_Square"),
                new Vector2(hitbox.X + 1, hitbox.Y + 1),
                null, white, 0, Vector2.Zero,
                new Vector2((hitbox.Width - 2) / 16f, (hitbox.Height - 2) / 16f),
                SpriteEffects.None, 0f);
        }
#endif
    }
}
