export class ForgotPasswordRequest {
  email: string;

  constructor(args: { email: string }) {
    this.email = args.email;
  }
}
