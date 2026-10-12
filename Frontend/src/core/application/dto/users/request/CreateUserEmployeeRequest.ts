import Constants from '@/core/application/common/constants';

export class CreateUserEmployeeRequest {
  status: number | null;
  name: string | null;
  avatar: string | null;
  phoneNumber: string | null;
  email: string | null;
  gender: number | null;
  dob: string | null; // or Date if you handle conversion
  address: string | null;
  code: string | null;
  emailCompany: string | null;
  jobPosition: string | null;
  roleId: number;

  constructor(init?: Partial<CreateUserEmployeeRequest>) {
    this.status = init?.status ?? Constants.UserStatusConfig.Active.value;
    this.name = init?.name ?? null;
    this.avatar = init?.avatar ?? null;
    this.phoneNumber = init?.phoneNumber ?? null;
    this.email = init?.email ?? null;
    this.gender = init?.gender ?? null;
    this.dob = init?.dob ?? null;
    this.address = init?.address ?? null;
    this.code = init?.code ?? null;
    this.emailCompany = init?.emailCompany ?? null;
    this.jobPosition = init?.jobPosition ?? null;
    this.roleId = init?.roleId ?? 0;
  }
}
