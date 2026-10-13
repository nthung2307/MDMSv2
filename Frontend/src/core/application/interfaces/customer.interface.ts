import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { ChangeCustomerStatusRequest } from '@/core/application/dto/customer/request/ChangeCustomerStatusRequest';
import { DeleteCustomerRequest } from '@/core/application/dto/customer/request/DeleteCustomerRequest';
import { ExportCustomerCSVRequest } from '@/core/application/dto/customer/request/ExportCustomerCSVRequest';
import { ExportCustomerPDFRequest } from '@/core/application/dto/customer/request/ExportCustomerPDFRequest';
import { GetCustomerDetailRequest } from '@/core/application/dto/customer/request/GetCustomerDetailRequest';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest';
import { UpdateCustomerRequest } from '@/core/application/dto/customer/request/UpdateCustomerRequest';

export interface ICustomerManagementService {
  createCustomer(
    endpoint: string,
    params: any,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getListCustomer(
    endpoint: string,
    params: GetListCustomerRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getCustomerDetail(
    endpoint: string,
    params: GetCustomerDetailRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateCustomer(
    endpoint: string,
    params: UpdateCustomerRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  changeCustomerStatus(
    endpoint: string,
    params: ChangeCustomerStatusRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  deleteCustomer(
    endpoint: string,
    params: DeleteCustomerRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
  exportCustomerCSV(
    endpoint: string,
    params: ExportCustomerCSVRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
  exportCustomerPDF(
    endpoint: string,
    params: ExportCustomerPDFRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
