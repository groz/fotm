namespace FotM.Cassandra
{
    public class CassandraStats
    {
        public CassandraStats()
        {
            TotalCalls = 0;
            TeamsPossible = 0;
            FullTeamsDetected = 0;
            IncompleteTeams = 0;
            OverbookedTeams = 0;
        }

        public int TotalCalls { get; set; }
        public int TeamsPossible { get; set; }
        public int FullTeamsDetected { get; set; }
        public int IncompleteTeams { get; set; }
        public int OverbookedTeams { get; set; }

        // calculated
        public double IncompleteTeamsPercent
        {
            get { return 100.0 * IncompleteTeams / TeamsPossible; }
        }

        public double OverbookedTeamsPercent
        {
            get { return 100.0 * OverbookedTeams / TeamsPossible; }
        }

        public double FullTeamsPercent
        {
            get { return 100.0 * FullTeamsDetected / TeamsPossible; }
        }

        public override string ToString()
        {
            return string.Format("Total calls: {0}, teams possible: {1}, full teams: {2:F1}%, incomplete teams: {3:F1}%, Overbooked teams: {4:F1}%",
                TotalCalls, TeamsPossible, FullTeamsPercent, IncompleteTeamsPercent, OverbookedTeamsPercent);
        }
    }
}