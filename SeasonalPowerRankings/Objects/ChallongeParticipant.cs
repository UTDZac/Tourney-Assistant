using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Objects
{
    public class ChallongeParticipant
    {
        public ChallongeParticipant()
        {
            Id = -1;
            Name = string.Empty;
            Email = string.Empty;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
