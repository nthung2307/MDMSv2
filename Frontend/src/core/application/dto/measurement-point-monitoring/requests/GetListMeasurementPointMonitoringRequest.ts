export class GetListMeasurementPointMonitoringRequest {
  pageSize?: number;
  after?: number;
  before?: number;
  searchByFields?: Array<{ searchFieldName: string; searchValue: string }> | [];
  sortByFields?: Array<{ colName: string; sortDirection: string }> | [];
  customerName?: string;
  measurementpointCode?: string;
  measurementPointCode?: string;
  startDate?: string;
  endDate?: string;

  constructor(args: {
    pageSize?: number;
    after?: number;
    before?: number;
    searchByFields: Array<{ searchFieldName: string; searchValue: string }> | [];
    sortByFields: Array<{ colName: string; sortDirection: string }> | [];
    customerName?: string;
    measurementpointCode?: string;
    measurementPointCode?: string;
    startDate?: string;
    endDate?: string;
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
    if (args.customerName) {
      this.customerName = args.customerName;
    }
    if (args.measurementpointCode) {
      this.measurementpointCode = args.measurementpointCode;
    }
    if (args.measurementPointCode) {
      this.measurementPointCode = args.measurementPointCode;
    }
    if (args.startDate) {
      this.startDate = args.startDate;
    }
    if (args.endDate) {
      this.endDate = args.endDate;
    }
  }
}
