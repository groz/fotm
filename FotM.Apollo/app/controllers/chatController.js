app.controller('ChatController', ['$scope', '$rootScope', 'shared', 'media', function ($scope, $rootScope, shared, media) {

    console.log("ChatController activated");

    $rootScope.$watch('shared.currentRegion', function (newRegion, oldRegion) {
        if ($scope.chatLoaded) {
            $scope.messages = [];

            console.log("Switching chat rooms from", oldRegion, "to", newRegion, "...");
            chat.server.leaveRoom(oldRegion);
            chat.server.joinRoom(newRegion, shared.adminKey);
        }
    });

    $scope.data = { currentMessage: "" }; // workaround for ng-if creating a new scope

    $scope.maxNumberOfMessages = 100;
    $scope.maxMessageLength = 140;

    $scope.media = media;

    $scope.messages = [];
    $scope.usersOnline = [];

    function createAvatar(userAvatar) {
        return {
            race: userAvatar.chatAvatar.Fields[0],
            gender: userAvatar.chatAvatar.Fields[1],
            classSpec: userAvatar.chatAvatar.Fields[2],
            userId: userAvatar.user.id.Fields[0],
            isAdmin : userAvatar.user.isAdmin
        };
    }

    function addMessage(userAvatar, text) {
        var chatEntry = createAvatar(userAvatar);
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
        addMessage($scope.userAvatar, text);
        scrollDown();
    }
    
    // subscribe to chat
    var chat = $.connection.chatHub;

    chat.client.userJoined = function(userAvatar) {
        console.log(userAvatar, "joined...");

        $scope.usersOnline.push(createAvatar(userAvatar));
        $scope.$digest();
    }

    chat.client.userLeft = function (userId) {
        console.log(userId, "left.");

        $scope.usersOnline = $.grep($scope.usersOnline, function(user) {
            return user.userId != userId;
        });

        $scope.$digest();
    }

    chat.client.setChatInfo = function(userAvatar, currentMessages, users) {
        console.log("Avatar assigned:", userAvatar, "messages:", currentMessages, "users online:", users);

        $scope.usersOnline = $.map(users, createAvatar);
        $scope.userAvatar = userAvatar;
        $scope.userAvatarVM = createAvatar(userAvatar);
        $scope.chatLoaded = true;

        $scope.$digest();

        for (var iMsg in currentMessages) {
            var msg = currentMessages[iMsg];
            console.log(msg);
            addMessage(msg.Item1, msg.Item2.Fields[0]);
        }

        $scope.$digest();

        scrollDown();

        $('#chatInputBox').focus();
    }

    chat.client.messageAdded = function (userAvatar, text) {
        console.log(userAvatar, "sent message", text);
        addMessage(userAvatar, text.Fields[0]);
        $scope.$digest();
        scrollDown();
    }

    shared.hubReady.done(function () {
        console.log("Subscribed to chat updates. Joining room", shared.currentRegion);
        chat.server.joinRoom(shared.currentRegion, shared.adminKey);
    });

}]);