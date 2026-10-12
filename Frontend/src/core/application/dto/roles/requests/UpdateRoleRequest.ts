export class UpdateRoleRequest {
  name: string;
  description: string;
  permissions: Array<string>;

  constructor(arg: { name: string; description: string; permissions: Array<string> }) {
    this.name = arg.name;
    this.description = arg.description;
    this.permissions = arg.permissions;
  }
}
