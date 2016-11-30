var namespace = {};
namespace.instances = [];

$(function () {
    // Declare a proxy to reference the hub.
    var chat = $.connection.cSMonitorHub;
    // Create a function that the hub can call to broadcast messages.
    //chat.client.broadcastMessage = function (name, message) {
    //    // Html encode display name and message.
    //    var encodedName = $('<div />').text(name).html();
    //    var encodedMsg = $('<div />').text(message).html();
    //    // Add the message to the page.
    //    $('#discussion').append('<li><strong>' + encodedName
    //        + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
    //};
    //chat.client.tick = function (tick) {
    //    console.info(tick);
    //};

    chat.client.pulse = function (instanceid, state) {
        console.info(instanceid + ' ' + state);

        $('#discussion').append('<li><strong>' + instanceid
            + '</strong>:&nbsp;&nbsp;' + state + '</li>');

        var obj = { instanceId: instanceid, state: state };

        if (namespace.instances.length === 0)
            namespace.instances.push(obj);
        else
            for (var i = 0, len = namespace.instances.length; i < len; i++) {
                if (namespace.instances[i].instanceId === obj.instanceId)
                    namespace.instances[i].state = obj.state;
                else
                    namespace.instaces.push(obj);
            }
    }
    // Get the user name and store it to prepend to messages.
    //$('#displayname').val(prompt('Enter your name:', ''));
    //// Set initial focus to message input box.
    //$('#message').focus();
    // Start the connection.
    $.connection.hub.start().done(function () {
        //$('#sendmessage').click(function () {
        //    // Call the Send method on the hub.
        //    chat.server.send($('#displayname').val(), $('#message').val());
        //    // Clear text box and reset focus for next comment.
        //    $('#message').val('').focus();
        //});
    });
});