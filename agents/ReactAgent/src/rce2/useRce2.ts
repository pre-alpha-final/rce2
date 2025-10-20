import { Subject } from "rxjs";
import type { Rce2Message } from "./Rce2Message";
import type { UseRce2Type } from "./UseRce2Type";

const id = crypto.randomUUID();
const rce2Url = "https://localhost:7113/api/agent/" + id;
const rce2InputSubject$ = new Subject<Rce2Message>();
const rce2Input$ = rce2InputSubject$.asObservable();
let isRunning = false;

async function getFeed(): Promise<Rce2Message[]> {
  const response = await fetch(rce2Url);
  if (!response.ok) {
    throw new Error("Can't fetch feed");
  }
  const data: Rce2Message[] = await response.json();

  return data;
}

async function feedHandler() {
  let delay = 1;
  try {
    const rce2Messages = await getFeed();
    for (const rce2Message of rce2Messages) {
      if (rce2Message.type === "whois") {
        await rce2Send(
          null,
          {
            id: id,
            channels: [],
            name: "React Agent",
            ins: { input: "string" },
            outs: { output: "string" },
          },
          "whois"
        );
      } else {
        rce2InputSubject$.next(rce2Message);
      }
    }
  } catch (error) {
    console.error("Feed error:", error);
    delay = 1000;
  }

  setTimeout(() => feedHandler(), delay);
}

async function rce2Send(
  contact: string | null,
  payload: unknown,
  type: string = ""
) {
  await fetch(rce2Url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      type: type,
      contact: contact,
      payload: payload,
    }),
  });
}

export default function useRce2(): UseRce2Type {
  if (isRunning === false) {
    isRunning = true;
    setTimeout(() => feedHandler(), 1);
  }

  return [rce2Input$, rce2Send];
}
