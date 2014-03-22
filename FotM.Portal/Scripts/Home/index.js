// Code-behind viewmodel for /Home/Index action

function ArmoryViewModel(region, data) {
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
        // convert server times to local time
        $(msg.PlayingNow).each(function (idx) {
            this.LocalUpdateTime = asLocalTime(this.Updated);
        });

        $(msg.AllTimeLeaders).each(function (idx) {
            this.LocalUpdateTime = asLocalTime(this.Updated);
        });

        self.model().AllTimeLeaders(msg.AllTimeLeaders);
        self.model().PlayingNow(msg.PlayingNow);
        self.model().TeamSetupsViewModels(msg.TeamSetupsViewModels);
    };

    self.selectedTeams = ko.observable();

    self.showTeams = function (setup) {
        self.showFotMHint(false);
        self.selectedTeams(setup.Teams);

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
}

function initializePage(region, data) {
    var armory = new ArmoryViewModel(region, data);
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