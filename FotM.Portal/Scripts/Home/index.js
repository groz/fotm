// Code-behind viewmodel for /Home/Index action

function ArmoryViewModel(region, data, media, un) {
    var self = this;

    self.hub = null;
    
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
    
    self.selectedSetup = ko.observable(null);
    self.fotmTeams = ko.observable({});
    self.fotmSetups = ko.observable(self.model().TeamSetupsViewModels);

    self.showTeams = function (setup) {
        self.showFotMHint(false);
        self.selectedSetup(setup);

        var virtualPage = '/fotm?rank=' + setup.Rank;
        virtualPageView(virtualPage);

        if (self.hub != null) {
            self.hub.server.queryTeamsForSetup(setup);
        }
    };

    function exists(data) {
        return !((typeof data === 'undefined') || (data === null));
    }

    self.showFotMHint = ko.observable(true);

    if (!exists(data)) {
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

        var selectedSetup = self.selectedSetup();
        
        if (!exists(selectedSetup) || (selectedSetup == {}))
            return false;

        for (var i = 0; i < 3; ++i) {
            if (selectedSetup.Specs[i] !== teamSetup.Specs[i]) {
                return false;
            }
        }
        return true;
    };
    
    function getAllSpecs() {
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

    function getAllClasses() {
        var result = [];
        var dictionary = media.SpecsToClasses;

        for (var key in dictionary) {
            if (dictionary.hasOwnProperty(key)) {
                var classId = dictionary[key];
                if (-1 == $.inArray(classId, result))
                    result.push(classId);
            }
        }

        return result;
    }

    self.allSpecs = ko.observableArray(getAllSpecs());
    self.allClasses = ko.observableArray(getAllClasses());
    
    self.setupFilters = ko.observable([
        { classId: null, specId: null },
        { classId: null, specId: null },
        { classId: null, specId: null }
    ]);

    self.updateClassFilter = function (filterIndex, classId) {
        var idx = filterIndex();
        console.log(idx, classId);

        self.setupFilters()[idx].classId = classId;

        console.log("Sending filtering request for", self.setupFilters());
        
        self.hub.server.queryFilteredSetups(self.setupFilters());
        
        self.filterClassViews[idx](
            createHtmlForClass(classId)
        );
    };

    self.updateSpecFilter = function (filterIndex, spec) {
        if (filterIndex == null) {
            self.hub.server.queryFilteredSetups(self.setupFilters());
            return;
        }

        var specId = spec != null ? spec.specId : null;
        var idx = filterIndex();
        console.log(idx, specId);

        self.setupFilters()[idx].specId = specId;

        console.log("Sending filtering request for", self.setupFilters());

        self.hub.server.queryFilteredSetups( self.setupFilters() );

        self.filterSpecViews[idx](
            createHtmlForSpec(specId)
        );
    };

    var emptySpecHtml = "<span>All</span>";

    function createHtmlForSpec(specId) {
        console.log("create html called for specid:", specId);
        if (specId != null) {
            return '<img src="' + self.toSpecImage(specId) + '" alt="SpecImage" />';
        } else {
            return emptySpecHtml;
        }
    }

    function createHtmlForClass(classId) {
        if (classId != null) {
            return '<img src="' + self.toClassImage(classId) + '" alt="ClassImage" />';
        } else {
            return emptySpecHtml;
        }
    }

    self.filterClassViews = [
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml)
    ];

    self.filterSpecViews = [
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml),
        ko.observable(emptySpecHtml)
    ];

    self.filterClassView = function (d) {
        var idx = d();
        return self.filterClassViews[idx];
    };

    self.filterSpecView = function (d) {
        var idx = d();
        return self.filterSpecViews[idx];
    };
}

function initializePage(region, data, media) {
    console.log(media);

    var hub = $.connection.indexHub;

    var armory = new ArmoryViewModel(region, data, media);
    ko.applyBindings(armory);

    hub.client.update = function (msg) {
        console.log("Msg received", msg);
        armory.update(msg);
    };
    
    hub.client.showSetupTeams = function (teams) {
        console.log("Teams for queried setup received. Populating...");
        armory.fotmTeams(teams);
    };
    
    hub.client.showFilteredSetups = function (setups) {
        console.log("Filter response received: ", setups);
        armory.fotmSetups(setups);
    };

    $.connection.hub.start().done(function () {
        armory.hub = hub;
        armory.updateSpecFilter(null);

        hub.server.queryLatestUpdate();
    });
}