using System.Collections.Generic;
using System.Linq;
using FotM.Utilities;
using log4net;

namespace FotM.Domain
{
    public class PlayerRegistry
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<PlayerRegistry>();

        private readonly HashSet<Player> _players;

        public PlayerRegistry()
        {
            _players = new HashSet<Player>();
        }

        public void Update(Leaderboard snapshot)
        {
            var newPlayers = new HashSet<Player>(snapshot.Rows.Select(GetPlayer));
            _players.Clear();
            _players.UnionWith(newPlayers);
        }

        public static Player CreatePlayerFrom(LeaderboardEntry entry)
        {
            return new Player
            {
                Name = entry.Name,
                Realm = new Realm()
                {
                    RealmId = entry.RealmId,
                    RealmName = entry.RealmName,
                    RealmSlug = entry.RealmSlug
                }
            };
        }

        public Player GetPlayer(LeaderboardEntry entry)
        {
            var player = CreatePlayerFrom(entry);

            if (_players.Add(player))
            {
                Logger.InfoFormat("New player added to registry: {0}. Total players: {1}", player, _players.Count);
            }

            return player;
        }
    }
}