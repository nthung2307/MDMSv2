export class CreateBrandRequest {
  name: string;
  banner?: string;
  description?: string;
  status?: number;

  constructor(arg: { banner?: string; description?: string; name: string; status?: number }) {
    this.name = arg.name;
    this.description = arg.description;
    this.banner = arg.banner;
    this.status = arg.status;
  }
}
