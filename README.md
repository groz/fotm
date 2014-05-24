fotm
====

World of Warcraft arena FotM monitor.

TODO

Frontend:
- Align social buttons
- Show win ratio for teams (possibly setups)
- Add Google Analytics virtual pages
- Brackets:
  - 2v2, take most stuff from 3v3 and check on lower sizes
  - 5v5, remove names?
  - rbg, remove names, classes, redesign filters?

Infrastructure:
- Show teams with > 1 wins and > 1 games per day in Leaderboard and > 1 games in Playing Now.
- Backfill Athena and Apollo on startup from the latest update in storage
- Deploy Avatars
- Deploy Apollo
- Add redirects from old domains (i.e. fotm.eu -> fotm.info/#/eu/3v3)

Machine learning:
- Model realm as binary (feature1: is user from Spirestone or not, feature 2: is user from Al'Akir or not?)
- Add super tests for 2v2/5v5/rbg

Backlog:
- Discussion forum per team setup?
- Add AWS SNS/SQS failover
- Create admin push mechanism
