# CoffeeScript

class ArmoryViewModel
    bracketSize = 3
    hub = $.connection.indexHub
    emptyClassHtml = "<span>All</span>"
    emptySpecHtml = "<span>&nbsp;</span>"    
    filterClassViews = (ko.observable(emptyClassHtml) for i in [0..bracketSize-1])
    filterSpecViews = (ko.observable(emptySpecHtml) for i in [0..bracketSize-1])
    latestSetupRequestGuid = null
    latestFilterRequestGuid = null
    
    constructor: (region, armory, @media, isLeaderboardSelected) ->
        console.log armory
        @EU = region == 'EU'
        @US = region == 'US'
        @KR = region == 'KR'
        @leaderboardSelected = ko.observable isLeaderboardSelected
        @playingNowSelected = ko.computed (=> !@leaderboardSelected())
        @playingNowTeams = ko.observable armory.PlayingNow
        @setupFilters = ko.observable(
            (new SetupFilter(null, null) for i in [0..bracketSize-1])
        )
        @fotmSetups = ko.observable armory.TeamSetupsViewModels
        @selectedSetup = ko.observable null
        @fotmTeams = ko.observable armory.AllTimeLeaders
        
        @allClasses = @media.allClasses
        @possibleSpecs = (ko.observableArray() for i in [0..bracketSize-1])
        @serverActionsDisabled = ko.observable true
        
        if !armory
            console.log "Initialization data was not provided. Waiting for updates..."
        else
            console.log "Initializing with", armory
        
        hub.client.updateAll = (msg) =>
            console.log "updateAll msg received:", msg
            @fotmTeams msg.AllTimeLeaders
            @playingNowTeams msg.PlayingNow
            @fotmSetups msg.TeamSetupsViewModels
            
        hub.client.updateNow = (teams) =>
            console.log "updateNow msg received:", teams
            @playingNowTeams teams
                
        hub.client.showSetupTeams = (requestGuid, teams) =>
            console.log "Teams for queried setup received for request #{requestGuid}"
            if requestGuid == latestSetupRequestGuid
                @fotmTeams teams
            else
                console.log "Response to request #{requestGuid} is outdated and discarded."
    
        hub.client.showFilteredSetups = (requestGuid, setups, teams) =>
            console.log "Filter response received for request guid #{requestGuid}"
            if requestGuid == latestFilterRequestGuid
                @fotmSetups setups
                @fotmTeams teams
            else
                console.log "Response to request #{requestGuid} is outdated and discarded."

        $.connection.hub.start().done =>
            console.log "connected"
            hub.server.queryLatestUpdate()
            @serverActionsDisabled null

    virtualPageView: (virtualPage) ->
        console.log virtualPage
        ga 'send', 'pageview', virtualPage
                
    leaderboardClicked: =>
        if !@leaderboardSelected()
            @leaderboardSelected true
            @virtualPageView "/leaderboard"
            
    playingNowClicked: =>
        if !@playingNowSelected()
            @leaderboardSelected false
            @virtualPageView "/now"

    isSetupSelected: (teamSetup) -> 
        if !@selectedSetup()
            return false
            
        for i in [0..bracketSize-1]
            if @selectedSetup().Specs[i] != teamSetup.Specs[i]
                return false
                
        return true
        
    filterClassView: ($i) -> filterClassViews[$i()]
    filterSpecView: ($i) -> filterSpecViews[$i()]        
    specsFor: ($i) -> @possibleSpecs[$i()]()
        
    toLocal: (utcTime) -> asLocalTime(utcTime)
        
    updateClassFilter: ($i, classId) =>
        idx = $i()
        console.log idx, classId
        
        @fotmTeams []

        setupFilter = @setupFilters()
        setupFilter[idx].classId = classId;
        setupFilter[idx].specId = null;

        possibleSpecs = @media.getSpecsForClass setupFilter[idx].classId
        console.log "Setting possible specs", idx, "to", possibleSpecs
        @possibleSpecs[idx] possibleSpecs

        console.log "Sending filtering request for", setupFilter

        filterClassViews[idx] @createHtmlForClass(classId)        
        filterSpecViews[idx] @createHtmlForSpec(null)

        @queryServerForFilteredSetups setupFilter

    updateSpecFilter: ($i, spec) =>
        idx = $i()
        specId = if spec then spec.specId else null
        console.log idx, specId

        @fotmTeams []
        setupFilter = @setupFilters()

        setupFilter[idx].specId = specId

        console.log "Sending filtering request for", setupFilter

        filterSpecViews[idx] @createHtmlForSpec(specId)
        
        @queryServerForFilteredSetups setupFilter
        
    createHtmlForSpec: (specId) =>
        if specId
            "<img src=\"#{@media.toSpecImage(specId)}\" alt=\"SpecImage\" />"
        else
            emptySpecHtml

    createHtmlForClass: (classId) =>
        if classId
            "<img src=\"#{@media.toClassImage(classId)}\" alt=\"ClassImage\" />"
        else
            emptyClassHtml
            
    showTeams: (setup) =>
        if !@serverActionsDisabled()
            if setup == @selectedSetup()
                @selectedSetup null
                console.log "Cancelled setup selection"
                @queryServerForFilteredSetups @setupFilters()
            else
                @virtualPageView "/fotm?rank=#{setup.Rank}"
                @selectedSetup setup
                @queryServerForSetup setup

    queryServerForSetup: (setup) ->
        requestGuid = genGuid()
        latestSetupRequestGuid = requestGuid
        hub.server.queryTeamsForSetup requestGuid, setup

    queryServerForFilteredSetups: (setupFilter) ->
        @virtualPageView "/filter"
        requestGuid = genGuid()
        latestFilterRequestGuid = requestGuid
        hub.server.queryFilteredSetups requestGuid, setupFilter

class @Main
    constructor: (region, regionEndPoint, armory, mediaData) -> 
        armory = armory or { TeamSetupsViewModels: {}, PlayingNow: [], AllTimeLeaders: {} }

        media = new Media(regionEndPoint, mediaData)
        armoryViewModel = new ArmoryViewModel(region, armory, media, true)
        ko.applyBindings armoryViewModel
        
class Media
    constructor: (@regionEndPoint, @data) ->
        @allClasses = []
        for s, c of @data.SpecsToClasses
            if @allClasses.indexOf(c) == -1
                @allClasses.push c
    
    toFactionImage: (factionId) => @data.FactionImages[factionId]    
    toClassImage: (classId) => @data.ClassImages[classId]
    toSpecImage: (specId) => @data.SpecImages[specId]    
    toRaceImage: (raceId) => @data.RaceImages[raceId]
    
    armoryLink: (data) =>
        "#{@regionEndPoint}wow/en/character/#{data.RealmSlug}/#{data.Name}/simple"
    
    getSpecsForClass: (classId) =>
        result = []
        for s, c of @data.SpecsToClasses
            if c == classId
                result.push new SetupFilter(c, s)
        result

class SetupFilter
    constructor: (@classId, @specId) ->