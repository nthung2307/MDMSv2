export class GetListParamRequest {
  size: number;
  page: number;
  name?: string;
  status?: number;
  constructor(arg: { size: number; page: number; name?: string; status: number }) {
    this.size = arg.size;
    this.page = arg.page;
    this.name = arg.name;
    this.status = arg.status;
  }
}
