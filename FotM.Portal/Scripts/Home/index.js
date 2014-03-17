// Code-behind for Home/Index action

function ArmoryViewModel() {
    var self = this;
    
    self.bracket = ko.observable("3v3");
    self.region = ko.observable("US");
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
}

$(function () {

    var armory = new ArmoryViewModel();
    ko.applyBindings(armory);
    
    armory.leaderboardSelected(true);
    armory.playingNowSelected(false);
    
    $("#leaderboardBtn").click(function () {
        armory.leaderboardSelected(true);
        armory.playingNowSelected(false);
    });
    
    $("#playingNowBtn").click(function () {
        armory.leaderboardSelected(false);
        armory.playingNowSelected(true);
    });

    var hub = $.connection.indexHub;

    hub.client.update = function (msg) {
        console.log(msg);

        $(msg.PlayingNow).each(function (idx) {
            this.LocalUpdateTime = asLocalTime(this.Updated);
        });
        
        $(msg.AllTimeLeaders).each(function (idx) {
            this.LocalUpdateTime = asLocalTime(this.Updated);
        });

        armory.model().AllTimeLeaders(msg.AllTimeLeaders);
        armory.model().PlayingNow(msg.PlayingNow);
        armory.model().TeamSetupsViewModels(msg.TeamSetupsViewModels);
    };

    $.connection.hub.start().done(function () {
        hub.server.queryLatestUpdate();
    });

});