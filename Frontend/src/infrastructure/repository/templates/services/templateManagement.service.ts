import { CancelToken } from 'axios';

import { Endpoint } from '@/core/application/common/endPoint';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse';
import {
  CreateNotificationTemplateRequest,
  CreateStatementPdfTemplateRequest,
  PreviewNotificationTemplateRequest,
  PreviewStatementPdfTemplateRequest,
  TestSendNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
  UpdateStatementPdfTemplateRequest,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import axiosInstance from '@/infrastructure/common/libs/axios/interceptors';
import Constants from '@/core/application/common/constants';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import LoggerService from '@/infrastructure/services/logger.service';
import RequestService from '@/infrastructure/services/request.service';

export class TemplateManagementService {
  private readonly loggerService = new LoggerService();
  private readonly requestService = new RequestService();
  private readonly localStorageService = new LocalStorageService();
  private readonly baseURL = `${import.meta.env.VITE_APP_API_URL || ''}`;

  // ==================== NOTIFICATION TEMPLATES ====================

  public async getNotificationTemplates(code?: string, cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.GetNotificationTemplates;
      const params = code ? { code } : undefined;
      const res = await this.requestService.makeGetRequestAsync(endpoint, params, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getNotificationTemplateDetail(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.GetNotificationTemplateDetail}/${id}`;
      const res = await this.requestService.makeGetRequestAsync(endpoint, undefined, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async createNotificationTemplate(data: CreateNotificationTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.CreateNotificationTemplate;
      const res = await this.requestService.makePostRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async updateNotificationTemplate(id: number | string, data: UpdateNotificationTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.UpdateNotificationTemplate}/${id}`;
      const res = await this.requestService.makePutRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async deleteNotificationTemplate(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.DeleteNotificationTemplate}/${id}`;
      const res = await this.requestService.makeDeleteRequestAsync(endpoint, undefined, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async setDefaultNotificationTemplate(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.SetDefaultNotificationTemplate}/${id}`;
      const res = await this.requestService.makePostRequestAsync(endpoint, {}, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async previewNotificationTemplate(data: PreviewNotificationTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.PreviewNotificationTemplate;
      const res = await this.requestService.makePostRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async testSendNotificationTemplate(data: TestSendNotificationTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.TestSendNotificationTemplate;
      const res = await this.requestService.makePostRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  // ==================== STATEMENT PDF TEMPLATES ====================

  public async getStatementPdfTemplates(cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.GetStatementPdfTemplates;
      const res = await this.requestService.makeGetRequestAsync(endpoint, undefined, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async getStatementPdfTemplateDetail(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.GetStatementPdfTemplateDetail}/${id}`;
      const res = await this.requestService.makeGetRequestAsync(endpoint, undefined, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async createStatementPdfTemplate(data: CreateStatementPdfTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = Endpoint.Config.CreateStatementPdfTemplate;
      const res = await this.requestService.makePostRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async updateStatementPdfTemplate(id: number | string, data: UpdateStatementPdfTemplateRequest, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.UpdateStatementPdfTemplate}/${id}`;
      const res = await this.requestService.makePutRequestAsync(endpoint, data, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async deleteStatementPdfTemplate(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.DeleteStatementPdfTemplate}/${id}`;
      const res = await this.requestService.makeDeleteRequestAsync(endpoint, undefined, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async setDefaultStatementPdfTemplate(id: number | string, cancellationToken?: CancelToken) {
    try {
      const endpoint = `${Endpoint.Config.SetDefaultStatementPdfTemplate}/${id}`;
      const res = await this.requestService.makePostRequestAsync(endpoint, {}, cancellationToken);
      return res;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }

  public async previewStatementPdfTemplate(data: PreviewStatementPdfTemplateRequest, cancellationToken?: CancelToken): Promise<Blob> {
    try {
      const apiPath = Endpoint.Config.PreviewStatementPdfTemplate.replace(/^\//, '');
      const url = this.baseURL ? `${this.baseURL}/${apiPath}` : `/${apiPath}`;
      const token = this.localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await axiosInstance.post(url, data, {
        headers,
        responseType: 'blob',
        cancelToken: cancellationToken,
      });

      return response.data;
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}
