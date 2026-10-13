export interface ICustomerListResponseInterface {
  hasNext: boolean;
  hasPrevious: boolean;
  data: ICustomerDetailResponseInterface[];
}

export interface ICustomerDetailResponseInterface {
  id: number;
  userName: string;
  roleName: string;
  fullName: string;
  phoneNumber: string;
  customerCode: string;
  address: string;
  email: string;
  status: string;
  measurePoints?: any[];
  userCustomer?: any[];
}
