import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export interface IRequestService {
  /**
   * Build request configurations including content-type, authorization header, cancellation token
   * @param cancellationToken: Token to cancel request if needed
   */
  /**
   * Make POST request to the endpoint
   * @param endpoint
   * @param params
   * @param cancellationToken
   */

  makeGetRequestAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  makePostRequestAsync(
    endpoint: string,
    params: object,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  makeUploadRequestAsync(endpoint: string, params: any, file: File): Promise<RequestResponse>;

  makePutRequestAsync(
    endpoint: string,
    requestBody: object,
    cancellationToken?: CancelToken,
    params?: any
  ): Promise<RequestResponse>;

  makePatchRequestAsync(
    endpoint: string,
    params: any,
    requestBody: object,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  makeDeleteRequestAsync(endpoint: string, params: object): Promise<RequestResponse>;
}
