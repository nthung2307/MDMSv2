export class ChangeCustomerStatusRequest {
  id: number;

  constructor(arg: { id: number }) {
    this.id = arg.id;
  }
}
