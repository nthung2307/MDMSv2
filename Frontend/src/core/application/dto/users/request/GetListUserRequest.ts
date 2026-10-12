export class GetListUserRequest {
  page: number;
  size: number;
  name?: string;
  status?: number;

  constructor(arg: { size: number; page: number; name?: string; status?: number }) {
    this.page = arg.page;
    this.size = arg.size;

    if (arg.name) {
      this.name = arg.name;
    }

    if (arg.status) {
      this.status = arg.status;
    }
  }
}
