import { CancelToken } from 'axios';

import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';

export interface IAccountManagementService {
  createAccount(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getListAccount(
    endpoint: string,
    params: GetListAccountRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getAccountDetail(
    endpoint: string,
    params: GetAccountDetailRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateAccount(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateStatus(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  deleteAccount(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
