import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { tagNameListRequest } from '@/core/application/dto/tag-name/requests/TagNameListRequest';

export interface IConfigManagementService {
  getTagNameListAsync(
    endpoint: string,
    params: tagNameListRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  createTagNameAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateTagNameListAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getSystemListAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  createSystemConfigAsync(
    endpoint: string,
    params: systemConfigRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
