export class GetListDataGCSRequest {
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

  periodNumber: number;

  month: number;

  year?: number;

  isRefresh?: boolean;

  quanlity?: boolean;

  constructor(args: {
    pageSize: number;
    after?: number;
    before?: number;
    searchByFields?: [{ searchFieldName: 'string'; searchValue: 'string' }];
    sortByFields?: [{ colName: 'string'; sortDirection: 'string' }];
    gcsId: number;
    periodNumber: number;
    month: number;
    year: number;
    isRefresh?: boolean;
    quanlity?: boolean;
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
    this.periodNumber = args.periodNumber;
    this.month = args.month;
    if (args.sortByFields) {
      this.year = args.year;
    }
    this.isRefresh = args.isRefresh;
    this.quanlity = args.quanlity;
  }
}
