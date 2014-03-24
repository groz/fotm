fotm
====

World of Warcraft arena FotM monitor.

TODO

Feature requests from the users:
- Search fotm comps per spec
- More teams than top 20
- Click on the player to open armory
- Publish period for when the data was collected somewhere on the page

Frontend:
- When Playing now is empty show the time of latest update
- Create admin push mechanism
- Create admin method polling number of current connected users
- Add NavBar for FotM section to choose fotms by class, not spec
- Show fotm setups for current playing teams if appropriate section is selected
- Add armory links to players
- Shorten period for playing now

Infrastructure:
- Setup easy deployment for website to US/EU
- Setup easy deployment for scanner to US/EU
- Add website for KR
- Delete expired (1 game per day?) entries from leaderboard
- Remember teams, add win/loss ratio to each team
- Set smaller expiration date for teams that were seen only a few times
- Divide ratings update per number of games if diff is more than 1 to avoid showing 30+ rating changes

Backlog:
- Add AWS SNS/SQS
- Refactor the horrible code made while trying to create minimum viable product
- Make FotM.Config rebuild on each update to Regional.config
- Investigate compressing all SignalR communication
