// Code-behind viewmodel for /Home/Index action

function ArmoryViewModel(region, data, media) {
    
    function virtualPageView(virtualPage) {
        console.log(virtualPage);
        ga('send', 'pageview', virtualPage);
    }

    var self = this;

    self.hub = null;

    var latestSetupRequestGuid = null;
    var latestFilterRequestGuid = null;
    
    function queryServerForSetup(setup) {
        if (self.hub != null) {
            var requestGuid = genGuid();
            latestSetupRequestGuid = requestGuid;
            self.hub.server.queryTeamsForSetup(requestGuid, setup);
            virtualPageView("/teams");
        } else {
            console.error("hub is null");
        }
    }

    function queryServerForFilteredSetups(setupFilter) {
        if (self.hub != null) {
            var requestGuid = genGuid();
            latestFilterRequestGuid = requestGuid;
            self.hub.server.queryFilteredSetups(requestGuid, setupFilter);
            virtualPageView("/filter");
        } else {
            console.error("hub is null");
        }
    }

    self.populateFotmTeamsFromServer = function(requestGuid, teams) {
        if (requestGuid == latestSetupRequestGuid) {
            self.fotmTeams(teams);
        } else {
            console.log("Response to request", requestGuid, "is outdated and discarded.");
        }
    };

    self.populateFilteredSetupsFromServer = function (requestGuid, setups) {
        if (requestGuid == latestFilterRequestGuid) {
            self.fotmSetups(setups);
        } else {
            console.log("Response to request", requestGuid, "is outdated and discarded.");
        }
        
    };

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
    self.fotmTeams = ko.observableArray([]);
    self.fotmSetups = ko.observable(self.model().TeamSetupsViewModels);

    self.showTeams = function (setup) {
        self.showFotMHint(false);
        self.selectedSetup(setup);

        virtualPageView('/fotm?rank=' + setup.Rank);

        queryServerForSetup(setup);
    };

    function exists(d) {
        return !((typeof d === 'undefined') || (d === null));
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
        self.fotmTeams([]);

        var idx = filterIndex();
        console.log(idx, classId);

        var setupFilter = self.setupFilters();
        setupFilter[idx].classId = classId;
        setupFilter[idx].specId = null;

        var possibleSpecs = getSpecsFor(setupFilter[idx].classId);
        console.log("Setting possible specs", idx, "to", possibleSpecs);
        self.possibleSpecs[idx](possibleSpecs);

        console.log("Sending filtering request for", setupFilter);

        queryServerForFilteredSetups(setupFilter);
        
        self.filterClassViews[idx](
            createHtmlForClass(classId)
        );
        
        self.filterSpecViews[idx](
            createHtmlForSpec(null)
        );
    };

    self.updateSpecFilter = function (filterIndex, spec) {
        var setupFilter = self.setupFilters();
        
        if (filterIndex == null) {
            // only used on init
            queryServerForFilteredSetups(setupFilter);
            return;
        }
        
        self.fotmTeams([]);

        var specId = spec != null ? spec.specId : null;
        var idx = filterIndex();
        console.log(idx, specId);

        setupFilter[idx].specId = specId;

        console.log("Sending filtering request for", setupFilter);

        queryServerForFilteredSetups(setupFilter);

        self.filterSpecViews[idx](
            createHtmlForSpec(specId)
        );
    };

    var emptyClassHtml = "<span>All</span>";
    var emptySpecHtml = "<span>&nbsp;</span>";

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
            return emptyClassHtml;
        }
    }

    self.filterClassViews = [
        ko.observable(emptyClassHtml),
        ko.observable(emptyClassHtml),
        ko.observable(emptyClassHtml)
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
    
    function getSpecsFor(classId) {
        console.log("getSpecsFor called for", classId);

        // couldn't make $.grep work here for some reason
        var result = [];

        var dictionary = media.SpecsToClasses;

        for (var specId in dictionary) {
            if (dictionary.hasOwnProperty(specId)) {
                if (dictionary[specId] === classId) {
                    result.push({
                        specId: specId,
                        classId: dictionary[specId]
                    });
                }
            }
        }

        return result;
    }
    
    self.possibleSpecs = [
        ko.observableArray(),
        ko.observableArray(),
        ko.observableArray()
    ];
    
    self.specsFor = function ($i) {
        var i = $i();

        var specs = self.possibleSpecs[i];
        return specs();
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
    
    hub.client.showSetupTeams = function (requestGuid, teams) {
        console.log("Teams for queried setup received for request", requestGuid);
        armory.populateFotmTeamsFromServer(requestGuid, teams);
    };
    
    hub.client.showFilteredSetups = function (requestGuid, setups) {
        console.log("Filter response received for request guid", requestGuid);
        armory.populateFilteredSetupsFromServer(requestGuid, setups);
    };

    $.connection.hub.start().done(function () {
        armory.hub = hub;
        armory.updateSpecFilter(null);

        hub.server.queryLatestUpdate();
    });
}