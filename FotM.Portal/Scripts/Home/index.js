// Code-behind for Home/Index action

function ArmoryViewModel() {
    this.bracket = ko.observable("3v3");
    this.region = ko.observable("US");

    this.model = ko.observable({
        TeamStatsViewModels: ko.observable(),
        TeamSetupsViewModels: ko.observable(),
    });
}

$(function () {

    var armory = new ArmoryViewModel();
    ko.applyBindings(armory);

    var hub = $.connection.indexHub;

    hub.client.update = function (msg) {
        console.log(msg);

        $(msg.TeamStatsViewModels).each(function (idx) {
            this.LocalUpdateTime = asLocalTime(this.Updated);
        });

        armory.model().TeamStatsViewModels(msg.TeamStatsViewModels);
        armory.model().TeamSetupsViewModels(msg.TeamSetupsViewModels);
    };

    $.connection.hub.start().done(function () {
        hub.server.queryLatestUpdate();
    });

});