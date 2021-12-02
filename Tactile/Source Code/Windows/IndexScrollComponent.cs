using System;
using Microsoft.Xna.Framework;

namespace Tactile.Windows
{
    class IndexScrollComponent : ScrollComponent
    {
        private Vector2 OffsetTarget;
        private bool IndexedScroll = true;

        public override DirectionFlags ScrollDirection
        {
            get
            {
                if (IndexedScroll)
                {
                    DirectionFlags result = DirectionFlags.None;
                    Vector2 target = OffsetTarget * ElementSize;

                    if (this.offset.X > target.X && !this.AtLeft)
                        result |= DirectionFlags.Left;
                    else if (this.offset.X < target.X && !this.AtRight)
                        result |= DirectionFlags.Right;

                    if (this.offset.Y > target.Y && !this.AtTop)
                        result |= DirectionFlags.Up;
                    else if (this.offset.Y < target.Y && !this.AtBottom)
                        result |= DirectionFlags.Down;

                    return result;
                }
                else
                {
                    return base.ScrollDirection;
                }
            }
        }

        public IndexScrollComponent(Vector2 viewAreaSize, Vector2 elementSize, ScrollAxes direction)
            : base(viewAreaSize, elementSize, direction) { }

        protected override void SetOffset(Vector2 offset)
        {
            base.SetOffset(offset);
            ResetOffsetTarget();
        }

        private void ResetOffsetTarget()
        {
            OffsetTarget = this.offset / ElementSize;
        }

        public override void Update(bool active, int index = -1, Vector2 drawOffset = default(Vector2))
        {
            base.Update(active, index, drawOffset);

            if (!IndexedScroll || ScrollWheel)
                FixIndex();
            if (!IndexedScroll)
                ResetOffsetTarget();
        }

        protected override bool UpdateHorizontalInput(bool active, float baseSpeed, float maxSpeed, Vector2 drawOffset)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                bool result = base.UpdateHorizontalInput(active, baseSpeed, maxSpeed, drawOffset);
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
                    if (Direction == ScrollAxes.Horizontal && Global.Input.mouseScroll < 0)
                    {
                        if (OffsetTarget.X < ElementLengths.X - (int)ViewableElements.X)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.X++;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    else if (Direction == ScrollAxes.Horizontal && Global.Input.mouseScroll > 0)
                    {
                        if (OffsetTarget.X > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.X--;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    // Mouse
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        LeftMouseOver != null && LeftMouseOver.MouseOver())
                    {
                        ScrollSpeed.X = -baseSpeed;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        RightMouseOver != null && RightMouseOver.MouseOver())
                    {
                        ScrollSpeed.X = baseSpeed;
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

        protected override bool UpdateVerticalInput(bool active, float baseSpeed, float maxSpeed, Vector2 drawOffset)
        {
            if (Input.ControlScheme == ControlSchemes.Touch)
            {
                bool result = base.UpdateVerticalInput(active, baseSpeed, maxSpeed, drawOffset);
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
                        if (OffsetTarget.Y < ElementLengths.Y - (int)ViewableElements.Y)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.Y++;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    else if (Global.Input.mouseScroll > 0)
                    {
                        if (OffsetTarget.Y > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.Y--;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    // Mouse
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        UpMouseOver != null && UpMouseOver.MouseOver())
                    {
                        ScrollSpeed.Y = -baseSpeed;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        DownMouseOver != null && DownMouseOver.MouseOver())
                    {
                        ScrollSpeed.Y = baseSpeed;
                        IndexedScroll = false;
                        ScrollWheel = false;
                        return true;
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        Scrollbar != null && Scrollbar.UpTriggered)
                    {
                        if (OffsetTarget.Y > 0)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.Y--;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        Scrollbar != null && Scrollbar.DownTriggered)
                    {
                        if (OffsetTarget.Y < ElementLengths.Y - (int)ViewableElements.Y)
                        {
                            Global.game_system.play_se(System_Sounds.Menu_Move1);
                            OffsetTarget.Y++;
                            IndexedScroll = true;
                            ScrollWheel = true;
                        }
                    }
                    else if (Input.ControlScheme == ControlSchemes.Mouse &&
                        Scrollbar != null && Scrollbar.Scrubbing)
                    {
                        ScrollSpeed.Y = 0;
                        this.offset.Y = Scrollbar.ScrubPercent * (this.MaxOffset.Y - this.MinOffset.Y);
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

        public Vector2 ClampOffset(Vector2 objectLoc)
        {
            Vector2 result = -this.IntOffset;
            Vector2 offsetTarget = OffsetTarget;
            if (!IndexedScroll)
                offsetTarget = this.offset / ElementSize;

            Vector2 viewable = new Vector2(
                (int)this.ViewableElements.X, (int)this.ViewableElements.Y);
            Vector2 offsetElements = ElementLengths - viewable;
            Vector2 target = (offsetTarget - this.ViewableElements) * ElementSize;

            // Clamp horizontally
            bool atLeft = offsetTarget.X >= offsetElements.X;
            float left = (viewable.X - (atLeft ? 1 : this.LeftBuffer)) * ElementSize.X;
            result.X = Math.Min(result.X, left - objectLoc.X);

            bool atRight = offsetTarget.X <= 0;
            float right = (atRight ? 0 : this.RightBuffer - 1) * ElementSize.X;
            result.X = Math.Max(result.X, right - objectLoc.X);

            // Clamp vertically
            bool atBottom = offsetTarget.Y >= offsetElements.Y;
            float bottom = (viewable.Y - (atBottom ? 1 : this.BottomBuffer)) * ElementSize.Y;
            result.Y = Math.Min(result.Y, bottom - objectLoc.Y);

            bool atTop = offsetTarget.Y <= 0;
            float top = (atTop ? 0 : this.TopBuffer - 1) * ElementSize.Y;
            result.Y = Math.Max(result.Y, top - objectLoc.Y);

            return result;
        }
    }
}
