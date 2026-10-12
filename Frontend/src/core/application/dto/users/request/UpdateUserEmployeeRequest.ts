import Constants from '@/core/application/common/constants';

export class UpdateUserEmployeeRequest {
  id: number;
  constructor(init?: Partial<UpdateUserEmployeeRequest>) {
    this.id = init?.id ?? 0;
  }
}

export class UpdateUserEmployeeBodyRequest {
  phoneNumber: string | null;
  email: string | null;
  status: number;
  name: string | null;
  avatar: string | null;
  gender: number | null;
  dob: Date | null; // or Date if you handle conversion
  address: string | null;
  email_company: string | null;
  jobPosition: string | null;
  code: string | null;
  roleId: number | null;

  constructor(init?: Partial<UpdateUserEmployeeBodyRequest>) {
    this.phoneNumber = init?.phoneNumber ?? null;
    this.email = init?.email ?? null;
    this.status = init?.status ?? Constants.UserStatusConfig.Active.value;
    this.name = init?.name ?? null;
    this.avatar = init?.avatar ?? null;
    this.gender = init?.gender ?? null;
    this.dob = init?.dob ?? null;
    this.address = init?.address ?? null;
    this.email_company = init?.email_company ?? null;
    this.jobPosition = init?.jobPosition ?? null;
    this.code = init?.code ?? null;
    this.roleId = init?.roleId ?? null;
  }
}
