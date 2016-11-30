var namespace = {};

namespace.instances = [];

namespace.jobs = [];

namespace.updateJob = function updateJob(state, jobName, instanceId) {
    console.info(jobName);
    $.ajax({
        type: "POST",
        url: '/api/Command',
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify({ jobname: jobName, instanceid: instanceId, state: state }),
        success: function () {
            alert("Posted");
        },
        failure: function (error) {
            console.error(error);
        }
        //,
        //dataType: dataType
    });
}

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


    function refreshState() {

        var html = "<h2>Running Jobs</h2>";

        if (namespace.jobs.length > 0) {
            for (var i = 0, len = namespace.jobs.length; i < len; i++) {

                var job = namespace.jobs[i];

                html += "<h3>" + job.jobName + "</h3><ul>"

                for (var j = 0, len1 = job.data.length; j < len; j++) {
                    var data = job.data[i];
                    html += "<li> InstanceId: " + data.instanceId;
                    if (data.state === "Up") {
                        html += " <button class='btn btn-success' onclick='namespace.updateJob(0,\"" + job.jobName + "\",\"" + data.instanceId + "\");'>Disable Instance</button>";
                    }
                    else {
                        html += " <button class='btn btn-warning' onclick='namespace.updateJob(1,\"" + job.jobName + "\",\"" + data.instanceId + "\");'>Enable Instance</button>";
                    }
                    html += "</li>";
                }
                html += "</ul>";
            }

        }
        $("#jobs").html(html);
    }

    function updateJob(jobInfo) {
        if (namespace.jobs.length === 0)
            namespace.jobs.push({ jobName: jobInfo.jobName, data: [{ instanceId: jobInfo.instanceId, state: jobInfo.state }] });
        else
            for (var i = 0, len = namespace.jobs.length; i < len; i++) {

                var job = namespace.jobs[i];

                if (job.jobName === jobInfo.jobName) {

                    for (var j = 0, len1 = job.data.length; j < len1; j++) {
                        var inner = job.data[j];

                        if (inner.instanceId === jobInfo.instanceId) {
                            job.data[j].state = jobInfo.state;
                        }
                        else {
                            job.data.push({ instanceId: jobInfo.instanceId, state: jobInfo.state });
                        }
                    }
                }
                else
                    namespace.jobs.push({ jobName: jobInfo.jobName, data: [{ instanceId: jobInfo.instanceId, state: jobInfo.state }] });
            }
    }

    chat.client.pulse = function (jobName, instanceId, state) {

        //console.info(jobName + ' ' + instanceid + ' ' + state);

        //$('#discussion').append('<li><strong>' + instanceid
        //    + '</strong>:&nbsp;&nbsp;' + state + '</li>');

        var obj = { jobName: jobName, instanceId: instanceId, state: state };

        updateJob(obj);

        refreshState();

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