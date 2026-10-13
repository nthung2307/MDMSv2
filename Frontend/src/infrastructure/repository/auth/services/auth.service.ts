import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse';
import { ChangeInformationAccountRequest } from '@/core/application/dto/identity/auth/requests/ChangeInformationAccountRequest.ts';
import { ChangePasswordRequest } from '@/core/application/dto/identity/auth/requests/ChangePassword.ts';
import { CheckTokenForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/CheckTokenForgotPasswordRequest.ts';
import { ConfirmForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/ConfirmForgotPasswordRequest.ts';
import { ForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/ForgotPasswordRequest.ts';
import { LoginRequest } from '@/core/application/dto/identity/auth/requests/LoginRequest';
import { LogoutRequest } from '@/core/application/dto/identity/auth/requests/LogoutRequest';
import { RefreshTokenRequest } from '@/core/application/dto/identity/auth/requests/RefreshTokenRequest';
import { IAuthManagementService } from '@/core/application/interfaces/auth.interface';
import LoggerService from '@/infrastructure/services/logger.service';
import RequestService from '@/infrastructure/services/request.service';

export class AuthManagementService implements IAuthManagementService {
  private readonly loggerService = new LoggerService();

  public async loginAsync(
    endpoint: string,
    params: LoginRequest,
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

  public async refreshTokenAsync(
    endpoint: string,
    params: RefreshTokenRequest,
    cancellationToken?: CancelToken
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

  public async logoutAsync(
    endpoint: string,
    params: LogoutRequest,
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

  public async forgotPasswordAsync(
    endpoint: string,
    params: ForgotPasswordRequest,
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

  public async confirmForgotPasswordAsync(
    endpoint: string,
    params: ConfirmForgotPasswordRequest,
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

  public async changeInformationAccount(
    endpoint: string,
    params: ChangeInformationAccountRequest,
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

  public async changePassword(
    endpoint: string,
    params: ChangePasswordRequest,
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

  public async checkTokenForgotPassword(
    endpoint: string,
    params: CheckTokenForgotPasswordRequest,
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
}
