using System;
using System.Collections.Generic;

namespace Tactile.Constants
{
    public class Actor
    {
        public const int MAX_ACTOR_COUNT = short.MaxValue / 2; // Non-generic Ids must <= this value; generics start counting after this
    }
}
