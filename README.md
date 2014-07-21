FotM
====
F# and AngularJS realtime World of Warcraft arena setups and rankings monitor.

TODO

Chat:
- Show list of users in chat
- Add admin differentiation
- Filter spammers
  - Do not allow duplicate messages
  - Do not allow too quick successive messages

Frontend:
- Add buttons to clear setup filter
- Add Discourse discussion forum
- Make region buttons visible in mobile UI

Infrastructure:
- Add unit tests for Athena

Machine learning:
- Model realm as binary (feature1: is user from Spirestone or not, feature 2: is user from Al'Akir or not?)
- Add super tests for F#
  - 2v2
  - 3v3
  - 5v5
  - rbg
- Try choosing clustering by total setup popularity score

Backlog:
- Discussion forum per team setup?
- Add AWS SNS/SQS failover
- Create admin push mechanism
- Add metrics infrastructure
