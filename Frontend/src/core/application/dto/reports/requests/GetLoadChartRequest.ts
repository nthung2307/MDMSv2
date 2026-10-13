export class GetLoadChartRequest {
  type?: string;
  timeSelector: string;
  measurementPointId: number;
  customerId: number;
  date: string;

  constructor(args: GetLoadChartRequest) {
    this.type = args.type;
    this.timeSelector = args.timeSelector;
    this.measurementPointId = args.measurementPointId;
    this.customerId = args.customerId;
    this.date = args.date;
  }
}
