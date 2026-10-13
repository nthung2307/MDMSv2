import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse';
import { RefreshTokenRequest } from '@/core/application/dto/identity/auth/requests/RefreshTokenRequest';
import { CodesMap } from '@/core/domain/enums/CodesMap';
import { notifyError } from '@/infrastructure/common/components/toast/toastMessage';
import { AuthManagementService } from '@/infrastructure/repository/auth/services/auth.service';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import LoggerService from '@/infrastructure/services/logger.service';
import { refactorFormDataCommon } from '@/infrastructure/utils/helpers';

export const refreshTokenAsync = async (params: RefreshTokenRequest) => {
  const loggerService = new LoggerService();
  const localStorageService = new LocalStorageService();
  const response = await new AuthManagementService().refreshTokenAsync(
    Endpoint.Auth.RefreshToken, //endpoint
    refactorFormDataCommon(params)
  );
  let result: any = {};
  if ((response as FailureResponse)?.code !== CodesMap.CANCEL_TOKEN) {
    switch (response.status) {
      case 200: {
        const res = (response as SuccessResponse).data;
        if (res?.success) {
          // let getListRoleToken = getListRole();
          // let listRoleNewToken: any = jwtDecode(res.access_token);
          // if (
          //   getListRoleToken &&
          //   getListRoleToken.length &&
          //   listRoleNewToken &&
          //   listRoleNewToken.length &&
          //   getListRoleToken.toString() !== listRoleNewToken.toString()
          // ) {
          let storage = null;
          const accessToken = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
          const refreshToken = localStorageService.readStorage(Constants.API_REFRESH_TOKEN_STORAGE);
          if (accessToken && refreshToken) {
            storage = {
              accessToken: accessToken,
              refreshToken: refreshToken,
            };
          }
          if (storage) {
            storage.accessToken = res.data.accessToken;
            storage.refreshToken = res.data.refreshToken;
          }
          localStorageService.setStorage(Constants.API_TOKEN_STORAGE, res.data.accessToken);
          localStorageService.setStorage(
            Constants.API_REFRESH_TOKEN_STORAGE,
            res.data.refreshToken
          );
          result = res || {};
          // }
        } else {
          localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
          localStorageService.removeStorage(Constants.API_REFRESH_TOKEN_STORAGE);
          window.location.href = '/';
          notifyError(
            '',
            (res.errors?.length && res.errors[0]?.error) ||
              'An error occurred. Please contact the administrator'
          );
        }

        break;
      }
      case 202: {
        notifyError(
          '',
          (response as FailureResponse).message ||
            'An error occurred. Please contact the administrator'
        );
        const res = (response as SuccessResponse).data;
        if (!res?.success) {
          localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
          window.location.href = '/';
        }
        break;
      }
      case 400: {
        notifyError(
          '',
          (response as InvalidModelStateResponse).message ||
            'An error occurred. Please contact the administrator'
        );
        loggerService.info(response as InvalidModelStateResponse);
        break;
      }
      case 401: {
        localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
        window.location.href = '/';
        break;
      }
      default: {
        notifyError('', 'An error occurred. Please contact the administrator');
      }
    }
    return result;
  }
  return result;
};
