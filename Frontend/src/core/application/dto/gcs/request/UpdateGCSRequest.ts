export class UpdateGCSRequest {
  id: number;
  code: string;
  name: string;
  periodNumber: number;
  description: string;
  status: string;
  dayPeriod: Array<number>;

  constructor(args: {
    id: number;
    code: string;
    name: string;
    periodNumber: number;
    description: string;
    status: string;
    dayPeriod: Array<number>;
  }) {
    this.id = args.id;
    this.code = args.code;
    this.name = args.name;
    this.periodNumber = args.periodNumber;
    this.description = args.description;
    this.status = args.status;
    this.dayPeriod = args.dayPeriod;
  }
}
