import type { Observable } from "rxjs";
import type { Rce2Message } from "./Rce2Message";

export type UseRce2Type = [Observable<Rce2Message>, (contact: string | null, payload: unknown, type?: string) => Promise<void>];
