using FotM.Domain;

namespace FotM.Portal.Infrastructure
{
    public class TeamInfo
    {
        public Team Team { get; set; }
        public TeamSetup Setup { get; set; }
        public TeamStats Stats { get; set; }
    }
}