using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface;

namespace Tactile.Windows
{
    enum ScrollDirections : byte
    {
        Vertical =      1 << 0,
        Horizontal =    1 << 1,
        Free =          Vertical | Horizontal
    }

    class ScrollComponent : Graphic_Object
    {
        private Vector2 ViewAreaSize;
        protected Vector2 ElementSize { get; private set; }
        protected ScrollDirections Direction { get; private set; }
        protected Vector2 ElementLengths { get; private set; }
        protected Vector2 TopIndex { get; private set; }
        public int Index { get; private set; }
        protected Vector2 ScrollSpeed = Vector2.Zero;
        protected bool ScrollWheel = false;

        private float MaxScrollSpeed = 4;
        private float ScrollFriction = 0.95f;

        public IUIObject UpMouseOver, DownMouseOver;
        public IUIObject LeftMouseOver, RightMouseOver;
        public Scroll_Bar Scrollbar;

        public Vector2 IntOffset
        {
            get
            {
                return new Vector2(
                    (int)Math.Round(this.offset.X),
                    (int)Math.Round(this.offset.Y));
            }
        }

        private Vector2 ScrollAreaSize { get { return ElementSize * ElementLengths; } }
        protected Vector2 MinOffset { get { return new Vector2(0, 0); } }
        protected Vector2 MaxOffset
        {
            get
            {
                Vector2 result = Vector2.Max(new Vector2(0, 0), this.ScrollAreaSize - ViewAreaSize);
                // If necessary restrict to one axis
                if (!Direction.HasFlag(ScrollDirections.Horizontal))
                    result.X = 0;
                if (!Direction.HasFlag(ScrollDirections.Vertical))
                    result.Y = 0;
                return result;
            }
        }

        private Rectangle ViewAreaRectangle
        {
            get
            {
                return new Rectangle(
                    (int)this.loc.X, (int)this.loc.Y,
                    (int)ViewAreaSize.X, (int)ViewAreaSize.Y);
            }
        }

        protected Vector2 ViewableElements { get { return ViewAreaSize / ElementSize; } }

        protected Vector2 ScrollIndex
        {
            get
            {
                switch (Direction)
                {
                    case ScrollDirections.Horizontal:
                        int rows = Math.Max(1, (int)ElementLengths.Y);
                        return new Vector2(Index / rows, Index % rows);
                    default:
                        int columns = Math.Max(1, (int)ElementLengths.X);
                        return new Vector2(Index % columns, Index / columns);
                }
            }
        }

        public bool AtLeft { get { return this.offset.X <= this.MinOffset.X; } }
        public bool AtRight { get { return this.offset.X >= this.MaxOffset.X; } }
        public bool AtTop { get { return this.offset.Y <= this.MinOffset.Y; } }
        public bool AtBottom { get { return this.offset.Y >= this.MaxOffset.Y; } }

        public virtual bool IsScrolling
        {
            get
            {
                Vector2 scrollElement = this.offset / ElementSize;
                return ScrollSpeed.X != 0 || ScrollSpeed.Y != 0 ||
                    scrollElement.X % 1 != 0 || scrollElement.Y % 1 != 0;
            }
        }

        /// <summary>
        /// Returns the distance in elements the index is outside of the visible area.
        /// </summary>
        protected Vector2 OutsideScrollAreaOffset
        {
            get
            {
                Vector2 index = this.ScrollIndex;

                float ox = 0;
                if (TopIndex.X > index.X)
                    ox = TopIndex.X - index.X;
                else if (TopIndex.X + ViewableElements.X < index.X + 1)
                    ox = (TopIndex.X + ViewableElements.X) - (index.X + 1);

                float oy = 0;
                if (TopIndex.Y > index.Y)
                    oy = TopIndex.Y - index.Y;
                else if (TopIndex.Y + ViewableElements.Y < index.Y + 1)
                    oy = (TopIndex.Y + ViewableElements.Y) - (index.Y + 1);

                return new Vector2(ox, oy);
            }
        }

        public ScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollDirections direction)
        {
            ElementLengths = Vector2.One;

            ViewAreaSize = Vector2.Max(viewAreaSize, Vector2.One);
            ElementSize = Vector2.Max(elementSize, Vector2.One);
            Direction = direction;
        }

        /// <summary>
        /// Sets the number of columns and rows of the scrollable area elements.
        /// </summary>
        public void SetElementLengths(Vector2 elementLengths)
        {
            ElementLengths = elementLengths;
        }
        /// <summary>
        /// Set the parameters managing scroll speed.
        /// </summary>
        /// <param name="scrollFriction">Multiplied by scroll speed as frition. Maximum of 0.999f.</param>
        public void SetScrollSpeeds(float maxScrollSpeed, float scrollFriction)
        {
            MaxScrollSpeed = maxScrollSpeed;
            ScrollFriction = Math.Min(0.999f, scrollFriction);
        }


        protected virtual void SetOffset(Vector2 offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Reset scroll speed to zero.
        /// </summary>
        public void Reset()
        {
            ScrollSpeed = Vector2.Zero;
        }

        /// <summary>
        /// Jump the bottom offset and set vertical scroll to zero.
        /// </summary>
        public void ScrollToBottom()
        {
            this.offset.Y = Math.Max(this.MinOffset.Y, this.MaxOffset.Y);
            ScrollSpeed.Y = 0;
        }

        /// <summary>
        /// Clamps the index within the current view area.
        /// </summary>
        protected void FixIndex()
        {
            Vector2 offset = this.OutsideScrollAreaOffset;
            if (offset != Vector2.Zero)
            {
                int ox = (int)Math.Round(offset.X);
                int oy = (int)Math.Round(offset.Y);

                int indexOffset;
                switch (Direction)
                {
                    case ScrollDirections.Horizontal:
                        int rows = Math.Max(1, (int)ElementLengths.Y);
                        indexOffset = rows * ox + oy;
                        break;
                    default:
                        int columns = Math.Max(1, (int)ElementLengths.X);
                        indexOffset = columns * oy + ox;
                        break;
                }

                Index += indexOffset;
            }
        }

        // Jumps scroll position to bring index inside the view area.
        public void FixScroll(int index)
        {
            Index = index;

            Vector2 offset = this.OutsideScrollAreaOffset;
            if (offset != Vector2.Zero)
            {
                Vector2 scrollIndex = this.offset / ElementSize - offset;
                scrollIndex.X = scrollIndex.X >= 0 ? (float)Math.Ceiling(scrollIndex.X) : (float)Math.Floor(scrollIndex.X);
                scrollIndex.Y = scrollIndex.Y >= 0 ? (float)Math.Ceiling(scrollIndex.Y) : (float)Math.Floor(scrollIndex.Y);

                SetOffset(Vector2.Clamp(
                    scrollIndex * ElementSize,
                    this.MinOffset, this.MaxOffset));
                
                UpdateTopIndex();
                Reset();
            }
        }

        public virtual void Update(bool active, int index = -1)
        {
            if (index != -1)
                Index = index;

            // Adjust max speed by input method
            float maxSpeed;
            // On buttons, double speed if speed up button is held
            if (Input.ControlScheme == ControlSchemes.Buttons)
                maxSpeed = (Global.Input.speed_up_input() ? 2 : 1) *
                    MaxScrollSpeed;
            else
            {
                // On mouse, max speed is five times button base speed
                maxSpeed = 5f * MaxScrollSpeed;
                // On touch allow scrolling the entire screen each tick
                if (Input.ControlScheme == ControlSchemes.Touch)
                    maxSpeed = Math.Max(maxSpeed, Config.WINDOW_HEIGHT);
            }

            UpdateInput(active, maxSpeed);

            // Clamp to max speed
            ScrollSpeed.X = MathHelper.Clamp(ScrollSpeed.X, -maxSpeed, maxSpeed);
            ScrollSpeed.Y = MathHelper.Clamp(ScrollSpeed.Y, -maxSpeed, maxSpeed);

            // Apply scroll speed to offset and clamp to offset
            this.offset = Vector2.Clamp(this.offset + ScrollSpeed,
                this.MinOffset, this.MaxOffset);
            if (ScrollSpeed.LengthSquared() == 0)
                this.offset = this.IntOffset;

            UpdateTopIndex();
        }

        private void UpdateTopIndex()
        {
            TopIndex = new Vector2(
                this.offset.X / ElementSize.X,
                this.offset.Y / ElementSize.Y);
        }

        private void UpdateInput(bool active, float maxSpeed)
        {
            // Called functions return true if the user input something that
            // affects scroll position
            bool gotUserInput = false;
            if (Direction.HasFlag(ScrollDirections.Horizontal))
                gotUserInput |= UpdateHorizontalInput(active, maxSpeed);
            if (Direction.HasFlag(ScrollDirections.Vertical))
                gotUserInput |= UpdateVerticalInput(active, maxSpeed);
        }

        protected virtual bool UpdateHorizontalInput(bool active, float maxSpeed)
        {
            if (active)
            {
                // Buttons
                if (Global.Input.pressed(Inputs.Left))
                {
                    if (ScrollSpeed.X > 0)
                        ScrollSpeed.X = 0;
                    if (ScrollSpeed.X > -maxSpeed)
                        ScrollSpeed.X--;
                    return true;
                }
                else if (Global.Input.pressed(Inputs.Right))
                {
                    if (ScrollSpeed.X < 0)
                        ScrollSpeed.X = 0;
                    if (ScrollSpeed.X < maxSpeed)
                        ScrollSpeed.X++;
                    return true;
                }
                // Mouse scroll wheel (if only horizontal scrolling is allowed)
                else if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll < 0)
                {
                    ScrollSpeed.X += maxSpeed / 5;
                    ScrollWheel = true;
                    return true;
                }
                else if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll > 0)
                {
                    ScrollSpeed.X += -maxSpeed / 5;
                    ScrollWheel = true;
                    return true;
                }
                // Mouse
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    LeftMouseOver != null && LeftMouseOver.MouseOver())
                {
                    ScrollSpeed.X = -MaxScrollSpeed;
                    // If only horizontal
                    if (Direction == ScrollDirections.Horizontal)
                        ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    RightMouseOver != null && RightMouseOver.MouseOver())
                {
                    ScrollSpeed.X = MaxScrollSpeed;
                    // If only horizontal
                    if (Direction == ScrollDirections.Horizontal)
                        ScrollWheel = false;
                    return true;
                }
                // Touch gestures
                else if (Global.Input.gesture_rectangle(
                    TouchGestures.HorizontalDrag, this.ViewAreaRectangle, false))
                {
                    ScrollSpeed.X = -(int)Global.Input.horizontalDragVector.X;
                    return true;
                }
            }

            UpdateHorizontalScroll();
            return false;
        }
        protected void UpdateHorizontalScroll()
        {
            // If scrolling and there were no inputs, decelerate/etc
            if (ScrollSpeed.X != 0)
            {
                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    ScrollSpeed.X = (float)Additional_Math.double_closer(
                        ScrollSpeed.X, 0, 1);
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse)
                {
                    if (Direction == ScrollDirections.Horizontal && ScrollWheel)
                        ScrollSpeed.X *= (float)Math.Pow(
                            ScrollFriction, 2f);
                    else
                        ScrollSpeed.X = (float)Additional_Math.double_closer(
                        ScrollSpeed.X, 0, 1);
                }
                else
                {
                    ScrollSpeed.X *= ScrollFriction;
                }

                if (Math.Abs(ScrollSpeed.X) < 0.1f)
                {
                    ScrollSpeed.X = 0;
                    // If only horizontal
                    if (Direction == ScrollDirections.Horizontal)
                        ScrollWheel = false;
                }
            }
        }

        protected virtual bool UpdateVerticalInput(bool active, float maxSpeed)
        {
            if (active)
            {
                // Buttons
                if (Global.Input.pressed(Inputs.Up))
                {
                    if (ScrollSpeed.Y > 0)
                        ScrollSpeed.Y = 0;
                    if (ScrollSpeed.Y > -maxSpeed)
                        ScrollSpeed.Y--;
                    return true;
                }
                else if (Global.Input.pressed(Inputs.Down))
                {
                    if (ScrollSpeed.Y < 0)
                        ScrollSpeed.Y = 0;
                    if (ScrollSpeed.Y < maxSpeed)
                        ScrollSpeed.Y++;
                    return true;
                }
                // Mouse scroll wheel
                else if (Global.Input.mouseScroll < 0)
                {
                    ScrollSpeed.Y += maxSpeed / 5;
                    ScrollWheel = true;
                    return true;
                }
                else if (Global.Input.mouseScroll > 0)
                {
                    ScrollSpeed.Y += -maxSpeed / 5;
                    ScrollWheel = true;
                    return true;
                }
                // Mouse
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    UpMouseOver != null && UpMouseOver.MouseOver())
                {
                    ScrollSpeed.Y = -MaxScrollSpeed;
                    ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    DownMouseOver != null && DownMouseOver.MouseOver())
                {
                    ScrollSpeed.Y = MaxScrollSpeed;
                    ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Scrollbar != null && Scrollbar.UpHeld)
                {
                    ScrollSpeed.Y = -MaxScrollSpeed;
                    ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Scrollbar != null && Scrollbar.DownHeld)
                {
                    ScrollSpeed.Y = MaxScrollSpeed;
                    ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Scrollbar != null && Scrollbar.Scrubbing)
                {
                    ScrollSpeed.Y = 0;
                    this.offset.Y = Scrollbar.ScrubPercent * (this.MaxOffset.Y - this.MinOffset.Y);
                    ScrollWheel = false;
                    return true;
                }
                // Touch gestures
                else if (Global.Input.gesture_rectangle(
                    TouchGestures.VerticalDrag, this.ViewAreaRectangle, false))
                {
                    ScrollSpeed.Y = -(int)Global.Input.verticalDragVector.Y;
                    return true;
                }
            }

            UpdateVerticalScroll();
            return false;
        }
        protected void UpdateVerticalScroll()
        {
            // If scrolling and there were no inputs, decelerate/etc
            if (ScrollSpeed.Y != 0)
            {
                if (Input.ControlScheme == ControlSchemes.Buttons)
                {
                    ScrollSpeed.Y = (float)Additional_Math.double_closer(
                        ScrollSpeed.Y, 0, 1);
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse)
                {
                    if (ScrollWheel)
                        ScrollSpeed.Y *= (float)Math.Pow(
                            ScrollFriction, 2f);
                    else
                        ScrollSpeed.Y = (float)Additional_Math.double_closer(
                            ScrollSpeed.Y, 0, 1);
                }
                else
                {
                    ScrollSpeed.Y *= ScrollFriction;
                }

                if (Math.Abs(ScrollSpeed.Y) < 0.1f)
                {
                    ScrollSpeed.Y = 0;
                    ScrollWheel = false;
                }
            }
        }

#if DEBUG
        public void Draw(SpriteBatch spriteBatch, Vector2 drawOffset = default(Vector2))
        {
            var texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Color color = new Color(0, 80, 80, 40);
            spriteBatch.Draw(texture, this.loc, new Rectangle(0, 0, 16, 16),
                color, 0f, this.offset, ViewAreaSize / new Vector2(16f), SpriteEffects.None, 0f);
        }
#endif
    }
}
