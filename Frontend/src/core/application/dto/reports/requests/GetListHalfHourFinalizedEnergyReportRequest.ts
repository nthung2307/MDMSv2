export class GetListHalfHourFinalizedEnergyReportRequest {
  customerId: number;
  measurementPointId: number;
  startTime: Date;
  endTime: Date;
  before?: number;
  after?: number;
  pageSize?: number;

  constructor(args: GetListHalfHourFinalizedEnergyReportRequest) {
    this.customerId = args.customerId;
    this.measurementPointId = args.measurementPointId;
    this.startTime = args.startTime;
    this.endTime = args.endTime;
    this.before = args.before;
    this.after = args.after;
    this.pageSize = args.pageSize;
  }
}
