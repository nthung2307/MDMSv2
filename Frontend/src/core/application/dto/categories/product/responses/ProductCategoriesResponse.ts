export class ProductCategoriesResponse {
  public data: Array<ProductCategoryItemResponse>;
  public total: number;

  constructor(args: { data: Array<ProductCategoryItemResponse>; total: number }) {
    this.data = args.data;
    this.total = args.total;
  }
}

export class ProductCategoryItemResponse {
  public id: number;
  public code: string;
  public name: string;
  public description: string;
  public status: number;
  public root: boolean;
  public children?: any[];
  public level: number;
  public parent: any;
  public parentId: number;

  constructor(args: {
    code: string;
    children?: any[];
    name: string;
    description: string;
    id: number;
    status: number;
    root: boolean;
    level: number;
    parent: any;
    parentId: number;
  }) {
    this.id = args.id;
    this.code = args.code;
    this.name = args.name;
    this.description = args.description;
    this.children = args.children;
    this.status = args.status;
    this.root = args.root;
    this.level = args.level;
    this.parent = args.parent;
    this.parentId = args.parentId;
  }
}
