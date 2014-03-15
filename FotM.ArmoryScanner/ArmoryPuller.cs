using System;
using System.Linq;
using FotM.Domain;

namespace FotM.ArmoryScanner
{
    public class ArmoryPuller
    {
        private readonly Uri _baseAddress;
        public string Host { get; private set; }

        public ArmoryPuller(string host, string locale = Locale.EnUs)
        {
            Host = host;

            var builder = new UriBuilder(Uri.UriSchemeHttp, host);
            builder.Path = "api/wow";
            _baseAddress = builder.Uri;
        }

        public Leaderboard DownloadLeaderboard(Bracket bracket, string locale = Locale.EnUs)
        {
            string bracketString = null;

            switch (bracket)
            {
                case Bracket.Twos:
                    bracketString = "2v2";
                    break;
                case Bracket.Threes:
                    bracketString = "3v3";
                    break;
                case Bracket.Fives:
                    bracketString = "5v5";
                    break;
                case Bracket.Rbg:
                    bracketString = "rbg";
                    break;
            }

            if (string.IsNullOrEmpty(bracketString))
            {
                throw new ArgumentException("Bracket not found");
            }

            var uriBuilder = new UriBuilder(_baseAddress);
            uriBuilder.Path += "/leaderboard/" + bracketString;
            uriBuilder.Query = "locale=" + locale;
            var rawJsonPuller = new RawJsonPuller(uriBuilder.Uri);

            // making results consistent as Blizzard randomly tosses around players with same rating
            var leaderboard = rawJsonPuller.DownloadJson<Leaderboard>();
            leaderboard.Bracket = bracket;

            leaderboard.Rows = leaderboard.Rows
                .OrderByDescending(r => r.Rating)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.RealmId)
                .ToArray();

            for (int i = 0; i < leaderboard.Rows.Length; ++i)
            {
                leaderboard.Rows[i].Ranking = i;
            }

            return leaderboard;
        }
    }
}