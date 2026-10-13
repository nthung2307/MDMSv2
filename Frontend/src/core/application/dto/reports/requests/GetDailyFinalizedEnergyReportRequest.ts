export class GetListDailyDailyFinalizedEnergyReportRequest {
  customerId: number;
  measurementPointId: number;
  month: number;
  year: number;
  before?: number;
  after?: number;
  pageSize?: number;

  constructor(args: GetListDailyDailyFinalizedEnergyReportRequest) {
    this.customerId = args.customerId;
    this.measurementPointId = args.measurementPointId;
    this.month = args.month;
    this.year = args.year;
    this.before = args.before;
    this.after = args.after;
    this.pageSize = args.pageSize;
  }
}
