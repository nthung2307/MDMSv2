import { CancelToken } from 'axios';

import { GetListDataGCSRequest } from '@/core/application/dto/IoT/requests/GetDataGcsRequest';
import { GetDataMeasurementPointMonitoringRequest } from '@/core/application/dto/IoT/requests/GetDataMeasurementPointMonitoringRequest.ts';
import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';

export interface IIotService {
  getData(
    endpoint: string,
    params: GetDataMeasurementPointMonitoringRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getListDataGcs(
    endpoint: string,
    params: GetListDataGCSRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
