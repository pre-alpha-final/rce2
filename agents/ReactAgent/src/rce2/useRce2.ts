import { Observable, Subject } from "rxjs";

const id = (() => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => ('x' === c ? (Math.random() * 16 | 0) : (Math.random() * 16 | 0) & 0x3 | 0x8).toString(16)))();
const rce2Url = "https://localhost:7113/api/agent/" + id;
let isRunning = false;

const rce2InputSubject$ = new Subject<Rce2Message>();
const rce2Input$ = rce2InputSubject$.asObservable();

interface Rce2Message {
  type: string;
  contact: string;
  payload: unknown;
}

async function getFeed(): Promise<Rce2Message[]> {
  const response = await fetch(rce2Url);
  if (!response.ok) {
    throw new Error("Network response was not ok");
  }
  const data: Rce2Message[] = await response.json();

  return data;
}

async function rce2Send(contact: string | null, payload: unknown, type: string = '') {
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

async function feedHandler() {
  let delay = 1;

  try {
    const rce2Messages = await getFeed();
    rce2Messages.forEach(async (e) => {
      if (e.type === 'whois') {
        await rce2Send(null, {
          id: id,
          channels: [],
          name: "React Agent",
          ins: {
            input: 'string',
          },
          outs: {
            output: 'string',
          },
        }, 'whois');
        return;
      }
      else {
        rce2InputSubject$.next(e);
      }
    });
  } catch (error) {
    console.error("Feed error:", error);
    delay = 1000;
  }

  setTimeout(() => feedHandler(), delay);
}

export default function useRce2(): [Observable<Rce2Message>, (contact: string | null, payload: unknown, type?: string) => Promise<void>] {
  if (isRunning === false) {
    isRunning = true;
    feedHandler();
  }

  return [rce2Input$, rce2Send];
}
