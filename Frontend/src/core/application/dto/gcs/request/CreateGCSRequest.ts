export class CreateGCSRequest {
  code: string;
  name: string;
  periodNumber: number;
  description: string;
  dayPeriod: Array<number>;
  status: string;

  constructor(args: {
    code: string;
    name: string;
    periodNumber: number;
    description: string;
    dayPeriod: Array<number>;
    status: string;
  }) {
    this.code = args.code;
    this.name = args.name;
    this.periodNumber = args.periodNumber;
    this.description = args.description;
    this.dayPeriod = args.dayPeriod;
    this.status = args.status;
  }
}
