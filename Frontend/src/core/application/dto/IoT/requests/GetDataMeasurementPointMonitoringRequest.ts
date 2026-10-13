export class GetDataMeasurementPointMonitoringRequest {
  timestamp: number;
  values: Array<{
    id: string;
    v: number;
    q: string;
    t: number;
  }>;

  constructor(args: {
    timestamp: number;
    values: Array<{ id: string; v: number; q: string; t: number }>;
  }) {
    this.timestamp = args.timestamp;
    this.values = args.values;
  }
}
