export class GetYearlyFinalizedEnergyReportRequest {
  customerId: number;
  measurementPointId: number;
  year: number;
  before?: number;
  after?: number;
  pageSize?: number;

  constructor(args: GetYearlyFinalizedEnergyReportRequest) {
    this.customerId = args.customerId;
    this.measurementPointId = args.measurementPointId;
    this.year = args.year;
    this.before = args.before;
    this.after = args.after;
    this.pageSize = args.pageSize;
  }
}
