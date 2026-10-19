import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import { GetConsumptionOutputChartRequest } from '@/core/application/dto/reports/requests/GetConsumptionOutputChartRequest.ts';
import { GetListDailyDailyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetDailyFinalizedEnergyReportRequest';
import { GetFinalizedOutputElectricityChartRequest } from '@/core/application/dto/reports/requests/GetFinalizedOutputElectricityChartRequest.ts';
import { GetListFinalizedOutputElectricityRequest } from '@/core/application/dto/reports/requests/GetListFinalizedOutputElectricityRequest.ts';
import { GetListHalfHourFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetListHalfHourFinalizedEnergyReportRequest';
import { GetLoadChartRequest } from '@/core/application/dto/reports/requests/GetLoadChartRequest.ts';
import { GetPowerChartRequest } from '@/core/application/dto/reports/requests/GetPowerChartRequest.ts';
import { GetVoltageChartRequest } from '@/core/application/dto/reports/requests/GetVoltageChartRequest.ts';
import { IReportsInterface } from '@/core/application/interfaces/methods/reports/reports.interface.ts';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

export class ReportsService implements IReportsInterface {
  public readonly loggerService = new LoggerService();

  public async getConsumptionOutputChart(
    endpoint: string,
    params: GetConsumptionOutputChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }

      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getFinalizedOutputElectricityChart(
    endpoint: string,
    params: GetFinalizedOutputElectricityChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getListFinalizedOutputElectricity(
    endpoint: string,
    params: GetListFinalizedOutputElectricityRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }

      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getLoadChart(
    endpoint: string,
    params: GetLoadChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }

      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getPowerChart(
    endpoint: string,
    params: GetPowerChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }

      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getVoltageChart(
    endpoint: string,
    params: GetVoltageChartRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );

      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }

      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getHalfHourFinalizedEnergyReport(
    endpoint: string,
    params: GetListHalfHourFinalizedEnergyReportRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getDailyFinalizedEnergyReport(
    endpoint: string,
    params: GetListDailyDailyFinalizedEnergyReportRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getYearlyFinalizedEnergyReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getOperatingParameterReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getOperatingParameterChart(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getEnergyConsumptionReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getActivePointsEnergyReport(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePostRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) {
        return result as SuccessResponse;
      }
      if (result.status === 202) {
        return result as FailureResponse;
      }
      if (result.status === 400) {
        return result as InvalidModelStateResponse;
      }
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}

