export interface IGetLoadChartResponse {
  date: string; // ISO date string format
  previousPeriodDate: string; // ISO date string format
  currentPeriodData: DataPoint[]; // Array of DataPoint type
  previousPeriodData: DataPoint[]; // Array of DataPoint type
}

export interface IGetVoltageChartResponse {
  date: string; // ISO date string format
  currentPeriodData: VoltageDataPoint[]; // Array of VoltageDataPoint type
}

export interface IGetPowerChartResponse {
  date: string; // ISO date string format
  currentPeriodData: PowerDataPoint[]; // Array of PowerDataPoint type
}

export interface IGetConsumptionOutputChartResponse {
  date: string; // ISO date string format
  unit: string;
  currentPeriodData: ConsumptionCurrentDataPoint[]; // Array of current period data points
  previousPeriodData: ConsumptionPreviousDataPoint[]; // Array of previous period data points
}

export interface IGetFinalizedOutputElectricityChartResponse {
  date: string;
  unit: string;
  currentPeriodData: Array<{
    period: number;
    month: number;
    bt: number;
    td: number;
    cd: number;
    sg: number;
    vc: number;
  }>;
  previousPeriodData: Array<{
    period: number; // The time period or identifier for the previous data
    month: number; // The month corresponding to the previous period
    previousSg: number; // The signal power for the previous period
    previousVc: number; // The voltage current for the previous period
  }>;
}

export interface IGetListFinalizedOutputElectricityResponse
  extends Array<FinalizedOutputElectricityData> {}

export interface IHalfHourFinalizedEnergyReportListResponse {
  hasNext: boolean;
  hasPrevious: boolean;
  data: IHalfHourFinalizedEnergyReportItem[];
}

export interface IDailyFinalizedEnergyReportListResponse {
  hasNext: boolean;
  hasPrevious: boolean;
  data: IDailyFinalizedEnergyReportItem[];
}

export interface IHalfHourFinalizedEnergyReportItem {
  id: number;
  createdTime: string;
  chiSoTichLuyDienNangTacDungAPTongKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit?: string;
  chiSoTichLuyDienNangPhanKhangAQTongKenhGiao?: number;
  chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPTongKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit?: string;
  chiSoTichLuyDienNangPhanKhangAQTongKenhNhan?: number;
  chiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit?: string;
}

export interface IDailyFinalizedEnergyReportItem {
  id: number;
  createdTime: string;
  chiSoTichLuyDienNangTacDungAPTongKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit?: string;
  chiSoTichLuyDienNangPhanKhangAQTongKenhGiao?: number;
  chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit?: string;
  chiSoTichLuyDienNangTacDungAPTongKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit?: string;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan?: number;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit?: string;
  chiSoTichLuyDienNangPhanKhangAQTongKenhNhan?: number;
  chiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit?: string;
}

export interface FinalizedOutputElectricityData {
  customerCode: string;
  customerName: string;
  measurementPointCode: string;
  measurementPointDescription: string;
  month: number;
  bt: number;
  td: number;
  cd: number;
  sg: number;
  vc: number;
}

export interface ConsumptionCurrentDataPoint {
  label: string;
  bt: number;
  td: number;
  cd: number;
  sg: number;
  vc: number;
}

export interface ConsumptionPreviousDataPoint {
  label: string;
  previousSg: number;
  previousVc: number;
}

export interface PowerDataPoint {
  labelMax: string;
  valueMax: number;
  labelMin: string;
  valueMin: number;
  unit: string;
}

export interface VoltageDataPoint {
  label: string;
  phaA: number;
  phaAUnit: string;
  phaB: number;
  phaBUnit: string;
  phaC: number;
  phaCUnit: string;
}

export interface DataPoint {
  label: string;
  value: number;
  unit: string;
}

export interface IActivePointsEnergyReportItem {
  measurementPointId: number;
  measurementPointCode: string;
  measurementPointName?: string;
  customerId: number;
  customerCode: string;
  customerName: string;
  multiplierFactor: number;
  createdTime?: string;

  // Kênh Giao (Export)
  chiSoTichLuyDienNangTacDungAPTongKenhGiao?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao?: string | null;
  chiSoTichLuyDienNangPhanKhangAQTongKenhGiao?: string | null;

  // Kênh Nhận (Import)
  chiSoTichLuyDienNangTacDungAPTongKenhNhan?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan?: string | null;
  chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan?: string | null;
  chiSoTichLuyDienNangPhanKhangAQTongKenhNhan?: string | null;
}

