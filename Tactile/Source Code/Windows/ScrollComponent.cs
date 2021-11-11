using System;
using Microsoft.Xna.Framework;
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
        private Vector2 ElementSize;
        private ScrollDirections Direction;
        private Vector2 ElementLengths = Vector2.One;
        private Vector2 TopIndex = Vector2.Zero;
        public int Index { get; private set; }
        private Vector2 ScrollSpeed = Vector2.Zero;
        private bool ScrollWheel = false;

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
        private Vector2 MinOffset { get { return new Vector2(0, 0); } }
        private Vector2 MaxOffset
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

        public ScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollDirections direction)
        {
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
        public void SetScrollSpeeds(float maxScrollSpeed, float scrollFriction)
        {
            MaxScrollSpeed = maxScrollSpeed;
            ScrollFriction = scrollFriction;
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

        public void Update(bool active, int index = -1)
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

            TopIndex = new Vector2(
                this.offset.X / ElementSize.X,
                this.offset.Y / ElementSize.Y);
        }

        private void UpdateInput(bool active, float maxSpeed)
        {
            if (Direction.HasFlag(ScrollDirections.Horizontal))
                UpdateHorizontalInput(active, maxSpeed);
            if (Direction.HasFlag(ScrollDirections.Vertical))
                UpdateVerticalInput(active, maxSpeed);
        }

        private void UpdateHorizontalInput(bool active, float maxSpeed)
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
                    return;
                }
                else if (Global.Input.pressed(Inputs.Right))
                {
                    if (ScrollSpeed.X < 0)
                        ScrollSpeed.X = 0;
                    if (ScrollSpeed.X < maxSpeed)
                        ScrollSpeed.X++;
                    return;
                }
                // Mouse scroll wheel (if only horizontal scrolling is allowed)
                else if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll < 0)
                {
                    ScrollSpeed.X += maxSpeed / 5;
                    ScrollWheel = true;
                    return;
                }
                else if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll > 0)
                {
                    ScrollSpeed.X += -maxSpeed / 5;
                    ScrollWheel = true;
                    return;
                }
                // Mouse
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    LeftMouseOver != null && LeftMouseOver.MouseOver())
                {
                    ScrollSpeed.X = -MaxScrollSpeed;
                    // If only horizontal
                    if (Direction == ScrollDirections.Horizontal)
                        ScrollWheel = false;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    RightMouseOver != null && RightMouseOver.MouseOver())
                {
                    ScrollSpeed.X = MaxScrollSpeed;
                    // If only horizontal
                    if (Direction == ScrollDirections.Horizontal)
                        ScrollWheel = false;
                    return;
                }
                // Touch gestures
                else if (Global.Input.gesture_rectangle(
                    TouchGestures.HorizontalDrag, this.ViewAreaRectangle, false))
                {
                    ScrollSpeed.X = -(int)Global.Input.horizontalDragVector.X;
                    return;
                }
            }

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
        private void UpdateVerticalInput(bool active, float maxSpeed)
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
                    return;
                }
                else if (Global.Input.pressed(Inputs.Down))
                {
                    if (ScrollSpeed.Y < 0)
                        ScrollSpeed.Y = 0;
                    if (ScrollSpeed.Y < maxSpeed)
                        ScrollSpeed.Y++;
                    return;
                }
                // Mouse scroll wheel
                else if (Global.Input.mouseScroll < 0)
                {
                    ScrollSpeed.Y += maxSpeed / 5;
                    ScrollWheel = true;
                    return;
                }
                else if (Global.Input.mouseScroll > 0)
                {
                    ScrollSpeed.Y += -maxSpeed / 5;
                    ScrollWheel = true;
                    return;
                }
                // Mouse
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    UpMouseOver != null && UpMouseOver.MouseOver())
                {
                    ScrollSpeed.Y = -MaxScrollSpeed;
                    ScrollWheel = false;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    DownMouseOver != null && DownMouseOver.MouseOver())
                {
                    ScrollSpeed.Y = MaxScrollSpeed;
                    ScrollWheel = false;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Scrollbar != null && Scrollbar.UpHeld)
                {
                    ScrollSpeed.Y = -MaxScrollSpeed;
                    ScrollWheel = false;
                    return;
                }
                else if (Input.ControlScheme == ControlSchemes.Mouse &&
                    Scrollbar != null && Scrollbar.DownHeld)
                {
                    ScrollSpeed.Y = MaxScrollSpeed;
                    ScrollWheel = false;
                    return;
                }
                // Touch gestures
                else if (Global.Input.gesture_rectangle(
                    TouchGestures.VerticalDrag, this.ViewAreaRectangle, false))
                {
                    ScrollSpeed.Y = -(int)Global.Input.verticalDragVector.Y;
                    return;
                }
            }

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
    }
}
