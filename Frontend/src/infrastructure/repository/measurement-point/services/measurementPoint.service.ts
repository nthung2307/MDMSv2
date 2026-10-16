import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import { CreateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/CreateMeasurementPointRequest.ts';
import { DeleteMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/DeleteMeasurementPointRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/GetMeasurementPointRequest.ts';
import { UpdateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/UpdateMeasurementPointRequest.ts';
import { IMeasurementPointService } from '@/core/application/interfaces/methods/measurement-point/measurementPoint.interface.ts';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

export class MeasurementPointService implements IMeasurementPointService {
  public readonly loggerService = new LoggerService();

  public async getListMeasurementPoint(
    endpoint: string,
    params: GetListMeasurementPointsRequest,
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

  public async updateMeasurementPoint(
    endpoint: string,
    params: UpdateMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePutRequestAsync(
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

  public async getMeasurementPointDetail(
    endpoint: string,
    params: GetMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeGetRequestAsync(
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

  public async deleteMeasurementPoint(
    endpoint: string,
    _params: DeleteMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeDeleteRequestAsync(endpoint, cancellationToken);

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

  public async createMeasurementPoint(
    endpoint: string,
    params: CreateMeasurementPointRequest,
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

  public async changeMeasurementPointStatus(
    endpoint: string,
    _params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePutParamRequestAsync(
        endpoint,
        {},
        _params,
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

  public async getCountAlert(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeGetRequestAsync(
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

  public async importMeasurementPointCsv(endpoint: string, file: File): Promise<RequestResponse> {
    try {
      return await new RequestService().makeUploadRequestAsync(endpoint, {
        key: 'file',
        file: file,
      });
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}
