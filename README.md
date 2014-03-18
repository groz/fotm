fotm
====

World of Warcraft arena FotM monitor.

TODO

Infrastructure:
- Delete expired (1 week?) entries from leaderboard
- Add correct setup per region
- Deploy per region
- Set smaller expiration date for teams that were seen only once
- Divide ratings updatet per number of games if diff more than 1
- Google analytics

Backlog:
- Refactor Messaging
- Add AWS SNS/SQS

Machine learning:
- Create separate SuperTest for the case when all playing teams are winning/losing in the same turn
- Create separate SuperTests per KMeans algorithm, make algorithm optionally injectable to Cassandra
- Apply mean normalization/feature scaling
- Learn feature weights for k-means for normalized features on generated training set
