export class ExportCustomerCSVRequest {
  searchByFields?:
    | [
        {
          searchFieldName: string;
          searchValue: string;
        },
      ]
    | [];

  constructor(args: { searchByFields?: [{ searchFieldName: 'string'; searchValue: 'string' }] }) {
    if (args.searchByFields) {
      this.searchByFields = args.searchByFields;
    }
  }
}
