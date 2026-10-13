export class CreateCustomerRequest {
  customerCode: string;
  fullName: string;
  phoneNumber: string;
  address: string;
  email: string;
  email2: string;
  email3: string;
  userIds: number[];
  status: string;

  constructor(args: CreateCustomerRequest) {
    this.customerCode = args.customerCode;
    this.fullName = args.fullName;
    this.phoneNumber = args.phoneNumber;
    this.address = args.address;
    this.email = args.email;
    this.email2 = args.email2;
    this.email3 = args.email3;
    this.userIds = args.userIds;
    this.status = args.status;
  }
}
