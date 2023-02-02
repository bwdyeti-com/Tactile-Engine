using System.Collections.Generic;
using System.Linq;
using TactileLibrary;

namespace Tactile.Events
{
    /// <summary>
    /// Defines an event control block.
    /// This can be determined from a <see cref="List{T}"/> of <see cref="Event_Control"/>s
    /// using <see cref="GetEventBlocks(List{Event_Control}, int, int)"/>,
    /// and contains the indices into that event data for the
    /// start and end of the block, and any intermediate lines.
    /// </summary>
    public struct EventControlBlock
    {
        const int COMMENT_KEY = 102;

        public int StartControlIndex { get; private set; }
        public int EndControlIndex { get; private set; }
        public List<int> IntermediateControlIndices
        {
            get { return _IntermediateControlIndices.OrderBy(x => x).ToList(); }
        }
        private HashSet<int> _IntermediateControlIndices;

        private EventControlBlock(int start, int end, params int[] other)
        {
            StartControlIndex = start;
            EndControlIndex = end;
            _IntermediateControlIndices = new HashSet<int>(other);
        }

        public bool HasIntermediateIndex(int id)
        {
            return IntermediateControlIndices.Contains(id);
        }

        public override string ToString()
        {
            return string.Format("EventControlBlock: {0}-{1}",
                StartControlIndex,
                EndControlIndex >= 0 ? EndControlIndex.ToString() : "(end missing)");
        }

        /// <summary>
        /// Processes a list of event controls to get the
        /// event control blocks it contains.
        /// </summary>
        public static List<EventControlBlock> GetEventBlocks(
            List<Event_Control> eventData,
            int indexStart,
            int indexEnd)
        {
            var blocks = new List<EventControlBlock>();
            // Get the start and end of each block
            for (int i = indexStart; i <= indexEnd; i++)
            {
                int key = eventData[i].Key;
                // When a block start is found
                if (EventControlBlockDef.EVENT_BLOCKS.Any(x => x.StartControlId == key))
                {
                    var block = FindBlock(eventData, i, indexEnd);

                    blocks.Add(block);
                }
            }

            return blocks;
        }

        /// <summary>
        /// Gets an event block, starting from the first line of the block.
        /// Attempts to find an end line, if one exists.
        /// Requires any nested blocks to be complete and will not check inside them for partners.
        /// </summary>
        private static EventControlBlock FindBlock(
            List<Event_Control> eventData,
            int index,
            int indexEnd)
        {
            int key = eventData[index].Key;
            int start = index;
            int end = -1;
            List<int> intermediates = new List<int>();

            var blockType = EventControlBlockDef.EVENT_BLOCKS.First(x => x.StartControlId == key);
            var otherBlocks = new HashSet<EventControlBlockDef>(
                EventControlBlockDef.EVENT_BLOCKS.Where(x => x.StartControlId != key));

            // Loop through the remaining controls
            int nested = 0;

            for (int j = index + 1; j <= indexEnd; j++)
            {
                int nextKey = eventData[j].Key;
                // Check if this is the start of some other kind of block
                if (otherBlocks.Any(x => nextKey == x.StartControlId))
                {
                    var otherBlock = FindBlock(eventData, j, indexEnd);
                    // Went past the end of the event
                    if (otherBlock.EndControlIndex == -1)
                        return new EventControlBlock(start, -1);
                    else
                    {
                        j = otherBlock.EndControlIndex;
                        continue;
                    }
                }

                if (nextKey == blockType.StartControlId)
                    // If another of the same type of block is found, count it as nested
                    nested++;
                else if (blockType.HasIntermediateId(nextKey) && nested == 0)
                    intermediates.Add(j);
                else if (nextKey == blockType.EndControlId)
                {
                    if (nested == 0)
                    {
                        end = j;
                        break;
                    }
                    else
                        // Leaving a nested block
                        nested--;
                }
            }

            return new EventControlBlock(start, end, intermediates.ToArray());
        }

        /// <summary>
        /// Processes a list of event controls to get the
        /// indentation of each line of the event.
        /// </summary>
        public static List<int> GetIndentation(
            List<Event_Control> eventData)
        {
            List<EventControlBlock> blocks;
            return GetIndentation(eventData, out blocks);
        }
        /// <summary>
        /// Processes a list of event controls to get the
        /// indentation of each line of the event.
        /// Also outputs the <see cref="EventControlBlock"/>s found during processing.
        /// </summary>
        public static List<int> GetIndentation(
            List<Event_Control> eventData,
            out List<EventControlBlock> blocks)
        {
            int indexStart = 0;
            int indexEnd = eventData.Count - 1;

            blocks = Tactile.Events.EventControlBlock.GetEventBlocks(
                eventData, indexStart, indexEnd);

            return GetIndentation(eventData, indexEnd, blocks);
        }

        /// <summary>
        /// Processes a list of event controls to get the
        /// indentation of each line of the event,
        /// using the provided <see cref="EventControlBlock"/>s.
        /// </summary>
        public static List<int> GetIndentation(
            List<Event_Control> eventData,
            int indexEnd,
            List<EventControlBlock> blocks)
        {
            List<int> indentation = new List<int>();
            var intermediates = new HashSet<int>(blocks
                .SelectMany(x => x.IntermediateControlIndices));
            // Get the indentation of each line based on each block
            for (int i = 0; i <= indexEnd; i++)
            {
                int indent = 0;

                int key = eventData[i].Key;
                // If a comment
                if (key == COMMENT_KEY)
                {
                    // Check for the next lines being a comment or intermediate
                    for (int j = i + 1; j <= indexEnd; j++)
                    {
                        int nextKey = eventData[j].Key;
                        // If not another comment
                        if (nextKey != COMMENT_KEY)
                        {
                            if (intermediates.Contains(j))
                                indent--;
                            // Break regardless of intermediate or not if not chaining comments
                            break;
                        }
                    }
                }
                for (int j = 0; j < blocks.Count; j++)
                {
                    var block = blocks[j];
                    if (i > block.StartControlIndex)
                    {
                        // Not an intermediate control for the block (else, elsif, etc)
                        // and before the end of the block or the block doesn't end
                        if (!block.HasIntermediateIndex(i) &&
                                (i < block.EndControlIndex || block.EndControlIndex == -1))
                            indent++;
                    }
                }
                indentation.Add(indent);
            }
            
            return indentation;
        }
    }
}
