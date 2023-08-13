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
                        'setColorNumber': 'number',
                        'setColorString': 'string'
                    },
                    'outs': {}
                }
            })
        });
    }

    function setColorNumber(number) {
        document.body.style.backgroundColor = toColor(number);
    }

    function setColorString(color) {
        document.body.style.backgroundColor = color;
    }
	
    function toColor(number) {
        number >>>= 0;
        var b = number & 0xFF,
            g = (number & 0xFF00) >>> 8,
            r = (number & 0xFF0000) >>> 16,
            a = ((number & 0xFF000000) >>> 24) / 255;
        //return "rgba(" + [r, g, b, a].join(",") + ")";
		return "rgba(" + [r, g, b, 1].join(",") + ")";
    }

    function uuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    async function tryRun(f) {
        try {
            await f();
        } catch (e) {}
    }

    while (true) {
        try {
            let feedResponse = await getFeed();
            for (let message of await feedResponse.json()) {
                switch (message.contact) {
                    case "setColorNumber":
                        await tryRun(() => setColorNumber(message.payload.data));
                        break;

                    case "setColorString":
                        await tryRun(() => setColorString(message.payload.data));
                        break;

                    default:
                        await tryRun(() => sendWhoIs());
                        break;
                }
            }

        } catch (e) {
            // ignore
        }
    }
})(rce2);
