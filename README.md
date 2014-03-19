fotm
====

World of Warcraft arena FotM monitor.

TODO

I. Refactor the horrible code made while trying to create minimum viable product

II. Infrastructure:
- Delete expired (1 week?) entries from leaderboard
- Add correct setup per region
- Deploy per region
- Set smaller expiration date for teams that were seen only once
- Divide ratings updatet per number of games if diff more than 1
- Google analytics

Backlog:
- Refactor Messaging
- Add AWS SNS/SQS

III. Machine learning:
- Introduce some team jumping in test data
- Compute several clusterings and rank them against each other based on following rules: 
  - Lower score for teams with double/triple healers
  - Lower score for teams with class stacking
