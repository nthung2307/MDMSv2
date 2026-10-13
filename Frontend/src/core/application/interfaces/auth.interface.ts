import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';
import { ChangeInformationAccountRequest } from '@/core/application/dto/identity/auth/requests/ChangeInformationAccountRequest.ts';
import { ChangePasswordRequest } from '@/core/application/dto/identity/auth/requests/ChangePassword.ts';
import { CheckTokenForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/CheckTokenForgotPasswordRequest.ts';
import { LoginRequest } from '@/core/application/dto/identity/auth/requests/LoginRequest';
import { LogoutRequest } from '@/core/application/dto/identity/auth/requests/LogoutRequest';
import { RefreshTokenRequest } from '@/core/application/dto/identity/auth/requests/RefreshTokenRequest';

export interface IAuthManagementService {
  /**
   * Make get request to the endpoint
   * @param endpoint
   * @param params
   *  Build request configurations including content-type, authorization header, cancellation token
   * @param cancellationToken: Token to cancel request if needed
   */
  loginAsync(
    endpoint: string,
    params: LoginRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  refreshTokenAsync(
    endpoint: string,
    params: RefreshTokenRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  logoutAsync(
    endpoint: string,
    params: LogoutRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  forgotPasswordAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  confirmForgotPasswordAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  changeInformationAccount(
    endpoint: string,
    params: ChangeInformationAccountRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  changePassword(
    endpoint: string,
    params: ChangePasswordRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  checkTokenForgotPassword(
    endpoint: string,
    params: CheckTokenForgotPasswordRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
