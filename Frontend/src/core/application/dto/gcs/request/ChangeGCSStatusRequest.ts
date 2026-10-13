export class ChangeGCSStatusRequest {
  id: number;

  constructor(arg: { id: number }) {
    this.id = arg.id;
  }
}
