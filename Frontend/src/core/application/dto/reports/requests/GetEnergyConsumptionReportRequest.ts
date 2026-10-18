export class GetEnergyConsumptionReportRequest {
  customerId: number;
  measurementPointId: number;
  cycleType: 'HALF_HOUR' | 'MONTHLY' | 'YEARLY';
  date?: string;
  month?: number;
  year: number;
  compareWithPrevious: boolean;

  constructor(args: GetEnergyConsumptionReportRequest) {
    this.customerId = args.customerId;
    this.measurementPointId = args.measurementPointId;
    this.cycleType = args.cycleType || 'HALF_HOUR';
    this.date = args.date;
    this.month = args.month;
    this.year = args.year;
    this.compareWithPrevious = args.compareWithPrevious ?? true;
  }
}

export interface EnergyConsumptionItem {
  timeLabel: string;
  createdTime?: string;
  multiplierFactor: number;

  sanLuongTacDungTongKenhGiao: number;
  sanLuongTacDungBTKenhGiao: number;
  sanLuongTacDungCDKenhGiao: number;
  sanLuongTacDungTDKenhGiao: number;
  sanLuongPhanKhangTongKenhGiao: number;

  sanLuongTacDungTongKenhNhan: number;
  sanLuongTacDungBTKenhNhan: number;
  sanLuongTacDungCDKenhNhan: number;
  sanLuongTacDungTDKenhNhan: number;
  sanLuongPhanKhangTongKenhNhan: number;

  previousSanLuongTacDungTongKenhGiao?: number;
  previousSanLuongTacDungTongKenhNhan?: number;
  deltaKenhGiao?: number;
  deltaPercentageKenhGiao?: number;
  deltaKenhNhan?: number;
  deltaPercentageKenhNhan?: number;
}

export interface EnergyConsumptionChartData {
  labels: string[];
  currentPeriodGiao: number[];
  previousPeriodGiao: number[];
  currentPeriodNhan: number[];
  previousPeriodNhan: number[];
}

export interface EnergyConsumptionReportResponse {
  items: EnergyConsumptionItem[];
  total?: EnergyConsumptionItem;
  chart: EnergyConsumptionChartData;
}
