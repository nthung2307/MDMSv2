export class GetMeasurementPointMonitoringRequest {
  tagName: string;

  constructor(args: { tagName: string }) {
    this.tagName = args.tagName;
  }
}
