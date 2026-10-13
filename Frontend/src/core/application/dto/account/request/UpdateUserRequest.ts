export class UpdateUserRequest {
  id: number;
  userName: string;
  roleName: string;
  fullName: string;
  phoneNumber: string;
  customerCode: string;
  address: string;
  email: string;
  status: string;
  description: string;

  constructor(args: {
    id: number;
    roleName: string;
    userName: string;
    fullName: string;
    phoneNumber: string;
    address: string;
    email: string;
    status: string;
    customerCode: string;
    description: string;
  }) {
    this.id = args.id;
    this.roleName = args.roleName;
    this.userName = args.userName;
    this.fullName = args.fullName;
    this.phoneNumber = args.phoneNumber;
    this.address = args.address;
    this.email = args.email;
    this.status = args.status;
    this.customerCode = args.customerCode;
    this.description = args.description;
  }
}
