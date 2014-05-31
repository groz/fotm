FotM - F# and AngularJS realtime World of Warcraft arena setups and rankings monitor.
====


TODO

Frontend:
- Add buttons to clear setup filter
- Submit sitemap to Google webmaster tools
- Add Discourse discussion forum

Infrastructure:
- Add redirects from old domains (i.e. fotm.eu -> fotm.info/#/eu/3v3)
- Add metrics infrastructure

Machine learning:
- Model realm as binary (feature1: is user from Spirestone or not, feature 2: is user from Al'Akir or not?)
- Add super tests for F#
  - 2v2
  - 3v3
  - 5v5
  - rbg

Backlog:
- Discussion forum per team setup?
- Add AWS SNS/SQS failover
- Create admin push mechanism
- Change Athena backfill mechanism to work with 1 item (i.e. store state on each step and load it on init)
- Remove V1(C#) code.
