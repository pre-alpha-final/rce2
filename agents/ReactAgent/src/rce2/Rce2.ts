import { Subject } from 'rxjs';

const rce2Input$ = new Subject<string>();

export const publishRce2Input = (input: string) => {
  rce2Input$.next(input);
};

export const rce2InputObservable$ = rce2Input$.asObservable();

// component
// subscribes to bus
// responds to whois

// fibonacci sends to input