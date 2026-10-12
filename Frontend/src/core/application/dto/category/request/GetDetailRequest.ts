export class GetDetailRequest {
  id: number;
  constructor(init?: Partial<GetDetailRequest>) {
    this.id = init?.id ?? 0;
  }
}
