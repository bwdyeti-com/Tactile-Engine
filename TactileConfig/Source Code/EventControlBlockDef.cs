using System.Collections.Generic;

namespace Tactile.Events
{
    /// <summary>
    /// Defines the event control keys that make up a block of event code,
    /// such as the start and end of an if block, and the else and elseif
    /// statements mid-block.
    /// </summary>
    public struct EventControlBlockDef
    {
        public readonly static List<EventControlBlockDef> EVENT_BLOCKS =
            new List<EventControlBlockDef>
        {
            // If/Else
            new EventControlBlockDef(201, 204, 202, 203, 205),
            // Skip
            new EventControlBlockDef(211, 213, 212),
            new EventControlBlockDef(214, 215),
        };

        public int StartControlId { get; private set; }
        public int EndControlId { get; private set; }
        private HashSet<int> IntermediateControlIds;

        /// <summary></summary>
        /// <param name="start">The control code key for the start of the block.</param>
        /// <param name="end">The control code key for the end of the block.</param>
        /// <param name="intermediate">
        /// Other control codes that can apprear in
        /// the middle of the block to break it up,
        /// like else and elseif in an if block.
        /// </param>
        private EventControlBlockDef(int start, int end, params int[] intermediate)
        {
            StartControlId = start;
            EndControlId = end;
            IntermediateControlIds = new HashSet<int>(intermediate);
        }

        public bool HasIntermediateId(int id)
        {
            return IntermediateControlIds.Contains(id);
        }
    }
}
