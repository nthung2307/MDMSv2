export class GetFinalizedOutputElectricityChartRequest {
  measurementPointId: number;
  customerId: number;
  date: string;

  constructor(args: GetFinalizedOutputElectricityChartRequest) {
    this.measurementPointId = args.measurementPointId;
    this.customerId = args.customerId;
    this.date = args.date;
  }
}
