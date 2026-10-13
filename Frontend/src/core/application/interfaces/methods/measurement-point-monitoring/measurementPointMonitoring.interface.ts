import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest.ts';
import { GetMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetMeasurementPointMonitoringRequest';

export interface IMeasurementPointMonitoringService {
  getMeasurementPointMonitoringList(
    endpoint: string,
    params: GetListMeasurementPointMonitoringRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getMeasurementPointMonitoring(
    endpoint: string,
    params: GetMeasurementPointMonitoringRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
