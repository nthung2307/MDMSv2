export class GCSMonthlyDetailRequest {
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
  gcsCutOffCycleId: number;
  isChange?: boolean;

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

    gcsCutOffCycleId: number;
    isChange?: boolean;
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

    this.gcsCutOffCycleId = args.gcsCutOffCycleId;
    if (args.isChange) {
      this.isChange = args.isChange;
    }
  }
}
