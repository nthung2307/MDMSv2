export interface IAccountListResponseInterface {
  hasNext: boolean;
  hasPrevious: boolean;
  data: [
    {
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
    },
  ];
}

export interface IAccountDetailResponseInterface {
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
}
