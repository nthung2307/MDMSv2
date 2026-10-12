import { PermissionModel } from '@/core/application/dto/roles/models/Permission.ts';

export default class Dashboard {
  static ListPermissions: Array<PermissionModel> = [
    new PermissionModel({
      label: 'Dashboard',
      parent: 'Dashboard',
      detail: 'dashboard:view',
    }),
  ];
}
