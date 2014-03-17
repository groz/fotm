// Code-behind for Home/Index action

function ArmoryViewModel() {
    this.bracket = ko.observable("3v3");
    this.region = ko.observable("EU");
}

$(function () {
    
    var armory = new ArmoryViewModel();
    ko.applyBindings(armory);
   
    var hub = $.connection.indexHub;

    hub.client.update = function(msg) {
        console.log(msg);
    };

    $.connection.hub.start();

});