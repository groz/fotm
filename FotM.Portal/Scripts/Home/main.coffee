# CoffeeScript

class Team
    constructor: (@rank, @players, @rating, @ratingChange, @dateSeen, @faction) ->

class Filter
    constructor: (@specs) ->
        
class Setup
    constructor: (@rank, @specs, @popularity) ->

class Player
    constructor: (@name, @realm, @spec, @faction, @race) ->
    
class Spec
    constructor: (@classId, @specId) ->
    toString: ->
        "Spec: #{@classId}, #{@specId}"

class @PlayingNow
    constructor: (@teams) ->
    
class @Leaderboard
    constructor: (@teams, @setups, @filters = [], currentSetup = null) ->
        @currentSetup = ko.observable(currentSetup)

class @Main
    constructor: -> 
        console.log "initializing main object..."    
        
        @computedValue = ko.computed => 
            @value()[0] + @value()[1] + @value()[2]
        
    startLeaderboardPage: (@leaderboard) => @start()        
    startPlayingNowPage: (@playingNow) => @start()    
    
    value: ko.observableArray([1, 2, 3])
    
    start: () => 
        console.log "starting interactive workflows..."
        
        console.log @computedValue()
        @value([10, 20, 30])
        console.log @computedValue()        
        
        @hub = $.connection.indexHub

