using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.UserInterface;

namespace Tactile.Windows
{
    enum ScrollAxes : byte
    {
        Vertical =      1 << 0,
        Horizontal =    1 << 1,
        Free =          Vertical | Horizontal
    }

    class ScrollComponent : Graphic_Object
    {
        private Vector2 ViewAreaSize;
        protected Vector2 ElementSize { get; private set; }
        protected ScrollAxes Direction { get; private set; }
        protected Vector2 ElementLengths { get; private set; }
        protected Vector2 TopIndex { get; private set; }
        public int Index { get; private set; }
        protected Vector2 ScrollSpeed = Vector2.Zero;
        protected bool ScrollWheel = false;
        protected bool ResolveToIndex = false;

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
                if (!Direction.HasFlag(ScrollAxes.Horizontal))
                    result.X = 0;
                if (!Direction.HasFlag(ScrollAxes.Vertical))
                    result.Y = 0;
                // Ceil to element scale if resolving to index
                if (ResolveToIndex)
                {
                    result.X = (float)Math.Ceiling(result.X / ElementSize.X) * ElementSize.X;
                    result.Y = (float)Math.Ceiling(result.Y / ElementSize.Y) * ElementSize.Y;
                }
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
                    case ScrollAxes.Horizontal:
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

        public bool IsScrolling
        {
            get
            {
                if (ResolveToIndex)
                {
                    Vector2 scrollElement = this.offset / ElementSize;
                    Vector2 targetOffset = new Vector2(
                        (float)Math.Round(scrollElement.X),
                        (float)Math.Round(scrollElement.Y));

                    if (scrollElement != targetOffset)
                        return true;
                }

                return this.ScrollDirection != DirectionFlags.None;
            }
        }
        /// <summary>
        /// Returns the direction actively being scrolling in.
        /// </summary>
        public virtual DirectionFlags ScrollDirection
        {
            get
            {
                DirectionFlags result = DirectionFlags.None;

                if (ScrollSpeed.X < 0 && !this.AtLeft)
                    result |= DirectionFlags.Left;
                else if (ScrollSpeed.X > 0 && !this.AtRight)
                    result |= DirectionFlags.Right;

                if (ScrollSpeed.Y < 0 && !this.AtTop)
                    result |= DirectionFlags.Up;
                else if (ScrollSpeed.Y > 0 && !this.AtBottom)
                    result |= DirectionFlags.Down;

                return result;
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

        public ScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollAxes direction)
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

        /// <summary>
        /// Set if the scroll should always end up even with an index.
        /// </summary>
        public void SetResolveToIndex(bool value)
        {
            ResolveToIndex = value;
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
                    case ScrollAxes.Horizontal:
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

            if (Scrollbar != null)
            {
                DirectionFlags dir = this.ScrollDirection;
                if (Direction != ScrollAxes.Horizontal)
                {
                    if (dir.HasFlag(DirectionFlags.Up))
                        Scrollbar.moving_up();
                    else if (dir.HasFlag(DirectionFlags.Down))
                        Scrollbar.moving_down();
                }
            }
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
            if (Direction.HasFlag(ScrollAxes.Horizontal))
                gotUserInput |= UpdateHorizontalInput(active, maxSpeed);
            if (Direction.HasFlag(ScrollAxes.Vertical))
                gotUserInput |= UpdateVerticalInput(active, maxSpeed);

            // If the player didn't input anything, check for resolving the index
            if (!gotUserInput)
                UpdateResolvingIndex();
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
                else if (Direction == ScrollAxes.Horizontal && Global.Input.mouseScroll < 0)
                {
                    ScrollSpeed.X += maxSpeed / 5;
                    ScrollWheel = true;
                    return true;
                }
                else if (Direction == ScrollAxes.Horizontal && Global.Input.mouseScroll > 0)
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
                    if (Direction == ScrollAxes.Horizontal)
                        ScrollWheel = false;
                    return true;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    RightMouseOver != null && RightMouseOver.MouseOver())
                {
                    ScrollSpeed.X = MaxScrollSpeed;
                    // If only horizontal
                    if (Direction == ScrollAxes.Horizontal)
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
                    if (Direction == ScrollAxes.Horizontal && ScrollWheel)
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
                    if (Direction == ScrollAxes.Horizontal)
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

        /// <summary>
        /// Adjusts scrolling speed so that the scroll position will end up
        /// aligned with an element once it stops moving.
        /// </summary>
        protected void UpdateResolvingIndex()
        {
            if (ResolveToIndex)
            {
                // If not moving and already resolved to an index
                if (ScrollSpeed.LengthSquared() < 0.001f &&
                        (this.offset.X / ElementSize.X) % 1 == 0 &&
                        (this.offset.Y / ElementSize.Y) % 1 == 0)
                    // Short circuit out early if we're all good
                    return;

                Vector2 absRemainingScroll = GetAbsRemainingScroll();
                Vector2 remainingScroll = absRemainingScroll;
                if (ScrollSpeed.X < 0)
                    remainingScroll.X *= -1;
                if (ScrollSpeed.Y < 0)
                    remainingScroll.Y *= -1;

                Vector2 resultOffset = Vector2.Clamp(this.offset + remainingScroll,
                    this.MinOffset, this.MaxOffset);
                Vector2 scrollElement = resultOffset / ElementSize;
                // Not scrolling too fast, and still scrolling at all or misaligned
                bool xScrollEnding = absRemainingScroll.X < ElementSize.X &&
                    (absRemainingScroll.X > 0 || scrollElement.X % 1 != 0);
                bool yScrollEnding = absRemainingScroll.Y < ElementSize.Y &&
                    (absRemainingScroll.Y > 0 || scrollElement.Y % 1 != 0);

                if (xScrollEnding || yScrollEnding)
                {
                    // Get the scroll we want to end up on
                    Vector2 targetOffset = new Vector2(
                        (float)Math.Round(scrollElement.X),
                        (float)Math.Round(scrollElement.Y));
                    targetOffset *= ElementSize;

                    Vector2 targetDistance = targetOffset - this.offset;
                    // If not going far enough, or going the wrong direction
                    if (targetDistance.X > 0 ?
                        targetDistance.X > remainingScroll.X : targetDistance.X < remainingScroll.X)
                    {
                        if (targetDistance.X > 0)
                            ScrollSpeed.X = Math.Max(1, targetDistance.X / 4f);
                        else
                            ScrollSpeed.X = Math.Min(-1, targetDistance.X / 4f);
                        remainingScroll.X = GetRemainingScroll().X;
                    }
                    if (targetDistance.Y > 0 ?
                        targetDistance.Y > remainingScroll.Y : targetDistance.Y < remainingScroll.Y)
                    {
                        if (targetDistance.Y > 0)
                            ScrollSpeed.Y = Math.Max(1, targetDistance.Y / 4f);
                        else
                            ScrollSpeed.Y = Math.Min(-1, targetDistance.Y / 4f);
                        remainingScroll.Y = GetRemainingScroll().Y;
                    }

                    // If scrolling too far
                    if (targetDistance.X > 0 ?
                        targetDistance.X < remainingScroll.X : targetDistance.X > remainingScroll.X)
                    {
                        ScrollSpeed.X *= targetDistance.X / remainingScroll.X;
                        if (Math.Abs(targetDistance.X) < 1)
                            ScrollSpeed.X = targetDistance.X;
                        else if (Math.Abs(ScrollSpeed.X) < 1)
                            ScrollSpeed.X = targetDistance.X > 0 ? 1 : -1;
                    }
                    if (targetDistance.Y > 0 ?
                        targetDistance.Y < remainingScroll.Y : targetDistance.Y > remainingScroll.Y)
                    {
                        float ratio = targetDistance.Y / remainingScroll.Y;
                        ratio = (float)Math.Pow(ratio, 0.67f);
                        ScrollSpeed.Y *= ratio;
                        // If close enough, set to remaining distance
                        if (Math.Abs(targetDistance.Y) < 1)
                            ScrollSpeed.Y = targetDistance.Y;
                        else if (Math.Abs(ScrollSpeed.Y) < 1)
                            ScrollSpeed.Y = targetDistance.Y > 0 ? 1 : -1;
                    }
                }
            }
        }

        private Vector2 GetAbsRemainingScroll()
        {
            if (Input.ControlScheme == ControlSchemes.Buttons)
            {
                return GetRemainingScroll(ScrollSpeed, false);
            }
            else if (Input.ControlScheme == ControlSchemes.Mouse)
            {
                if (ScrollWheel)
                {
                    float friction = (float)Math.Pow(ScrollFriction, 2f);
                    if (Direction == ScrollAxes.Horizontal)
                        return GetRemainingScroll(new Vector2(ScrollSpeed.X, 0), true, friction);
                    else
                        return GetRemainingScroll(new Vector2(0, ScrollSpeed.Y), true, friction);
                }
                else
                {
                    return GetRemainingScroll(ScrollSpeed, false);
                }
            }
            else
            {
                return GetRemainingScroll(ScrollSpeed, true, ScrollFriction);
            }
        }
        private Vector2 GetRemainingScroll()
        {
            Vector2 remainingScroll = GetAbsRemainingScroll();
            if (ScrollSpeed.X < 0)
                remainingScroll.X *= -1;
            if (ScrollSpeed.Y < 0)
                remainingScroll.Y *= -1;
            return remainingScroll;
        }

        /// <summary>
        /// Determines how much further scrolling will go.
        /// Returns the absolute value of the result.
        /// </summary>
        /// <param name="speed">The current scroll speed.</param>
        /// <param name="useFriction">Whether friction is used for deceleration.</param>
        private static Vector2 GetRemainingScroll(Vector2 speed, bool useFriction, float friction = 0.5f)
        {
            Vector2 result = Vector2.Zero;
            speed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));

            if (useFriction)
            {
                if (speed.X > 0.1f)
                {
                    float steps = 1 + (float)Math.Max(0, Math.Log(1.0f / speed.X, friction));
                    result.X = (float)(speed.X * (1 - Math.Pow(friction, steps)) / (1 - friction));
                }
                if (speed.Y > 0.1f)
                {
                    float steps = 1 + (float)Math.Max(0, Math.Log(1.0f / speed.Y, friction));
                    result.Y = (float)(speed.Y * (1 - Math.Pow(friction, steps)) / (1 - friction));
                }
            }
            else
            {
                result.X = ((speed.X + 1) * speed.X) / 2;
                result.Y = ((speed.Y + 1) * speed.Y) / 2;
            }

            return result;
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
