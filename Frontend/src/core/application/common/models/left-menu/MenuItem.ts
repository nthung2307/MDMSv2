export default class MenuItem {
  constructor(
    roleAction: Array<any>,
    key: string,
    icon: any,
    displayText: string,
    breadcrumb: string,
    allowedPermissions: string[],
    hyperlink: string,
    parentKey: string | null
  ) {
    this.roleAction = roleAction;
    this.key = key;
    this.icon = icon;
    this.displayText = displayText;
    this.breadcrumb = breadcrumb;
    this.allowedPermissions = allowedPermissions;
    this.hyperlink = hyperlink;
    this.parentKey = parentKey;
  }
  roleAction: Array<any>;
  icon: any;
  key: string;
  type: string = 'item';
  displayText: string;
  breadcrumb: string;
  allowedPermissions: string[];
  hyperlink: string;
  parentKey: string | null = null;
}
