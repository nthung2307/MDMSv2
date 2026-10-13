export class GetVoltageChartRequest {
  type: string;
  timeSelector: string;
  measurementPointId: number;
  customerId: number;
  date: string;

  constructor(args: GetVoltageChartRequest) {
    this.type = args.type;
    this.timeSelector = args.timeSelector;
    this.measurementPointId = args.measurementPointId;
    this.customerId = args.customerId;
    this.date = args.date;
  }
}
