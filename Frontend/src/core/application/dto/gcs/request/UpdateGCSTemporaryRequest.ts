export class UpdateGCSTemporaryRequest {
  gcsTemporaryRequests: [
    {
      id: number;
      newCurrentReading?: number;
      newPreviousReading?: number;
    },
  ];
  gcsId: number;

  month: number;

  year: number;

  periodNumber: number;

  constructor(arg: {
    gcsTemporaryRequests: [
      {
        id: number;
        newCurrentReading?: number;
        newPreviousReading?: number;
      },
    ];
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
