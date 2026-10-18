export class GetOperatingParameterReportRequest {
  customerId: number;
  measurementPointId: number;
  startTime: string;
  endTime: string;
  before?: number;
  after?: number;
  pageSize?: number;

  constructor(args: GetOperatingParameterReportRequest) {
    this.customerId = args.customerId;
    this.measurementPointId = args.measurementPointId;
    this.startTime = args.startTime;
    this.endTime = args.endTime;
    this.before = args.before;
    this.after = args.after;
    this.pageSize = args.pageSize;
  }
}

export interface OperatingParameterItem {
  id?: number;
  createdTime?: string;
  dienApPhaA?: number;
  dienApPhaAUnit?: string;
  dienApPhaB?: number;
  dienApPhaBUnit?: string;
  dienApPhaC?: number;
  dienApPhaCUnit?: string;
  dongDienPhaA?: number;
  dongDienPhaAUnit?: string;
  dongDienPhaB?: number;
  dongDienPhaBUnit?: string;
  dongDienPhaC?: number;
  dongDienPhaCUnit?: string;
  congSuatTacDungTong3Pha?: number;
  congSuatTacDungTong3PhaUnit?: string;
  congSuatPhanKhangTong3Pha?: number;
  congSuatPhanKhangTong3PhaUnit?: string;
  heSoCongSuatPF?: number;
  tanSoHeThong?: number;
  tanSoHeThongUnit?: string;
}
