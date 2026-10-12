import { PermissionResponse } from '@/core/application/dto/roles/responses/PermissionResponse.ts';

export class ListRoleResponse {
  public data: Array<RoleItemResponse>;
  public total: number;

  constructor(args: { data: Array<RoleItemResponse>; total: number }) {
    this.data = args.data;
    this.total = args.total;
  }
}

export class RoleItemResponse {
  public id: number;
  public name: string;
  public description: string;
  public permissions?: Array<PermissionResponse>;

  constructor(args: {
    id: number;
    name: string;
    description: string;
    permissions?: Array<PermissionResponse>;
  }) {
    this.id = args.id;
    this.name = args.name;
    this.description = args.description;
    this.permissions = args.permissions;
  }
}
