const WebSocket = require('ws');

const endingCharacter = String.fromCharCode(30);

const wsClient = new WebSocket("ws://localhost:5289/hub");

wsClient.onopen = () => {
    wsClient.send(`{ "protocol": "json", "version": 1 }${endingCharacter}`);
    console.log('WS Connected');
    wsClient.send(`{ "arguments": [5, 1], "target": "JoinProcess", "type": 1 }${endingCharacter}`);
}

const StartProcess = (arguments) => {
    console.log("Start: ", arguments);
};

const UpdateProcessStatus = (arguments) => {
    console.log("Update: ", arguments);
};

const EndProcess = (arguments) => {
    console.log("End: ", arguments);
    wsClient.close()
};

const JoinConfirmation = (arguments) => {
    console.log("Joined: ", arguments);
};

wsClient.onclose = () => {
    console.log('WS Disconnected');
}

wsClient.onerror = (err) => console.log(`WS Error ${JSON.stringify(err)}`);

wsClient.onmessage = (e) => {
    // { "data": " {\"type\":1,\"target\":\"UpdateProcessStatus\",\"arguments\":[{\"step\":5,\"progress\":1,\"message\":\"Test message 5\"}]} " }
    var { data } = e;
    console.log();
    console.log();
    console.log("A new message was received: ", data);

    // {"type":1,"target":"UpdateProcessStatus","arguments":[{"step":5,"progress":1,"message":"Test message 5"}]}
    const { type, target, arguments } = JSON.parse(data.substring(0, data.length - 1)); // remove endingCharacter

    if (type !== 1) return; // interpret only type 1 messaages

    if (target === "StartProcess") {
        StartProcess(arguments);
    }

    if (target === "UpdateProcessStatus") {
        UpdateProcessStatus(arguments);
    }

    if (target === "EndProcess") {
        EndProcess(arguments);
    }

    if (target === "JoinConfirmation") {
        JoinConfirmation(arguments);
    }
}
