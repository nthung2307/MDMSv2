export class GetAccountDetailRequest {
  id: string;

  constructor(args: { id: string }) {
    this.id = args.id;
  }
}
