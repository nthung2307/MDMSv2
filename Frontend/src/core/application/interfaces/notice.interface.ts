import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { GetListNoticeRequest } from '@/core/application/dto/notice/request/GetListNoticeRequest';
import { SendAllMailNoticeRequest } from '@/core/application/dto/notice/request/SendAllMailRequest';

export interface INoticeManagementService {
  getListNoticeAsync(
    endpoint: string,
    params: GetListNoticeRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  sendMailNoticeAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  sendAllMailNoticeAsync(
    endpoint: string,
    params: SendAllMailNoticeRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
