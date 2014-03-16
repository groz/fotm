// Code-behind for Home/Index action
$(function () {

    var hub = $.connection.indexHub;

    hub.client.update = function(msg) {
        alert("update received");
    };

    $.connection.hub.start();

});