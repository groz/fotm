fotm
====

World of Warcraft arena FotM monitor.

TODO

Frontend:
- Switch to Angular.js or otherwise implement the correct urls for my SPA
- When Playing now is empty show the time of latest update
- Create admin push mechanism
- Create admin method polling number of current connected users

Infrastructure:
- Setup easy deployment for website to US/EU
- Setup easy deployment for scanner to US/EU
- Remember teams, add win/loss ratio to each team
- Divide ratings update per number of games if diff is more than 1 to avoid showing 30+ rating changes

Machine learning:

Backlog:
- Add AWS SNS/SQS
- Refactoring!
- Make FotM.Config rebuild on each update to Regional.config
- Investigate compressing all SignalR communication
