import MenuItem from '@/core/application/common/models/left-menu/MenuItem';

export default class GroupedMenuItem {
  constructor(
    roleAction: Array<any>,
    key: string,
    icon: any,
    displayText: string,
    title: string,
    allowedPermissions: string[],
    children: (MenuItem | GroupedMenuItem)[]
  ) {
    this.roleAction = roleAction;
    this.icon = icon;
    this.key = key;
    this.displayText = displayText;
    this.title = title;
    this.allowedPermissions = allowedPermissions;
    this.items = children;
  }
  roleAction: Array<any>;
  icon: any;
  key: string;
  type: string = 'group';
  displayText: string;
  title: string;
  allowedPermissions: string[];
  items: (MenuItem | GroupedMenuItem)[]; // list of menu items or nested groups in the group
}
