import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export default class FailureResponse extends RequestResponse {
  status: number = 202;
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
