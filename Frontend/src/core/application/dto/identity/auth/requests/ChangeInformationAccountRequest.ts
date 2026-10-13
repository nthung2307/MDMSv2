export class ChangeInformationAccountRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  description: string;

  constructor(args: { fullName: string; email: string; phoneNumber: string; description: string }) {
    this.fullName = args.fullName;
    this.email = args.email;
    this.phoneNumber = args.phoneNumber;
    this.description = args.description;
  }
}
