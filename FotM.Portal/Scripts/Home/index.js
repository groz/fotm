// Code-behind for Home/Index action

function ArmoryViewModel() {
    this.bracket = ko.observable("3v3");
    this.region = ko.observable("US");

    this.model = ko.observable({
        AllTimeLeaders: ko.observable(),
        PlayingNow: ko.observable(),
        TeamSetupsViewModels: ko.observable(),
    });
}

$(function () {

    var armory = new ArmoryViewModel();
    ko.applyBindings(armory);

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