export class GetListProductCategoryRequest {
  page: number;
  size: number;
  keyword?: string;
  status?: number;

  constructor(arg: { size: number; page: number; keyword?: string; status?: number }) {
    this.page = arg.page;
    this.size = arg.size;

    if (arg.keyword) {
      this.keyword = arg.keyword;
    }

    if (arg.status) {
      this.status = arg.status;
    }
  }
}
