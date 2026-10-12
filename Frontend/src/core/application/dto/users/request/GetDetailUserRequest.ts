export class GetDetailUserRequest {
  id: number;
  constructor(init?: Partial<GetDetailUserRequest>) {
    this.id = init?.id ?? 0;
  }
}
