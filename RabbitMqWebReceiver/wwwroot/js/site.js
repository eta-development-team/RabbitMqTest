const connection = new signalR.HubConnectionBuilder()
    .withUrl("/rabbitMqHub")
    .build();

connection.start().then( () => {
    console.log("SignarR has been started!")
}).catch( (err) => {
    console.error(err)
});

connection.on("ReceiveMessage", (message) => {
    const tag = document.createElement("p");
    const text = document.createTextNode(message);
    tag.appendChild(text);
    const element = document.body;
    element.appendChild(tag);
});