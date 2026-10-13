export class GetListMeasurementPointsRequest {
  pageSize?: number;
  after?: number;
  before?: number;
  searchByFields?: {
    searchFieldName: string;
    searchValue: string;
  }[];
  sortByFields?: {
    colName: string;
    sortDirection: string;
  }[];
  customerId?: number;

  constructor(args: GetListMeasurementPointsRequest) {
    this.pageSize = args.pageSize;
    this.after = args.after;
    this.before = args.before;
    if (args.searchByFields) {
      this.searchByFields = args.searchByFields;
    }

    if (args.sortByFields) {
      this.sortByFields = args.sortByFields;
    }
    this.customerId = args.customerId;
  }
}
