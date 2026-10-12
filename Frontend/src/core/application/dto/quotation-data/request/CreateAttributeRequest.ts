import { PriceAttributeRequest } from '@/core/application/dto/quotation-data/request/PriceAttributeRequest';

export class CreateAttributeRequest {
  code: string;
  name: string;
  description: string;
  status: number;
  values: Array<PriceAttributeRequest>;
  constructor(arg: {
    code: string;
    name: string;
    description: string;
    status: number;
    values: Array<PriceAttributeRequest>;
  }) {
    this.code = arg.code;
    this.name = arg.name;
    this.description = arg.description;
    this.status = arg.status;
    this.values = arg.values;
  }
}
