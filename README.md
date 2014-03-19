fotm
====

World of Warcraft arena FotM monitor.

TODO

I. Refactor the horrible code made while trying to create minimum viable product

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
- Setup different predictor injections into CassandraSuperTest and simultaneous run for all of them
- Simulate data per week
- Introduce some team jumping in test data
- Learn feature weights for k-means for normalized features on generated training set
