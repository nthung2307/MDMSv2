export class UpdateCustomerRequest {
  id: number;
  customerCode: string;
  fullName: string;
  phoneNumber: string;
  address: string;
  email: string;
  status: string;
  userIds: number[];
  measurementPointCustomers?:
    | [
        {
          measurementPointId: number;
          email: string;
          email2: string;
          email3: string;
        },
      ]
    | [];

  constructor(args: {
    id: number;
    customerCode: string;
    fullName: string;
    phoneNumber: string;
    address: string;
    email: string;
    status: string;
    userIds: number[];
    measurementPointCustomers?:
      | [
          {
            measurementPointId: number;
            email: string;
            email2: string;
            email3: string;
          },
        ]
      | [];
  }) {
    this.id = args.id;
    this.customerCode = args.customerCode;
    this.fullName = args.fullName;
    this.phoneNumber = args.phoneNumber;
    this.address = args.address;
    this.email = args.email;
    this.status = args.status;
    this.userIds = args.userIds;
    this.measurementPointCustomers = args.measurementPointCustomers;
  }
}
