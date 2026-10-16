import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { CreateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/CreateMeasurementPointRequest.ts';
import { DeleteMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/DeleteMeasurementPointRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/GetMeasurementPointRequest.ts';
import { UpdateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/UpdateMeasurementPointRequest.ts';

export interface IMeasurementPointService {
  createMeasurementPoint(
    endpoint: string,
    params: CreateMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getListMeasurementPoint(
    endpoint: string,
    params: GetListMeasurementPointsRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getMeasurementPointDetail(
    endpoint: string,
    params: GetMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateMeasurementPoint(
    endpoint: string,
    params: UpdateMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  deleteMeasurementPoint(
    endpoint: string,
    params: DeleteMeasurementPointRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  importMeasurementPointCsv(endpoint: string, file: File): Promise<RequestResponse>;
}
