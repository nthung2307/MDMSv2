export class tagNameRequest {
  tagNames: [
    {
      description: string;
      tagName: string;
      unit: string;
      customName: string;
    },
  ];
  measurementPointId: number;

  constructor(arg: {
    tagNames: [
      {
        description: string;
        tagName: string;
        unit: string;
        customName: string;
      },
    ];
    measurementPointId: number;
  }) {
    this.tagNames = arg.tagNames;
    this.measurementPointId = arg.measurementPointId;
  }
}
