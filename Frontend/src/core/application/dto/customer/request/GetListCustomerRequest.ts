export class GetListCustomerRequest {
  pageSize?: number;
  after?: number;
  before?: number;
  pageNumber?: number;
  searchByFields?: Array<{
    searchFieldName: string;
    searchValue: string;
  }>;
  sortByFields?: Array<{
    colName: string;
    sortDirection: string;
  }>;

  constructor(args: {
    pageSize: number;
    after?: number;
    before?: number;
    pageNumber?: number;
    searchByFields?: Array<{
      searchFieldName: string;
      searchValue: string;
    }>;
    sortByFields?: Array<{
      colName: string;
      sortDirection: string;
    }>;
  }) {
    this.pageSize = args.pageSize;

    if (args.after) {
      this.after = args.after;
    }
    if (args.before) {
      this.before = args.before;
    }
    if (args.pageNumber) {
      this.pageNumber = args.pageNumber;
    }
    if (args.searchByFields) {
      this.searchByFields = args.searchByFields;
    }
    if (args.sortByFields) {
      this.sortByFields = args.sortByFields;
    }
  }
}
