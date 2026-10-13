import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';

export interface IExportManagementService {
  exportListPdf(
    endpoint: string,
    params: any,
    requestBody: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  exportListCsv(
    endpoint: string,
    params: any,
    requestBody: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
