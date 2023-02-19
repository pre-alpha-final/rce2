var rce2 = {};
(async _rce2 => {
    let agentId = uuidv4();
    let address = "https://localhost:7113/api/agent/" + agentId;

    async function getFeed() {
        const response = await fetch(address, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });
        return response;
    }

    async function handleMessage(message) {
        switch (message.contact) {
            case "setColor":
                setColor(message.payload.data);
                break;

            default:
                await sendWhoIs();
                break;
        }
    }

    async function sendWhoIs() {
        const response = await fetch(address, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                'type': 'whois',
                'payload': {
                    'id': agentId,
                    'name': 'JS',
                    'ins': {
                        'setColor': 'string'
                    },
                    'outs': {}
                }
            })
        });
    }

    function setColor(color) {
        document.body.style.backgroundColor = color;
    }

    function uuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    while (true) {
        let feedResponse = await getFeed();
        for (let message of await feedResponse.json()) {
            await handleMessage(message);
        }
    }
})(rce2);
