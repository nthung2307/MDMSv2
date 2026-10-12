import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export interface IFilesManagementService {
  /**
   * Make get request to the endpoint
   * @param url
   * @param params
   */

  getFileAsync(url: string, params: any): Promise<RequestResponse>;

  uploadFileAsync(url: string, file: File): Promise<RequestResponse>;
}
