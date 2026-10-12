export class CreateParamRequest {
  name: string;
  description: string;
  status: number;
  constructor(arg: { name: string; description: string; status: number }) {
    this.name = arg.name;
    this.description = arg.description;
    this.status = arg.status;
  }
}
