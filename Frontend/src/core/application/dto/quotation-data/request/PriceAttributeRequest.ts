export class PriceAttributeRequest {
  name: string;
  original_material_price: number;
  original_labor_price: number;
  constructor(arg: {
    name: string;
    original_material_price: number;
    original_labor_price: number;
  }) {
    this.name = arg.name;
    this.original_material_price = arg.original_material_price;
    this.original_labor_price = arg.original_labor_price;
  }
}
