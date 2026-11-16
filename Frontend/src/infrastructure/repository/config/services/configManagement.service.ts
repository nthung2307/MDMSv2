import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { tagNameListRequest } from '@/core/application/dto/tag-name/requests/TagNameListRequest';
import { IConfigManagementService } from '@/core/application/interfaces/methods/tag-name/configManagement.interface';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

const configLogger = new LoggerService();

async function dispatchConfigRequestAsync(
  executor: () => Promise<RequestResponse>
): Promise<RequestResponse> {
  try {
    const result = await executor();

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
    configLogger.error(e);
    throw e;
  }
}

export class ConfigManagementService implements IConfigManagementService {
  public readonly loggerService = new LoggerService();

  public async getTagNameListAsync(
    endpoint: string,
    params: tagNameListRequest,
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

  public async createTagNameAsync(
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

  public async updateTagNameListAsync(
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

  public async getSystemListAsync(
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

  public async createSystemConfigAsync(
    endpoint: string,
    params: systemConfigRequest,
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

  public async getActiveEmailConfigAsync(
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

  public async updateEmailConfigAsync(
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

  public async testEmailConnectionAsync(
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

  public async getIoTConfigAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makeGetRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async updateIoTConfigAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makePostRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async generateIoTKeyAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makePostRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async getMeterProfilesAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makeGetRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async getUnmatchedSerialsAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makeGetRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async getPointApiKeyAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makeGetRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async generatePointApiKeyAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makePostRequestAsync(endpoint, params, cancellationToken)
    );
  }

  public async revokePointApiKeyAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    return dispatchConfigRequestAsync(() =>
      new RequestService().makePostRequestAsync(endpoint, params, cancellationToken)
    );
  }
}
