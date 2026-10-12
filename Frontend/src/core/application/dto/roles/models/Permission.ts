export class PermissionModel {
  public label: string;
  public parent: string;
  public list?: string;
  public detail?: string;
  public add?: string;
  public edit?: string;
  public delete?: string;
  public approve?: string;

  constructor(args: {
    label: string;
    parent: string;
    list?: string;
    detail?: string;
    add?: string;
    edit?: string;
    delete?: string;
    approve?: string;
  }) {
    this.label = args.label;
    this.parent = args.parent;
    this.list = args.list;
    this.detail = args.detail;
    this.add = args.add;
    this.edit = args.edit;
    this.delete = args.delete;
    this.approve = args.approve;
  }
}
