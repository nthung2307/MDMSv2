export class DeleteAccountRequest {
  id: number;

  constructor(args: { id: number }) {
    this.id = args.id;
  }
}
