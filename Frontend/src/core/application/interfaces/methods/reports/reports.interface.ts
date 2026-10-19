import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { GetConsumptionOutputChartRequest } from '@/core/application/dto/reports/requests/GetConsumptionOutputChartRequest.ts';
import { GetListDailyDailyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetDailyFinalizedEnergyReportRequest.ts';
import { GetFinalizedOutputElectricityChartRequest } from '@/core/application/dto/reports/requests/GetFinalizedOutputElectricityChartRequest.ts';
import { GetListFinalizedOutputElectricityRequest } from '@/core/application/dto/reports/requests/GetListFinalizedOutputElectricityRequest.ts';
import { GetListHalfHourFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetListHalfHourFinalizedEnergyReportRequest.ts';
import { GetLoadChartRequest } from '@/core/application/dto/reports/requests/GetLoadChartRequest.ts';
import { GetPowerChartRequest } from '@/core/application/dto/reports/requests/GetPowerChartRequest.ts';
import { GetVoltageChartRequest } from '@/core/application/dto/reports/requests/GetVoltageChartRequest.ts';

export interface IReportsInterface {
  getLoadChart(
    endpoint: string,
    params: GetLoadChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getVoltageChart(
    endpoint: string,
    params: GetVoltageChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getPowerChart(
    endpoint: string,
    params: GetPowerChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getConsumptionOutputChart(
    endpoint: string,
    params: GetConsumptionOutputChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getFinalizedOutputElectricityChart(
    endpoint: string,
    params: GetFinalizedOutputElectricityChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getListFinalizedOutputElectricity(
    endpoint: string,
    params: GetListFinalizedOutputElectricityRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getHalfHourFinalizedEnergyReport(
    endpoint: string,
    params: GetListHalfHourFinalizedEnergyReportRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getDailyFinalizedEnergyReport(
    endpoint: string,
    params: GetListDailyDailyFinalizedEnergyReportRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getYearlyFinalizedEnergyReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getOperatingParameterReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getOperatingParameterChart(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getEnergyConsumptionReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getActivePointsEnergyReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}

