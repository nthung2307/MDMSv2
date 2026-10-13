export class GetListFinalizedOutputElectricityRequest {
  measurementPointId: number;
  customerId: number;
  month: number;
  year: number;

  constructor(args: GetListFinalizedOutputElectricityRequest) {
    this.measurementPointId = args.measurementPointId;
    this.customerId = args.customerId;
    this.month = args.month;
    this.year = args.year;
  }
}
