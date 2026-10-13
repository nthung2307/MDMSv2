import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export default class SuccessResponse extends RequestResponse {
  status: number = 200;
  message: string = '';
  data: any;

  constructor(message: string, data: any) {
    super(200);
    this.message = message;
    this.data = data;
  }
}

export class DataResponse extends RequestResponse {
  status: number = 200;
  message: string = '';
  data: any;
  success: boolean = true;

  constructor(message: string, data: any) {
    super(200);
    this.message = message;
    this.data = data;
  }
}
