export class GetListGCSRequest {
  pageSize?: number;
  after?: number;
  before?: number;
  searchByFields?:
    | [
        {
          searchFieldName: string;
          searchValue: string;
        },
      ]
    | [];
  sortByFields?:
    | [
        {
          colName: string;
          sortDirection: string;
        },
      ]
    | [];

  constructor(args: {
    pageSize: number;
    after?: number;
    before?: number;
    searchByFields?:
      | [
          {
            searchFieldName: string;
            searchValue: string;
          },
        ]
      | [];
    sortByFields?:
      | [
          {
            colName: string;
            sortDirection: string;
          },
        ]
      | [];
  }) {
    this.pageSize = args.pageSize;

    if (args.after) {
      this.after = args.after;
    }
    if (args.before) {
      this.before = args.before;
    }
    if (args.searchByFields) {
      this.searchByFields = args.searchByFields;
    }
    if (args.sortByFields) {
      this.sortByFields = args.sortByFields;
    }
  }
}
