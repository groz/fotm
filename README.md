fotm
====

World of Warcraft arena FotM monitor.

TODO

Frontend:
- Move links to brackets from submenus to page itself
- Add buttons to clear setup filter
- Add notification of new website to old domains

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
