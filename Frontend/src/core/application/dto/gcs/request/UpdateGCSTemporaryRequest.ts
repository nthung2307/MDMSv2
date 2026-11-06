export class UpdateGCSTemporaryRequest {
  gcsTemporaryRequests: Array<{
    id: number;
    newCurrentReading?: number;
    newPreviousReading?: number;
    isChange?: boolean;
  }>;
  gcsId: number;

  month: number;

  year: number;

  periodNumber: number;

  constructor(arg: {
    gcsTemporaryRequests: Array<{
      id: number;
      newCurrentReading?: number;
      newPreviousReading?: number;
      isChange?: boolean;
    }>;
    gcsId: number;

    month: number;

    year: number;

    periodNumber: number;
  }) {
    this.gcsTemporaryRequests = arg.gcsTemporaryRequests;
    this.gcsId = arg.gcsId;
    this.month = arg.month;
    this.year = arg.year;
    this.periodNumber = arg.periodNumber;
  }
}
