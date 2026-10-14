import { AxiosResponse, CancelToken, isCancel } from 'axios';

import { LoadingState } from '@/core/application/common/atoms/global/loadingState';
import Constants from '@/core/application/common/constants';
import {
  ParamsUploadFileEntity,
  ParamsUploadListFileEntity,
} from '@/core/application/common/entities/params.entity';
import NetworkException from '@/core/application/common/exceptions/networkException';
import Failure500Response from '@/core/application/dto/common/responses/failure500Response';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse';
import { IRequestService } from '@/core/application/interfaces/request.interface';
import { CodesMap } from '@/core/domain/enums/CodesMap';
import axiosInstance from '@/infrastructure/common/libs/axios/interceptors';
import { setRecoilStateAsync } from '@/infrastructure/common/libs/recoil-outside/recoil.service';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import LoggerService from '@/infrastructure/services/logger.service';
import { acceptFile } from '@/infrastructure/utils/files';

export default class RequestService implements IRequestService {
  private readonly loggerService = new LoggerService();
  private readonly baseURL = `${import.meta.env.VITE_APP_API_URL}`;
  private readonly localStorageService = new LocalStorageService();
  private readonly errorCancel: FailureResponse = {
    message: 'cancel token',
    code: CodesMap.CANCEL_TOKEN,
    success: false,
    status: 202,
  };

  private getOptions(file: boolean = false) {
    // let token = context?.token || "";
    let token = this.localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
    let opts: any = {
      headers: {
        'Content-Type': 'application/json',
      },
    };
    if (token) {
      opts = {
        ...opts,
        headers: {
          ...opts.headers,
          Authorization: `Bearer ${token}`,
        },
      };
    }

    if (file) {
      opts.responseType = 'blob';
    }

    return opts;
  }

  private extractErrorMessage(e: any): string {
    if (e.response?.data?.errors?.length && e.response.data.errors[0]?.error) {
      return e.response.data.errors[0].error;
    }
    if (e.response?.data?.message) {
      return Array.isArray(e.response.data.message)
        ? e.response.data.message[0]
        : e.response.data.message;
    }
    if (e.response?.data?.title) {
      return e.response.data.title;
    }
    return e.message;
  }

  /**
   * Convert AxiosResponse to app request
   * @param response
   */
  private processRequest(response: AxiosResponse): RequestResponse {
    try {
      if (response.status === 200) {
        return new SuccessResponse(response.statusText, response.data);
      }
      if (response.status === 201) {
        return new SuccessResponse(response.statusText, response.data);
      }
      if (response.status === 202) {
        return new FailureResponse(response.data);
      }
      if (response.status === 400) {
        return new InvalidModelStateResponse(response.data);
      }
      throw new NetworkException('No http status code handler');
    } catch (e: any) {
      new LoggerService().error(`[ERROR] ${e}`);
      throw e;
    }
  }

  async makeGetRequestAsync(
    endpoint: string,
    params: any,
    cancellationToken?: CancelToken
  ): Promise<RequestResponse> {
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })
      return this.processRequest(
        await axiosInstance.get(_url, {
          ...this.getOptions(),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      if (e.response.status === 500) {
        return new Failure500Response({
          ...e,
          message: this.extractErrorMessage(e),
        });
      }
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makePostRequestAsync(
    endpoint: string,
    requestBody: any,
    cancellationToken?: CancelToken,
    params?: any
  ): Promise<RequestResponse> {
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })
      const _requestBody = JSON.stringify(requestBody);

      return this.processRequest(
        await axiosInstance.post(_url, _requestBody, {
          ...this.getOptions(),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      if (e.response?.status === 500) {
        return new Failure500Response({
          ...e,
          message: this.extractErrorMessage(e),
        });
      }
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makeGetFileRequestAsync(endpoint: string, params: any): Promise<RequestResponse> {
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      let option = this.getOptions(true);
      option.headers['Accept'] = acceptFile(endpoint, 'pdf');

      // const _params = params ? Object.keys(params).map(key => `${key}=${encodeURIComponent(params[key])}`).join("&") : "";
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })
      return this.processRequest(await axiosInstance.get(_url, { ...option }));
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      return new FailureResponse({
        ...e,
        message: e.response?.data?.message || e.message,
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makePostExportRequestAsync(
    endpoint: string,
    requestBody: any,
    cancellationToken?: CancelToken,
    params?: any
  ): Promise<RequestResponse> {
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })
      const _requestBody = JSON.stringify(requestBody);

      return this.processRequest(
        await axiosInstance.post(_url, _requestBody, {
          ...this.getOptions(true),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      if (e.response?.status === 500) {
        return new Failure500Response({
          ...e,
          message: this.extractErrorMessage(e),
        });
      }
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makeUploadRequestAsync(
    endpoint: string,
    params: ParamsUploadFileEntity | ParamsUploadListFileEntity
  ): Promise<RequestResponse> {
    try {
      const _url = `${this.baseURL}/${endpoint}`;
      await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url });
      const _form = new FormData();
      if (Array.isArray(params.file)) {
        params.file.forEach((file: File) => {
          _form.append(params.key, file);
        });
      } else {
        _form.append(params.key, params.file);
      }
      const _options = this.getOptions();
      delete _options.headers['Content-Type'];
      return this.processRequest(await axiosInstance.post(_url, _form, _options));
    } catch (e: any) {
      this.loggerService.error(`[ERROR] ${e}`);
      // throw e;
      // let result = (e as Error).message;
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });

      // TODO: Need to check this
      //return new FailureResponse(result || e.errors);
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makePostParamRequestAsync(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    //const setIsLoading = useSetRecoilState(LoadingState);
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })
      // const _params = JSON.stringify(params);
      return this.processRequest(
        await axiosInstance.post(_url, _params, {
          ...this.getOptions(true),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }

      // throw e;
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  //TODO:PUT
  async makePutRequestAsync(
    endpoint: string,
    requestBody: object,
    cancellationToken?: CancelToken,
    params?: any
  ): Promise<RequestResponse> {
    //const setIsLoading = useSetRecoilState(LoadingState);
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })

      const _requestBody = JSON.stringify(requestBody);
      return this.processRequest(
        await axiosInstance.put(_url, _requestBody, {
          ...this.getOptions(),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      return new FailureResponse({
        ...e,
        message: this.extractErrorMessage(e),
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  async makePutParamRequestAsync(
    endpoint: string,
    params: any,
    requestBody: object,
    cancellationToken?: CancelToken
  ): Promise<RequestResponse> {
    try {
      // Convert params to a query string
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';

      // Construct the URL with query string if params exist
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;

      // Convert the request body to JSON string
      const _requestBody = JSON.stringify(requestBody);

      // Make the PUT request with the provided options, including the cancellation token
      return this.processRequest(
        await axiosInstance.put(_url, _requestBody, {
          ...this.getOptions(),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        // Handle request cancellation
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        // Log any other errors
        this.loggerService.error(`[ERROR] ${e}`);
      }

      // Return a failure response with the error message
      return new FailureResponse({
        ...e,
        message: e.response?.data?.message || e.message,
      });
    }
  }

  async makePatchRequestAsync(
    endpoint: string,
    params: any,
    requestBody: object,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    //const setIsLoading = useSetRecoilState(LoadingState);
    try {
      const _params = params
        ? Object.keys(params)
            .map((key) => `${key}=${encodeURIComponent(params[key])}`)
            .join('&')
        : '';
      const _url = `${this.baseURL}/${endpoint}${_params === '' ? '' : '?' + _params}`;
      // await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url })

      const _requestBody = JSON.stringify(requestBody);
      return this.processRequest(
        await axiosInstance.patch(_url, _requestBody, {
          ...this.getOptions(),
          cancelToken: cancellationToken,
        })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      return new FailureResponse({
        ...e,
        message: e.response?.data?.message || e.message,
      });
    }
    // finally {
    //   await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' })
    // }
  }

  //TODO:DELETE
  async makeDeleteRequestAsync(
    endpoint: string,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    //const setIsLoading = useSetRecoilState(LoadingState);
    try {
      const _url = `${this.baseURL}/${endpoint}`;
      await setRecoilStateAsync(LoadingState, { isLoading: true, uri: _url });

      // const _params = JSON.stringify(params);
      return this.processRequest(
        await axiosInstance.delete(_url, { ...this.getOptions(), cancelToken: cancellationToken })
      );
    } catch (e: any) {
      if (isCancel(e)) {
        this.loggerService.error(`[ERROR]: ${endpoint} --- canceltoken`);
        return new FailureResponse(this.errorCancel);
      } else {
        this.loggerService.error(`[ERROR] ${e}`);
      }
      // throw e;
      return new FailureResponse({
        ...e,
        message: e.response?.data?.errors?.length
          ? e.response.data.errors[0]?.error || e.message
          : e.message,
      });
    } finally {
      await setRecoilStateAsync(LoadingState, { isLoading: false, uri: '' });
    }
  }
}
