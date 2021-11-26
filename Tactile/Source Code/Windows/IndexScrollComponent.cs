using System;
using Microsoft.Xna.Framework;

namespace Tactile.Windows
{
    class IndexScrollComponent : ScrollComponent
    {
        private Vector2 OffsetTarget;
        private bool IndexedScroll = true;
        private Rectangle Buffers = new Rectangle(1, 1, 2, 2);

        public IndexScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollDirections direction)
            : base(viewAreaSize, elementSize, direction) { }

        public void SetBuffers(Rectangle buffers)
        {
            if (buffers.X < 1)
            {
                buffers.Width += 1 - buffers.X;
                buffers.X = 1;
            }
            if (buffers.Width < 2)
                buffers.Width = 2;

            if (buffers.Y < 1)
            {
                buffers.Height += 1 - buffers.Y;
                buffers.Y = 1;
            }
            if (buffers.Height < 2)
                buffers.Height = 2;

            Buffers = buffers;
        }

        private int LeftBuffer { get { return Buffers.X; } }
        private int RightBuffer { get { return Buffers.Width - Buffers.X; } }
        private int TopBuffer { get { return Buffers.Y; } }
        private int BottomBuffer { get { return Buffers.Height - Buffers.Y; } }

        protected override void SetOffset(Vector2 offset)
        {
            base.SetOffset(offset);
            ResetOffsetTarget();
        }

        private void ResetOffsetTarget()
        {
            OffsetTarget = this.offset / ElementSize;
        }

        public override void Update(bool active, int index = -1)
        {
            base.Update(active, index);

            if (!IndexedScroll || ScrollWheel)
                FixIndex();
            if (!IndexedScroll)
                ResetOffsetTarget();
        }

        protected override bool UpdateHorizontalInput(bool active, float maxSpeed)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                bool result = base.UpdateHorizontalInput(active, maxSpeed);
                IndexedScroll = false;
                return result;
            }
            else
            {
                ScrollSpeed.X = 0;

                if (active)
                {
                    // Buttons
                    if (Input.ControlScheme == ControlSchemes.Buttons)
                    {
                        // Scroll to the active index
                        Vector2 index = this.ScrollIndex;
                        float rightTarget = index.X + this.RightBuffer - (int)ViewableElements.X;
                        float leftTarget = index.X + 1 - this.LeftBuffer;
                        if (rightTarget > TopIndex.X)
                        {
                            OffsetTarget.X = rightTarget;
                        }
                        else if (leftTarget < TopIndex.X)
                        {
                            OffsetTarget.X = leftTarget;
                        }

                        IndexedScroll = true;
                    }
                    // Mouse scroll wheel (if only horizontal scrolling is allowed)
                    if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll < 0)
                    {
                        OffsetTarget.X++;
                        IndexedScroll = true;
                        ScrollWheel = true;
                    }
                    else if (Direction == ScrollDirections.Horizontal && Global.Input.mouseScroll > 0)
                    {
                        OffsetTarget.X--;
                        IndexedScroll = true;
                        ScrollWheel = true;
                    }
                    // Mouse
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        LeftMouseOver != null && LeftMouseOver.MouseOver())
                    {
                        ScrollSpeed.X = -maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        RightMouseOver != null && RightMouseOver.MouseOver())
                    {
                        ScrollSpeed.X = maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                }

                if (IndexedScroll)
                {
                    ScrollSpeed.X = 0;

                    OffsetTarget.X = MathHelper.Clamp(OffsetTarget.X,
                        0, ElementLengths.X - (int)ViewableElements.X);

                    float xTarget = MathHelper.Clamp(OffsetTarget.X * ElementSize.X,
                        this.MinOffset.X, this.MaxOffset.X);
                    if (Math.Abs(this.offset.X - xTarget) < 2)
                    {
                        this.offset.X = xTarget;
                        ScrollWheel = false;
                    }
                    else
                        this.offset.X = (int)MathHelper.Lerp(
                            this.offset.X, xTarget, 0.5f);
                }
                else
                    base.UpdateHorizontalScroll();

                return IndexedScroll;
            }
        }

        protected override bool UpdateVerticalInput(bool active, float maxSpeed)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                bool result = base.UpdateVerticalInput(active, maxSpeed);
                IndexedScroll = false;
                return result;
            }
            else
            {
                ScrollSpeed.Y = 0;

                if (active)
                {
                    // Buttons
                    if (Input.ControlScheme == ControlSchemes.Buttons)
                    {
                        // Scroll to the active index
                        Vector2 index = this.ScrollIndex;
                        float downTarget = index.Y + this.BottomBuffer - (int)ViewableElements.Y;
                        float upTarget = index.Y + 1 - this.TopBuffer;
                        if (downTarget > TopIndex.Y)
                        {
                            OffsetTarget.Y = downTarget;
                        }
                        else if (upTarget < TopIndex.Y)
                        {
                            OffsetTarget.Y = upTarget;
                        }

                        IndexedScroll = true;
                    }
                    // Mouse scroll wheel
                    if (Global.Input.mouseScroll < 0)
                    {
                        OffsetTarget.Y++;
                        IndexedScroll = true;
                        ScrollWheel = true;
                    }
                    else if (Global.Input.mouseScroll > 0)
                    {
                        OffsetTarget.Y--;
                        IndexedScroll = true;
                        ScrollWheel = true;
                    }
                    // Mouse
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        UpMouseOver != null && UpMouseOver.MouseOver())
                    {
                        ScrollSpeed.Y = -maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        DownMouseOver != null && DownMouseOver.MouseOver())
                    {
                        ScrollSpeed.Y = maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                }

                if (IndexedScroll)
                {
                    ScrollSpeed.Y = 0;

                    OffsetTarget.Y = MathHelper.Clamp(OffsetTarget.Y,
                        0, ElementLengths.Y - (int)ViewableElements.Y);

                    float yTarget = MathHelper.Clamp(OffsetTarget.Y * ElementSize.Y,
                        this.MinOffset.Y, this.MaxOffset.Y);
                    if (Math.Abs(this.offset.Y - yTarget) < 2)
                    {
                        this.offset.Y = yTarget;
                        ScrollWheel = false;
                    }
                    else
                        this.offset.Y = (int)MathHelper.Lerp(
                            this.offset.Y, yTarget, 0.5f);
                }
                else
                    base.UpdateHorizontalScroll();

                return IndexedScroll;
            }
        }
    }
}
