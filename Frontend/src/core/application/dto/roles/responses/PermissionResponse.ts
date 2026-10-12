export class PermissionResponse {
  public id: number;
  public value: string;
  public role_id: number;

  constructor(args: { id: number; value: string; role_id: number }) {
    this.id = args.id;
    this.value = args.value;
    this.role_id = args.role_id;
  }
}
