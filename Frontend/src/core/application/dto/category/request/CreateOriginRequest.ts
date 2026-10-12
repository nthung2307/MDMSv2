export class CreateOriginRequest {
  name: string;
  description?: string;
  status?: number;

  constructor(arg: { description?: string; name: string; status?: number }) {
    this.name = arg.name;
    this.description = arg.description;
    this.status = arg.status;
  }
}
