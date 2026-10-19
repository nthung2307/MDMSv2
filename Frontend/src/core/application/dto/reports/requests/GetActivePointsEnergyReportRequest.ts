export class GetActivePointsEnergyReportRequest {
  public customerId?: number | null;
  public selectedTime: string;
  public search?: string | null;

  constructor(data?: Partial<GetActivePointsEnergyReportRequest>) {
    this.customerId = data?.customerId ?? null;
    this.selectedTime = data?.selectedTime ?? new Date().toISOString();
    this.search = data?.search ?? null;
  }
}
