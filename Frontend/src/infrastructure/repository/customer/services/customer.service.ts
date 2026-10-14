import { CancelToken } from 'axios';

import NetworkException from '@/core/application/common/exceptions/networkException.ts';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse.ts';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import SuccessResponse from '@/core/application/dto/common/responses/successResponse.ts';
import { ChangeCustomerStatusRequest } from '@/core/application/dto/customer/request/ChangeCustomerStatusRequest';
import { CreateCustomerRequest } from '@/core/application/dto/customer/request/CreateCustomerRequest';
import { DeleteCustomerRequest } from '@/core/application/dto/customer/request/DeleteCustomerRequest';
import { ExportCustomerCSVRequest } from '@/core/application/dto/customer/request/ExportCustomerCSVRequest';
import { ExportCustomerPDFRequest } from '@/core/application/dto/customer/request/ExportCustomerPDFRequest';
import { GetCustomerDetailRequest } from '@/core/application/dto/customer/request/GetCustomerDetailRequest';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest';
import { UpdateCustomerRequest } from '@/core/application/dto/customer/request/UpdateCustomerRequest';
import { ICustomerManagementService } from '@/core/application/interfaces/customer.interface';
import LoggerService from '@/infrastructure/services/logger.service.ts';
import RequestService from '@/infrastructure/services/request.service.ts';

export class CustomerManagementService implements ICustomerManagementService {
  private readonly loggerService = new LoggerService();

  public async getListCustomer(
    endpoint: string,
    params: GetListCustomerRequest,
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
  public async getCustomerDetail(
    endpoint: string,
    params: GetCustomerDetailRequest,
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
  public async createCustomer(
    endpoint: string,
    params: CreateCustomerRequest,
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

  public async updateCustomer(
    endpoint: string,
    params: UpdateCustomerRequest,
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
  public async deleteCustomer(
    endpoint: string,
    _params: DeleteCustomerRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makeDeleteRequestAsync(endpoint, cancellationToken);

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
  public async changeCustomerStatus(
    endpoint: string,
    _params: ChangeCustomerStatusRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse> {
    try {
      const result = await new RequestService().makePutParamRequestAsync(
        endpoint,
        _params,
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
  public async exportCustomerCSV(
    endpoint: string,
    params: ExportCustomerCSVRequest,
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
  public async exportCustomerPDF(
    endpoint: string,
    params: ExportCustomerPDFRequest,
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

  public async importCustomerCsv(endpoint: string, file: File): Promise<RequestResponse> {
    try {
      return await new RequestService().makeUploadRequestAsync(endpoint, {
        key: 'file',
        file: file,
      });
    } catch (e) {
      this.loggerService.error(e);
      throw e;
    }
  }
}
