export class ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;

  constructor(args: { oldPassword: string; newPassword: string; confirmPassword: string }) {
    this.oldPassword = args.oldPassword;
    this.newPassword = args.newPassword;
    this.confirmPassword = args.confirmPassword;
  }
}
