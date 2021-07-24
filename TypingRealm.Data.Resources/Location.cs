using System.Collections.Generic;

namespace TypingRealm.Data.Resources
{
#pragma warning disable CS8618
    public class Location
    {
        public string LocationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanHaveRopeWar { get; set; }
        public List<string> LocationEntrances { get; set; } = new List<string>();
    }
#pragma warning restore CS8618
}
