export class DeleteGCSRequest {
  id: number;

  constructor(arg: { id: number }) {
    this.id = arg.id;
  }
}
