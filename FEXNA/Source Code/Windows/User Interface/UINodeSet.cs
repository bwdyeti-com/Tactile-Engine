using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EnumExtension;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface
{
    class UINodeSet<T> : IEnumerable<T> where T : UINode
    {
        internal int ActiveNodeIndex { get; private set; }
        protected List<T> Nodes;
        private Dictionary<T, Vector2> NodeOriginalLocations;
        private NodeDestinationMap Destinations;
        private bool DestinationFromCenter = false;

        internal float AngleMultiplier = 1f;
        internal List<CardinalDirections> TangentDirections;
        internal System_Sounds CursorMoveSound = System_Sounds.Menu_Move2;
        internal Maybe<System_Sounds> HorizontalCursorMoveSound =
            default(Maybe<System_Sounds>);
        internal bool SoundOnMouseMove = false;
        internal bool WrapVerticalMove = false;
        internal bool WrapVerticalSameColumn = false;

        internal T ActiveNode
        {
            get { return ActiveNodeIndex > -1 ? Nodes[ActiveNodeIndex] : null; }
        }

        internal int Count { get { return Nodes.Count; } }

        internal UINodeSet(IEnumerable<T> set)
        {
            ActiveNodeIndex = -1;
            Nodes = new List<T>(set);
            refresh_destinations();
        }

        #region Interface
        public IEnumerator<T> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Nodes.Count)
                    throw new IndexOutOfRangeException();
                return Nodes[index];
            }
        }

        internal void refresh_destinations()
        {
            NodeOriginalLocations = Nodes
                .ToDictionary(x => x, x => node_location(x));
            var enabled_nodes = NodeOriginalLocations
                .Where(x => x.Key.Enabled)
                .Select(x => x.Value);
            if (TangentDirections != null)
                Destinations = NodeDestinationMap
                    .construct(enabled_nodes, TangentDirections, AngleMultiplier);
            else
                Destinations = NodeDestinationMap
                    .construct(enabled_nodes, AngleMultiplier);

            if (ActiveNodeIndex == -1 && Nodes.Any())
                set_active_node(Nodes[0]);
        }

        private Vector2 node_location(T node)
        {
            if (DestinationFromCenter)
                return node.CenterPoint;
            else
                return node.loc;
        }

        public void Update(bool input,
            Vector2 draw_offset = default(Vector2))
        {
            if (input)
                update_input();
            foreach (var node in Nodes)
                node.Update(this, input, draw_offset);
        }
        public void Update(ControlSet input,
            Vector2 draw_offset = default(Vector2))
        {
            if (input.HasEnumFlag(ControlSet.Buttons))
                update_input();
            foreach (var node in Nodes)
                node.Update(this, input, draw_offset);
        }

        protected void update_input()
        {
            CardinalDirections dir = CardinalDirections.None;
            if (Global.Input.repeated(Inputs.Down))
                dir = CardinalDirections.Down;
            else if (Global.Input.repeated(Inputs.Up))
                dir = CardinalDirections.Up;
            else if (Global.Input.repeated(Inputs.Left))
                dir = CardinalDirections.Left;
            else if (Global.Input.repeated(Inputs.Right))
                dir = CardinalDirections.Right;

            if (dir != CardinalDirections.None)
                if (move_node(dir))
                {
                    if (HorizontalCursorMoveSound.IsSomething &&
                            (dir == CardinalDirections.Left ||
                            dir == CardinalDirections.Right))
                        Global.game_system.play_se(HorizontalCursorMoveSound);
                    else
                        Global.game_system.play_se(CursorMoveSound);
                }
        }

        public Maybe<int> consume_triggered(Inputs input, MouseButtons button, TouchGestures gesture)
        {
            var result = consume_triggered(input);
            if (result.IsNothing)
                result = consume_triggered(button);
            if (result.IsNothing)
                result = consume_triggered(gesture);
            return result;
        }
        /* //Debug
        public Maybe<int> consume_triggered(Inputs input, MouseButtons button)
        {
            var result = consume_triggered(input);
            if (result.IsNothing)
                result = consume_triggered(button);
            return result;
        }*/
        public Maybe<int> consume_triggered(MouseButtons button, TouchGestures gesture)
        {
            var result = consume_triggered(button);
            if (result.IsNothing)
                result = consume_triggered(gesture);
            return result;
        }
        public Maybe<int> consume_triggered(Inputs input)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].consume_trigger(input))
                {
                    clear_triggers();
                    return i;
                }
            }
            return default(Maybe<int>);
        }
        public Maybe<int> consume_triggered(MouseButtons button)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].consume_trigger(button))
                {
                    clear_triggers();
                    return i;
                }
            }
            return default(Maybe<int>);
        }
        public Maybe<int> consume_triggered(TouchGestures gesture)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].consume_trigger(gesture))
                {
                    clear_triggers();
                    return i;
                }
            }
            return default(Maybe<int>);
        }

        private void clear_triggers()
        {
            foreach (var node in Nodes)
                node.clear_triggers();
        }

        internal bool move_node(CardinalDirections dir)
        {
            if (ActiveNodeIndex == -1)
                return false;

            var destination = Destinations.destination(
                NodeOriginalLocations[ActiveNode], dir);
            // Vertical wrap
            if (destination.IsNothing && WrapVerticalSameColumn)
            {
                destination = move_vertical_wrap(dir, destination, true);
            }
            else if (destination.IsNothing && WrapVerticalMove)
            {
                destination = move_vertical_wrap(dir, destination);
            }
            if (destination.IsNothing)
                return false;

            set_active_node(node_from_location(destination));
            return true;
        }

        private Maybe<Vector2> move_vertical_wrap(
            CardinalDirections dir, Maybe<Vector2> destination, bool sameColumn = false)
        {
            // Get a subset of nodes to check for vertical wrapping
            var node_subset = !sameColumn ? NodeOriginalLocations :
                NodeOriginalLocations
                    .Where(x => x.Value.X == NodeOriginalLocations[ActiveNode].X)
                    .ToDictionary(p => p.Key, p => p.Value);

            if (dir == CardinalDirections.Up &&
                Global.Input.triggered(Inputs.Up, false))
            {

                // Must be the topmost node
                if (node_subset[ActiveNode].Y ==
                    node_subset.Min(x => x.Value.Y))
                {
                    // Get the bottommost node
                    var target = node_subset
                        .OrderByDescending(x => x.Value.Y)
                        .First();
                    if (target.Key != ActiveNode)
                        return target.Value;
                }
            }
            else if (dir == CardinalDirections.Down &&
                Global.Input.triggered(Inputs.Down, false))
            {
                // Must be the bottommost node
                if (node_subset[ActiveNode].Y ==
                    node_subset.Max(x => x.Value.Y))
                {
                    // Get the topmost node
                    var target = node_subset
                        .OrderBy(x => x.Value.Y)
                        .First();
                    if (target.Key != ActiveNode)
                        return target.Value;
                }
            }
            return Maybe<Vector2>.Nothing;
        }

        internal void MouseMove(T node)
        {
            if (this.ActiveNode != node)
            {
                if (SoundOnMouseMove)
                    Global.game_system.play_se(CursorMoveSound);
                set_active_node(node);
            }
        }
        internal void TouchMove(T node, TouchGestures gesture)
        {
            if (this.ActiveNode != node)
            {
                if (SoundOnMouseMove)
                    Global.game_system.play_se(CursorMoveSound);
                set_active_node(node);
            }
        }

        internal void set_active_node(T node)
        {
            if (this.ActiveNode != null)
                this.ActiveNode.Deactivate();

            if (node == null)
                ActiveNodeIndex = -1;
            else
            {
                ActiveNodeIndex = Nodes.IndexOf(node);
                this.ActiveNode.Activate();
            }
        }

        private T node_from_location(Vector2 loc)
        {
            return NodeOriginalLocations.First(x => x.Value == loc).Key;
        }

        internal bool Contains(T node)
        {
            return NodeOriginalLocations.ContainsKey(node);
        }

        public void Draw(
            SpriteBatch sprite_batch,
            Vector2 draw_offset = default(Vector2))
        {
#if DEBUG
            const bool PREVIEW_NODES = false;
            // Draw node connections
            if (PREVIEW_NODES) //Debug
            {
                draw_node_connections(sprite_batch, draw_offset);
            }
#endif
            foreach (var node in Nodes)
                node.Draw(sprite_batch, draw_offset);
#if DEBUG
            // Draw hitboxes
            if (PREVIEW_NODES) //Debug
            {
                DrawNodeHitboxes(sprite_batch, draw_offset);
            }
#endif
        }

#if DEBUG
        internal void draw_node_connections(
            SpriteBatch sprite_batch,
            Vector2 draw_offset = default(Vector2))
        {
            foreach (var node in Nodes)
            {
                foreach (CardinalDirections dir in new List<CardinalDirections> {
                        CardinalDirections.Down,
                        CardinalDirections.Left,
                        CardinalDirections.Right,
                        CardinalDirections.Up })
                {
                    var destination = Destinations.destination(
                        NodeOriginalLocations[node], dir);
                    if (destination.IsSomething)
                    {
                        Color c =
                            (dir == CardinalDirections.Left ||
                            dir == CardinalDirections.Right) ?
                                new Color(0.75f, 0, 0.75f, 0.75f) :
                                new Color(0, 0.75f, 0, 0.75f);

                        var dest_node = node_from_location(destination);
                        Vector2 origin = new Vector2(
                            node_location(node).X,
                            node.loc.Y + node.Size.Y / 2f);
                        Vector2 target = new Vector2(
                            node_location(dest_node).X,
                            dest_node.loc.Y + dest_node.Size.Y / 2f);

                        float angle = (float)Math.Atan2(
                            target.Y - origin.Y, target.X - origin.X);
                        float dist = Math.Abs((origin - target).Length());

                        sprite_batch.Draw(
                            Global.Content.Load<Texture2D>(@"Graphics/White_Square"),
                            origin - draw_offset, null, c, angle,
                            new Vector2(0, 8), new Vector2(dist / 16f, 2 / 16f),
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        internal void DrawNodeHitboxes(
            SpriteBatch spriteBatch,
            Vector2 drawOffset = default(Vector2))
        {
            foreach (var node in Nodes)
            {
                node.DrawHitbox(spriteBatch, drawOffset);
            }
        }
#endif
    }
}
