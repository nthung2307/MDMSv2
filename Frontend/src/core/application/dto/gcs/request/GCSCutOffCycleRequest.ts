export class GCSCutOffCycleRequest {
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
  gcsId: number;

  month?: number;

  year?: number;

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

    gcsId: number;

    month?: number;

    year?: number;
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

    this.gcsId = args.gcsId;
    if (args.month) {
      this.month = args.month;
    }
    if (args.year) {
      this.year = args.year;
    }
  }
}
