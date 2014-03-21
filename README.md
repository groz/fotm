fotm
====

World of Warcraft arena FotM monitor.

TODO

Frontend:
- Icon for the site
- Move generation of image links from server viewmodel to js client view objects
- Add NavBar for FotM section to choose fotms by class, not spec
- Show fotm setups for current playing teams if appropriate section is selected
- Add armory links to players


Infrastructure:
- Add proper deployment configuration per region and setup queues/websites for US & KR
  - Test it deploying for US
- Delete expired (1 week?) entries from leaderboard
- Add correct setup per region
- Set smaller expiration date for teams that were seen only a few times
- Divide ratings update per number of games if diff is more than 1 to avoid showing 30+ rating changes
- Google analytics


Machine learning:
- Introduce some team jumping in test data
- Compute several clusterings and rank them against each other based on following rules: 
  - Lower score for teams with double/triple healers
  - Lower score for teams with class stacking


Backlog:
- Refactor Messaging
- Add AWS SNS/SQS
- Refactor the horrible code made while trying to create minimum viable product
