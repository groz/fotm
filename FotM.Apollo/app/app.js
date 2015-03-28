var app = angular.module('app', ['ngRoute', 'ngCookies']);

// uncomment the following line to disable all console output in production
//console.log = function() {}

$(function () {
    $.ajax({
        url: "https://googledrive.com/host/0B6k5NinKpXuAfkRhb0VyTHJFNnFFZFpmQmpYY1ZZLWhLUnFqNzRzZDJLOGh6MWZOYmUwblU/donations.json",
        success: function(data) {
            updateDonations(data);
        }
    });

    function updateDonations(data) {
        var donationBar = $('#donationProgress');

        var sum = 0;
        for (var i = 0; i < data.donations.length; ++i) {
            sum += data.donations[i].amount;
        }

        var val = sum * 100.0 / data.goal;
        donationBar.width(val + "%");
        donationBar.text("$"+sum + " / $" + data.goal + " ("+val.toFixed(0)+"%)");
    }

});