import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

export class MeterReplacementService {
  public readonly loggerService = new LoggerService();

  public async getListAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getDetailAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async createAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async updateAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePutRequestAsync(
        endpoint,
        params,
        cancellationToken
      );
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async deleteAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeDeleteRequestAsync(
        endpoint,
        cancellationToken
      );
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async approveAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async cancelAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getPointMeterInfoAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async uploadDocumentAsync(
    endpoint: string,
    file: File
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeUploadRequestAsync(endpoint, {
        key: 'file',
        file: file,
      });
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async extractOcrAsync(
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
      if (result.status === 200) return result as SuccessResponse;
      if (result.status === 202) return result as FailureResponse;
      if (result.status === 400) return result as InvalidModelStateResponse;
      throw new NetworkException('No http status code handler');
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}
