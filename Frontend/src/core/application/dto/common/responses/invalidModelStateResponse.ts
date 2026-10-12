import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export default class InvalidModelStateResponse extends RequestResponse {
  status: number = 400;
  code: string;
  message: any;
  success: boolean;

  constructor(args: { code: string; message: any; success: boolean }) {
    super(202);
    this.code = args.code;
    this.message = args.message;
    this.success = args.success;
  }
}
