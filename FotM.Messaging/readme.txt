Workflows:

Entities involved:
Portal (website)
ArmoryScanner (console application)

Service bus entities:
query-latest-stats-queue
stats-update-topic

Messages:
QueryLatestStatsMessage(requester)
StatsUpdateMessage

========================================

I. On new Portal initialization
Portal creates private queue with name Portal.Host
Portal starts listening to that queue (here it should abandon the request and listening if it already got update in *2*)
Portal sends QueryLatestStatsMessage(Portal.Host) -> query-latest-stats-queue -> ArmoryScanner

ArmoryScanner is listening to query-latest-stats-queue
ArmoryScanner receives QueryLatestStatsMessage(privateQueue) message from query-latest-stats-queue, 
	if ArmoryScanner has any data it sends StatsUpdateMessage(data) to given privateQueue

II. Usual workflow
Portal is listening to stats-update-topic
if ArmoryScanner scans any data it sends StatsUpdateMessage(data) to stats-update-topic

III. ToDo: scanners sync