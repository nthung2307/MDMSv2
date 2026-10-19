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
  isSuspected?: boolean;

  sanLuongTacDungTongKenhGiao?: number | null;
  sanLuongTacDungBTKenhGiao?: number | null;
  sanLuongTacDungCDKenhGiao?: number | null;
  sanLuongTacDungTDKenhGiao?: number | null;
  sanLuongPhanKhangTongKenhGiao?: number | null;

  sanLuongTacDungTongKenhNhan?: number | null;
  sanLuongTacDungBTKenhNhan?: number | null;
  sanLuongTacDungCDKenhNhan?: number | null;
  sanLuongTacDungTDKenhNhan?: number | null;
  sanLuongPhanKhangTongKenhNhan?: number | null;

  previousSanLuongTacDungTongKenhGiao?: number;
  previousSanLuongTacDungBTKenhGiao?: number;
  previousSanLuongTacDungCDKenhGiao?: number;
  previousSanLuongTacDungTDKenhGiao?: number;
  previousSanLuongPhanKhangTongKenhGiao?: number;

  previousSanLuongTacDungTongKenhNhan?: number;
  previousSanLuongTacDungBTKenhNhan?: number;
  previousSanLuongTacDungCDKenhNhan?: number;
  previousSanLuongTacDungTDKenhNhan?: number;
  previousSanLuongPhanKhangTongKenhNhan?: number;

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
