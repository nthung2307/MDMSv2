export class DeleteCustomerRequest {
  id: number;

  constructor(arg: { id: number }) {
    this.id = arg.id;
  }
}
