export class FinalMeterReadingRequest {
  gcsId: number;
  month: number;
  year: number;
  periodNumber: number;

  constructor(args: { gcsId: number; month: number; year: number; periodNumber: number }) {
    this.gcsId = args.gcsId;
    this.month = args.month;
    this.year = args.year;
    this.periodNumber = args.periodNumber;
  }
}
