app.controller('ChatController', ['$scope', '$rootScope', 'shared', 'media', function ($scope, $rootScope, shared, media) {

    console.log("ChatController activated");

    $rootScope.$watch('shared.currentRegion', function (newRegion, oldRegion) {
        if ($scope.chatLoaded) {
            $scope.messages = [];

            console.log("Switching chat rooms from", oldRegion, "to", newRegion, "...");
            chat.server.leaveRoom(oldRegion);
            chat.server.joinRoom(newRegion);
        }
    });

    $scope.data = { currentMessage: "" }; // workaround for ng-if creating a new scope

    $scope.maxNumberOfMessages = 100;
    $scope.maxMessageLength = 140;

    $scope.media = media;

    $scope.messages = [];
    $scope.usersOnline = [];

    function createAvatar(rawAvatar, userId) {
        return {
            race: rawAvatar.Fields[0],
            gender: rawAvatar.Fields[1],
            classSpec: rawAvatar.Fields[2],
            userId: userId
        };
    }

    function addMessage(rawAvatar, text) {
        var chatEntry = createAvatar(rawAvatar);
        chatEntry.text = text;
        $scope.messages.push(chatEntry);

        while ($scope.messages.length > $scope.maxNumberOfMessages) {
            $scope.messages.shift();
        }
    }

    function scrollDown() {
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
        scrollDown();
    }
    
    // subscribe to chat
    var chat = $.connection.chatHub;

    chat.client.userJoined = function(userId, chatAvatar) {
        console.log(userId, "with avatar", chatAvatar, "joined...");

        $scope.usersOnline.push(createAvatar(chatAvatar, userId));
        $scope.$digest();
    }

    chat.client.userLeft = function (userId) {
        console.log(userId, "left.");

        $scope.usersOnline = $.grep($scope.usersOnline, function(user) {
            return user.userId != userId;
        });

        $scope.$digest();
    }

    chat.client.setChatInfo = function(rawAvatar, currentMessages, users) {
        console.log("Avatar assigned:", rawAvatar, "messages:", currentMessages, "users online:", users);

        $scope.usersOnline = $.map(users, function(userAvatar) {
            return createAvatar(userAvatar.Fields[1], userAvatar.Fields[0].Fields[0]);
        });

        $scope.userRawAvatar = rawAvatar;
        $scope.userAvatar = createAvatar(rawAvatar);
        $scope.chatLoaded = true;
        $scope.$digest();

        for (var iMsg in currentMessages) {
            var msg = currentMessages[iMsg];
            addMessage(msg.Item1.Fields[1], msg.Item2.Fields[0]);
        }

        $scope.$digest();

        scrollDown();

        $('#chatInputBox').focus();
    }

    chat.client.messageAdded = function (sender, senderAvatar, message) {
        console.log(sender, "with avatar", senderAvatar, "sent message", message);
        addMessage(senderAvatar.Fields[1], message.Fields[0]);
        $scope.$digest();
        scrollDown();
    }

    shared.hubReady.done(function () {
        console.log("Subscribed to chat updates. Joining room", shared.currentRegion);
        chat.server.joinRoom(shared.currentRegion);
    });

}]);