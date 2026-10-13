export class UpdateMeasurementPointRequest {
  id: number;
  code: string;
  type: string;
  address: string;
  description: string;
  customerId: number;
  status: string;
  gcsId: number;
  multiplierFactor: number;

  constructor(args: {
    id: number;
    code: string;
    type: string;
    address: string;
    description: string;
    status: string;
    customerId: number;
    multiplierFactor: number;
    gcsId: number;
  }) {
    this.id = args.id;
    this.code = args.code;
    this.type = args.type;
    this.address = args.address;
    this.description = args.description;
    this.status = args.status;
    this.customerId = args.customerId;
    this.multiplierFactor = args.multiplierFactor;
    this.gcsId = args.gcsId;
  }
}
