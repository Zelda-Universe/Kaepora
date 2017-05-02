using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaepora
{
    public class Segment
    {
        public string Key { get; set; }
        public int Priority { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ProfileSegment> Profiles { get; set; } = new List<ProfileSegment>();
    }

    public class Profile
    {
        public string Key { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProfileSegment> Segments { get; set; } = new List<ProfileSegment>();

        public override string ToString()
        {
            var segments = Segments.Select(x => x.Segment).Where(x => x.IsActive)
                                    .OrderByDescending(x => x.Priority)
                                    .Select(x => x.Content);

            return String.Join("\n\n", segments);
        }
    }

    public class ProfileSegment
    {
        public Segment Segment { get; set; }
        public Profile Profile { get; set; }
    }
}