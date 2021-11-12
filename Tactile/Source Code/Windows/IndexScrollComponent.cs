using System;
using Microsoft.Xna.Framework;

namespace Tactile.Windows
{
    class IndexScrollComponent :ScrollComponent
    {
        private Vector2 OffsetTarget;
        private bool IndexedScroll = true;

        public IndexScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollDirections direction)
            : base(viewAreaSize, elementSize, direction) { }

        public override void Update(bool active, int index = -1)
        {
            base.Update(active, index);

            if (!IndexedScroll || ScrollWheel)
                FixIndex();
            if (!IndexedScroll)
                OffsetTarget = this.offset  / ElementSize;
        }

        protected override void UpdateHorizontalInput(bool active, float maxSpeed)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                base.UpdateHorizontalInput(active, maxSpeed);
                IndexedScroll = false;
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
                        int buffer = 1;
                        Vector2 index = this.ScrollIndex;
                        float rightTarget = index.X + buffer - (int)ViewableElements.X;
                        float leftTarget = index.X + 1 - buffer;
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
                        return;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        RightMouseOver != null && RightMouseOver.MouseOver())
                    {
                        ScrollSpeed.X = maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return;
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
            }
        }

        protected override void UpdateVerticalInput(bool active, float maxSpeed)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                base.UpdateVerticalInput(active, maxSpeed);
                IndexedScroll = false;
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
                        int buffer = 1;
                        Vector2 index = this.ScrollIndex;
                        float downTarget = index.Y + buffer - (int)ViewableElements.Y;
                        float upTarget = index.Y + 1 - buffer;
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
                        return;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        DownMouseOver != null && DownMouseOver.MouseOver())
                    {
                        ScrollSpeed.Y = maxSpeed / 5f;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return;
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
            }
        }
    }
}
