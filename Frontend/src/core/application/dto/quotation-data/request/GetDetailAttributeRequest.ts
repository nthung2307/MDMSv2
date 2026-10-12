export class GetDetailAttributeRequest {
  id: number;
  constructor(init?: Partial<GetDetailAttributeRequest>) {
    this.id = init?.id ?? 0;
  }
}
