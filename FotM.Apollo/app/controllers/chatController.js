app.controller('ChatController', ['$scope', '$rootScope', 'shared', 'media', function ($scope, $rootScope, shared, media) {

    console.log("ChatController activated");

    $rootScope.$watch('shared.currentRegion', function (newRegion, oldRegion) {
        if ($scope.chatLoaded) {
            $scope.messages = [];

            console.log("Switching rooms from", oldRegion, "to", newRegion, "...");
            chat.server.leaveRoom(oldRegion);
            chat.server.joinRoom(newRegion);
        }
    });

    $scope.data = { currentMessage: "" }; // workaround for ng-if creating a new scope

    $scope.maxNumberOfMessages = 100;
    $scope.maxMessageLength = 140;

    $scope.media = media;

    $scope.messages = [];

    function createAvatar(rawAvatar) {
        return {
            race: rawAvatar.Fields[0],
            gender: rawAvatar.Fields[1],
            classSpec: rawAvatar.Fields[2],
        };
    }

    function addMessage(rawAvatar, text) {
        var chatEntry = createAvatar(rawAvatar);
        chatEntry.text = text;
        $scope.messages.push(chatEntry);

        while ($scope.messages.length > $scope.maxNumberOfMessages) {
            $scope.messages.shift();
        }

        var chatDiv = $("#chatDiv");
        chatDiv.animate({ scrollTop: chatDiv[0].scrollHeight }, 100);
    }

    $scope.chatLoaded = false;

    $scope.sendMessage = function (text) {
        if (text.length == 0)
            return;

        console.log("sending", text, "to room", shared.currentRegion);

        text = text.substring(0, $scope.maxMessageLength);
        chat.server.message(shared.currentRegion, text);

        $scope.data.currentMessage = "";
        addMessage($scope.userRawAvatar, text);
    }
    
    // subscribe to chat
    var chat = $.connection.chatHub;
    var user = chat.id;
    console.log("User:", user);

    chat.client.userJoined = function(newUser, newUserAvatar) {
        console.log(newUser, "with avatar", newUserAvatar, "joined...");
    }

    chat.client.userLeft = function (newUser) {
        console.log(newUser, "left.");
    }

    chat.client.setChatInfo = function(rawAvatar, currentMessages) {
        console.log("Avatar assigned:", rawAvatar, "messages:", currentMessages);

        $scope.userRawAvatar = rawAvatar;
        $scope.userAvatar = createAvatar(rawAvatar);
        $scope.chatLoaded = true;
        $scope.$digest();

        for (var iMsg in currentMessages) {
            console.log(iMsg);
            var msg = currentMessages[iMsg];
            console.log(msg);
            addMessage(msg.Item1.Fields[1], msg.Item2.Fields[0]);
        }

        $scope.$digest();
    }

    chat.client.messageAdded = function (sender, senderAvatar, message) {
        console.log(sender, "with avatar", senderAvatar, "sent message", message);
        addMessage(senderAvatar.Fields[1], message.Fields[0]);
        $scope.$digest();
    }

    $.connection.hub.start().done(function () {
        console.log("SUBSCRIBED TO CHAT", shared.currentRegion);
        chat.server.joinRoom(shared.currentRegion);
    });

}]);