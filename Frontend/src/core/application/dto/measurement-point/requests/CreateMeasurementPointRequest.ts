export class CreateMeasurementPointRequest {
  code: string;
  type: string;
  address: string;
  description: string;
  customerId: number;
  gcsId: number;
  multiplierFactor: number;
  status: string;
  meterSerial?: string | null;
  meterProfileId?: number | null;
  mappingMode?: string;

  constructor(args: {
    code: string;
    type: string;
    address: string;
    description: string;
    customerId: number;
    multiplierFactor: number;
    gcsId: number;
    status: string;
    meterSerial?: string | null;
    meterProfileId?: number | null;
    mappingMode?: string;
  }) {
    this.code = args.code;
    this.type = args.type;
    this.address = args.address;
    this.description = args.description;
    this.customerId = args.customerId;
    this.multiplierFactor = args.multiplierFactor;
    this.gcsId = args.gcsId;
    this.status = args.status;
    this.meterSerial = args.meterSerial ?? null;
    this.meterProfileId = args.meterProfileId ?? null;
    this.mappingMode = args.mappingMode || 'HYBRID';
  }
}
