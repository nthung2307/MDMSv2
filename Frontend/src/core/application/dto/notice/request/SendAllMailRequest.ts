export class SendAllMailNoticeRequest {
  gcsId: number;

  month?: number;

  year?: number;

  periodNumber: number;

  constructor(args: {
    gcsId: number;

    month?: number;

    year?: number;

    periodNumber: number;
  }) {
    this.gcsId = args.gcsId;
    if (args.month) {
      this.month = args.month;
    }
    if (args.year) {
      this.year = args.year;
    }

    this.periodNumber = args.periodNumber;
  }
}
