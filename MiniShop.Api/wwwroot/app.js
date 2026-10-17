// klijent za /hubs/orders; od nedelje 10: withUrl("/hubs/orders", { accessTokenFactory })
const log = (text) => {
    const li = document.createElement("li");
    li.textContent = `${new Date().toLocaleTimeString()}  ${text}`;   // textContent: bez XSS-a
    document.getElementById("log").appendChild(li);
};

const conn = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/orders")
    .withAutomaticReconnect()
    .build();

let current = null;

conn.on("statusChanged", (o) => log(`narudzbenica ${o.id}: ${o.status}`));
conn.onreconnecting(() => log("veza prekinuta, ponovno povezivanje..."));
conn.onreconnected(async () => {
    // nova veza = novi ConnectionId, a grupe stare veze su izgubljene
    if (current !== null) await conn.invoke("Subscribe", current);
    log("veza obnovljena");
});

document.getElementById("subscribe").addEventListener("click", async () => {
    current = Number(document.getElementById("orderId").value);
    await conn.invoke("Subscribe", current);
    log(`pretplata na order-${current}`);
});

await conn.start();
log("povezano");