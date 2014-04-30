fotm
====

World of Warcraft arena FotM monitor.

TODO

ASAP: -

Frontend:
- Show loading spin until SignalR is connected
- Switch to Angular.js or otherwise implement the correct urls altering for this SPA, i.e. when user applies some filter the address should change accordingly so that he can share the result
- When Playing now is empty show the time of latest update
- Create admin push mechanism
- Create admin method polling number of current connected users

Infrastructure:
- Setup easy deployment for website to US/EU
- Setup easy deployment for scanner to US/EU
- Remember teams, add win/loss ratio to each team
- Divide ratings update per number of games if diff is more than 1 to avoid showing 30+ rating changes

Machine learning:
- Add one team grouping with player outside of first 1000 on each turn
- Add super tests for 2v2/5v5/rbg
- Try evaluating all n choose 3 combinations instead of multiple k-means runs

Backlog:
- Add AWS SNS/SQS
- Refactoring!
- Make FotM.Config rebuild on each update to Regional.config
- Investigate compressing all SignalR communication
- Discussion forum per team setup?
- Add CN region
