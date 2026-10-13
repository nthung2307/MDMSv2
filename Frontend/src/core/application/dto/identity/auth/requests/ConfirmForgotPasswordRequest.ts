export class ConfirmForgotPasswordRequest {
  token: string;
  email: string;
  password: string;
  confirmPassword: string;

  constructor(args: { token: string; email: string; password: string; confirmPassword: string }) {
    this.token = args.token;
    this.email = args.email;
    this.password = args.password;
    this.confirmPassword = args.confirmPassword;
  }
}
