using FotM.Domain;

namespace FotM.ArmoryScanner
{
    public interface IArmoryPuller
    {
        Leaderboard DownloadLeaderboard(Bracket bracket, string locale = Locale.EnUs);
    }
}