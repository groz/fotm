// Code-behind viewmodel for /Home/Index action

function ArmoryViewModel(region, data, media) {
    var self = this;
    
    function virtualPageView(virtualPage) {
        console.log(virtualPage);
        ga('send', 'pageview', virtualPage);
    }
    
    self.bracket = ko.observable("3v3");
    self.region = ko.observable(region);
    
    self.US = ko.computed(function() {
        return self.region() == "US";
    });
    
    self.EU = ko.computed(function () {
        return self.region() == "EU";
    });
    
    self.leaderboardSelected = ko.observable(true);
    self.playingNowSelected = ko.observable(false);

    self.model = ko.observable({
        AllTimeLeaders: ko.observable(),
        PlayingNow: ko.observable(),
        TeamSetupsViewModels: ko.observable(),
    });

    self.selectedPage = ko.computed(function () {
        if (self.leaderboardSelected()) {
            return self.model().AllTimeLeaders();
        } else {
            return self.model().PlayingNow();
        }
    });

    self.update = function(msg) {
        self.model().AllTimeLeaders(msg.AllTimeLeaders);
        self.model().PlayingNow(msg.PlayingNow);
        self.model().TeamSetupsViewModels(msg.TeamSetupsViewModels);
    };

    self.selectedSetup = ko.observable({});

    self.showTeams = function (setup) {
        self.showFotMHint(false);
        self.selectedSetup(setup);

        var virtualPage = '/fotm?rank=' + setup.Rank;
        virtualPageView(virtualPage);
    };

    self.showFotMHint = ko.observable(true);

    if ((typeof data === "undefined") || (data === null)) {
        console.log("Initialization data was not provided. Waiting for updates...");
    } else {
        console.log("Initializing with", data);
        self.update(data);
    }

    self.playingNowClicked = function () {
        if (!self.playingNowSelected()) {
            self.leaderboardSelected(false);
            self.playingNowSelected(true);
            virtualPageView("/now");
        }
    };
    
    self.leaderboardClicked = function () {
        if (!self.leaderboardSelected()) {
            self.leaderboardSelected(true);
            self.playingNowSelected(false);
            virtualPageView("/leaderboard");
        }
    };

    self.toFactionImage = function (factionId) {
        return media.FactionImages[factionId];
    };
    
    self.toClassImage = function (classId) {
        return media.ClassImages[classId];
    };
    
    self.toSpecImage = function (specId) {
        return media.SpecImages[specId];
    };
    
    self.toRaceImage = function (raceId) {
        return media.RaceImages[raceId];
    };

    self.toLocal = function(utcTime) {
        var d = new Date(utcTime + " UTC");
        
        if (self.leaderboardSelected()) {
            return d.toLocaleDateString() + " " + d.toLocaleTimeString();
        } else {
            return d.toLocaleTimeString();
        }
    };

    self.isSetupSelected = function (teamSetup) {
        return self.selectedSetup() == teamSetup;
    };
    
    function specsToClasses() {
        var result = [];
        var dictionary = media.SpecsToClasses;

        for (var key in dictionary) {
            if (dictionary.hasOwnProperty(key)) {
                result.push({
                    specId: key,
                    classId: dictionary[key]
                });
            }
        }

        return result;
    }

    // not proud. TODO: refactor the section below
    self.allSpecs = ko.observableArray(specsToClasses());
    
    self.classFilters = ko.observable([null, null, null]);

    self.addFilter = function (filterIndex, spec) {
        var idx = filterIndex();
        console.log(idx, spec);
        self.classFilters()[idx] = spec;
        
        self.filterViews[idx](
            createHtmlForSpec(spec)
        );
    };

    var emptySpecHtml = "<span>All</span>";

    function createHtmlForSpec(spec) {
        if (spec != null) {
            return '<img src="' + self.toClassImage(spec.classId) + '" alt="ClassImage" />&nbsp;' +
                '<img src="' + self.toSpecImage(spec.specId) + '" alt="SpecImage" />';
        } else {
            return emptySpecHtml;
        }
    }

    self.filterViews = [
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml)
    ];
        

    self.filterView = function (d) {
        var idx = d();
        return self.filterViews[idx];
    };
}

function initializePage(region, data, media) {
    console.log(media);
    
    var armory = new ArmoryViewModel(region, data, media);
    ko.applyBindings(armory);

    var hub = $.connection.indexHub;

    hub.client.update = function (msg) {
        console.log("Msg received", msg);
        armory.update(msg);
    };

    $.connection.hub.start().done(function () {
        hub.server.queryLatestUpdate();
    });
}