export class CheckTokenForgotPasswordRequest {
  token: string;

  constructor(args: { token: string }) {
    this.token = args.token;
  }
}
