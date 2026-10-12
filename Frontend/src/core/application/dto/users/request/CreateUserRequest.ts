export class CreateCustomerRequest {
  phoneNumber: string | null;
  email: string;
  status: number;
  account_type: number;
  name: string;
  org_name: string;
  avatar: string | null;
  tax_code: string | null;
  gender: number | null;
  dob: string | null; // or Date if you handle conversion
  address: string | null;
  email_company: string;
  position: string | null;
  website: string | null;
  field_activity: string | null;
  customer_group: string | null;
  customer_type: string | null;
  landline_number: string | null;
  card_id: string | null;
  issuance_date: string | null; // or Date if you handle conversion
  place_of_issue: string | null;
  staff_id: number | null;

  constructor(init?: Partial<CreateCustomerRequest>) {
    this.phoneNumber = init?.phoneNumber ?? null;
    this.email = init?.email ?? '';
    this.status = init?.status ?? 0;
    this.account_type = init?.account_type ?? 0;
    this.name = init?.name ?? '';
    this.org_name = init?.org_name ?? '';
    this.avatar = init?.avatar ?? null;
    this.tax_code = init?.tax_code ?? null;
    this.gender = init?.gender ?? null;
    this.dob = init?.dob ?? null;
    this.address = init?.address ?? null;
    this.email_company = init?.email_company ?? '';
    this.position = init?.position ?? null;
    this.website = init?.website ?? null;
    this.field_activity = init?.field_activity ?? null;
    this.customer_group = init?.customer_group ?? null;
    this.customer_type = init?.customer_type ?? null;
    this.landline_number = init?.landline_number ?? null;
    this.card_id = init?.card_id ?? null;
    this.issuance_date = init?.issuance_date ?? null;
    this.place_of_issue = init?.place_of_issue ?? null;
    this.staff_id = init?.staff_id ?? null;
  }
}
