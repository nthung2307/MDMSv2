export class CreateProductCategoryRequest {
  // code: string;
  name: string;
  parentId?: number | null;
  description?: string;
  status?: number;

  constructor(arg: {
    // code: string;
    name: string;
    description?: string;
    parentId?: number | null;
    status?: number;
  }) {
    // this.code = arg.code;
    this.name = arg.name;
    this.parentId = arg.parentId;
    this.description = arg.description;
    this.status = arg.status;
  }
}
