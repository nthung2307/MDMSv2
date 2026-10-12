export class ExportListUserClientRequest {
  name?: string;
  status?: number;
  constructor(init?: Partial<ExportListUserClientRequest>) {
    if (init?.name) {
      this.name = init.name;
    }
    if (init?.status) {
      this.status = init.status;
    }
  }
}
