import { CancelToken } from 'axios';

import Constants from '@/core/application/common/constants';
import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import { CreateGCSRequest } from '@/core/application/dto/gcs/request/CreateGCSRequest';
import { DeleteGCSRequest } from '@/core/application/dto/gcs/request/DeleteGCSRequest';
import { FinalMeterReadingRequest } from '@/core/application/dto/gcs/request/FinalMeterReadingRequest';
import { GCSCutOffCycleRequest } from '@/core/application/dto/gcs/request/GCSCutOffCycleRequest';
import { GCSMonthlyDetailRequest } from '@/core/application/dto/gcs/request/GcsMonthlyDetailRequest';
import { GetGCSDetailRequest } from '@/core/application/dto/gcs/request/GetGCSDetailRequest';
import { GetListGCSRequest } from '@/core/application/dto/gcs/request/GetListGCSRequest';
import { UpdateGCSRequest } from '@/core/application/dto/gcs/request/UpdateGCSRequest';
import { UpdateGCSStatusRequest } from '@/core/application/dto/gcs/request/UpdateGCSStatusRequest.ts';
import { UpdateGCSTemporaryRequest } from '@/core/application/dto/gcs/request/UpdateGCSTemporaryRequest';
import { IGCSManagementService } from '@/core/application/interfaces/gcs.interface';
import axiosInstance from '@/infrastructure/common/libs/axios/interceptors';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

export class GCSManagementService implements IGCSManagementService {
  private readonly loggerService = new LoggerService();

  public async getListGCS(
    endpoint: string,
    params: GetListGCSRequest,
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

  public async getGCSDetail(
    endpoint: string,
    params: GetGCSDetailRequest,
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

  public async createGCS(
    endpoint: string,
    params: CreateGCSRequest,
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

  public async updateGCS(
    endpoint: string,
    params: UpdateGCSRequest,
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

  public async deleteGCS(
    endpoint: string,
    _params: DeleteGCSRequest,
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

  public async updateGCSStatus(
    endpoint: string,
    params: UpdateGCSStatusRequest,
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

  public async updateGCSTemporary(
    endpoint: string,
    params: UpdateGCSTemporaryRequest,
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

  public async FinalMeterReadingAsync(
    endpoint: string,
    params: FinalMeterReadingRequest,
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

  public async GCSCutOffCycleAsync(
    endpoint: string,
    params: GCSCutOffCycleRequest,
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

  public async GCSYearCutOffCycleAsync(
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

  public async GcsMonthlyDetailAsync(
    endpoint: string,
    params: GCSMonthlyDetailRequest,
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

  public async exportExcelTemporary(
    endpoint: string,
    params: {
      gcsId: number;
      month: number;
      year: number;
      periodNumber: number;
      onlySuspicious?: boolean;
    },
    cancellationToken?: CancelToken
  ): Promise<Blob> {
    try {
      const baseURL = `${import.meta.env.VITE_APP_API_URL}`;
      const apiPath = endpoint.replace(/^\//, '');
      const queryParams: Record<string, string> = {
        gcsId: params.gcsId.toString(),
        month: params.month.toString(),
        year: params.year.toString(),
        periodNumber: params.periodNumber.toString(),
      };
      if (params.onlySuspicious !== undefined) {
        queryParams['onlySuspicious'] = params.onlySuspicious.toString();
      }
      const query = new URLSearchParams(queryParams).toString();

      const url = `${baseURL}/${apiPath}?${query}`;
      const token = new LocalStorageService().readStorage(Constants.API_TOKEN_STORAGE);
      const headers: Record<string, string> = {};
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await axiosInstance.get(url, {
        headers,
        responseType: 'blob',
        cancelToken: cancellationToken,
      });

      return response.data;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async importExcelTemporary(
    endpoint: string,
    file: File,
    params: {
      gcsId: number;
      month: number;
      year: number;
      periodNumber: number;
    }
  ): Promise<RequestResponse> {
    try {
      const query = new URLSearchParams({
        gcsId: params.gcsId.toString(),
        month: params.month.toString(),
        year: params.year.toString(),
        periodNumber: params.periodNumber.toString(),
      }).toString();
      const url = `${endpoint}?${query}`;
      return await new RequestService().makeUploadRequestAsync(url, {
        key: 'file',
        file: file,
      });
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}
