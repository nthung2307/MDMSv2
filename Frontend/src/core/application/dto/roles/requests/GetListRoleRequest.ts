export class GetListRoleRequest {
  size: number;
  page: number;
  name?: string;
  constructor(arg: { size: number; page: number; name?: string }) {
    this.size = arg.size;
    this.page = arg.page;
    this.name = arg.name;
  }
}
