using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaepora
{
    public class TrackedMessage
    {
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}