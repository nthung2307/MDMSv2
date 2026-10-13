import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse';

export default class Failure500Response extends RequestResponse {
  status: number = 500;
  code: string;
  message: any;
  success: boolean;

  constructor(args: { code: string; message: any; success: boolean }) {
    super(500);
    this.code = args.code;
    this.message = args.message;
    this.success = args.success;
  }
}
