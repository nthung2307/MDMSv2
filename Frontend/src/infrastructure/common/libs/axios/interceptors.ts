import axios, {
  AxiosError,
  AxiosRequestConfig,
  AxiosResponse,
  AxiosStatic,
  InternalAxiosRequestConfig,
} from 'axios';
// import jwtDecode from 'jwt-decode';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint';
import { RefreshTokenRequest } from '@/core/application/dto/identity/auth/requests/RefreshTokenRequest';
import { refreshTokenAsync } from '@/infrastructure/repository/auth/hooks/useRefreshToken.hook';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import LoggerService from '@/infrastructure/services/logger.service';

// Request Interceptor
let loggerService = new LoggerService();
let axiosInstance: AxiosStatic = axios;
let isRefreshing = false;
let requests: any = [];

const onRequest = (config: InternalAxiosRequestConfig | any): InternalAxiosRequestConfig => {
  let localStorageService = new LocalStorageService();
  let storage: any = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
  // Set Headers Here
  // Check Authentication Here
  // Set Loading Start Here

  if (storage) {
    if (config.headers && typeof config.headers.set === 'function') {
      config.headers.set('Authorization', `Bearer ${storage}`);
    } else {
      config.headers = {
        ...config.headers,
        Authorization: `Bearer ${storage}`,
      };
    }
  }
  return config;
};

const onResponse = (response: AxiosResponse): AxiosResponse => {
  // Set Loading End Here
  // Handle Response Data Here
  // Error Handling When Return Success with Error Code Here
  // loggerService.trace(`ðŸš€ [API] ${method?.toUpperCase()} ${url} | Response ${status}`);

  return response;
};

// function refreshToken() {
//     return instance.post("/auth/refreshtoken", {
//       refreshToken: getLocalRefreshToken(),
//     });
//     return {data: {
//         accessToken: null
//     }}
// }

let localStorageService = new LocalStorageService();
// let storage = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);

// axiosRetry(axiosInstance, {
//   retries: 3, // number of retries
//   retryDelay: (retryCount) => {
//     return retryCount * 2000 // time interval between retries
//   },
//   retryCondition: (error: any) => {
//     // if retry condition is not specified, by default idempotent requests are retried
//     return error.code === 'ERR_NETWORK'
//     // return error.response?.status === 504 || error.response?.status === 502;
//   },
// })

const onErrorResponse = async (error: AxiosError | Error | any): Promise<unknown> => {
  if (axiosInstance.isCancel(error)) {
    return Promise.reject(error);
  }

  if (axiosInstance.isAxiosError(error)) {
    const { message } = error;

    const { method, url } = error.config as AxiosRequestConfig;
    const { status } = (error.response as AxiosResponse) ?? {};

    loggerService.error(`ðŸš¨ [API] ${method?.toUpperCase()} ${url} | Error ${status} ${message}`);

    switch (status) {
      case 401: {
        // "Login required" || "Refresh Token"
        const originalRequest: any = error.config;
        if (originalRequest.url.includes(Endpoint.Auth.RefreshToken)) {
          // Refresh token failed
          // Redirect to login page
          localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
          localStorageService.removeStorage(Constants.API_REFRESH_TOKEN_STORAGE);
          window.location.href = '/';
          return Promise.reject(error);
        }
        try {
          const localStorageService = new LocalStorageService();
          const token = localStorageService.readStorage(Constants.API_REFRESH_TOKEN_STORAGE);
          if (token && !originalRequest._retry) {
            originalRequest._retry = true;

            if (!isRefreshing) {
              isRefreshing = true;
              // const info: any = jwtDecode(token.idToken);
              // refreshTokenRequest = refreshTokenRequest ? refreshTokenRequest :
              const result = await refreshTokenAsync(
                new RefreshTokenRequest({
                  refreshToken: token,
                })
              ).finally(() => {
                isRefreshing = false;
              });
              // Add type assertion here
              if (result?.success) {
                if (originalRequest.headers) {
                  originalRequest.headers.Authorization = `Bearer ${result.data.accessToken}`;
                }
                requests.forEach((cb: any) => cb(result.idToken));
                requests = [];
                return axiosInstance(originalRequest);
              }
            } else {
              return new Promise((resolve) => {
                requests.push((newToken: string) => {
                  // Rename the parameter 'token' to 'newToken'
                  if (originalRequest.headers) {
                    originalRequest.headers.Authorization = `Bearer ${newToken}`; // Update the reference to the renamed parameter
                  }
                  resolve(axiosInstance(originalRequest));
                });
              });
            }
          }
        } catch (_error: any) {
          if (_error.response && _error.response.data) {
            return Promise.reject(_error.response.data);
          }

          return Promise.reject(_error);
        }
        break;
      }
      case 403: {
        // "Permission denied"
        break;
      }
      case 404: {
        // "Invalid request"
        break;
      }
      case 500: {
        break;
      }
      case 502: {
        break;
      }
      case 504: {
        break;
      }
      default: {
        // "Unknown error occurred"
        break;
      }
    }
  } else {
    loggerService.error(`ðŸš¨ [API] | Error ${error.message}`);
  }

  return Promise.reject(error);
};

const setupInterceptors = () => {
  axiosInstance.interceptors.request.use(onRequest, onErrorResponse);
  axiosInstance.interceptors.response.use(onResponse, onErrorResponse);

  return axiosInstance;
};
setupInterceptors();
export default axiosInstance;
