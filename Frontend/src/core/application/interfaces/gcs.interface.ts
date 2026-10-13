import { CancelToken } from 'axios';

import { RequestResponse } from '@/core/application/dto/common/responses/requestResponse.ts';
import { CreateGCSRequest } from '@/core/application/dto/gcs/request/CreateGCSRequest';
import { DeleteGCSRequest } from '@/core/application/dto/gcs/request/DeleteGCSRequest';
import { GetGCSDetailRequest } from '@/core/application/dto/gcs/request/GetGCSDetailRequest';
import { GetListGCSRequest } from '@/core/application/dto/gcs/request/GetListGCSRequest.ts';
import { UpdateGCSRequest } from '@/core/application/dto/gcs/request/UpdateGCSRequest';
import { UpdateGCSStatusRequest } from '@/core/application/dto/gcs/request/UpdateGCSStatusRequest.ts';
import { UpdateGCSTemporaryRequest } from '@/core/application/dto/gcs/request/UpdateGCSTemporaryRequest';

export interface IGCSManagementService {
  getListGCS(
    endpoint: string,
    params: GetListGCSRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  getGCSDetail(
    endpoint: string,
    params: GetGCSDetailRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  createGCS(
    endpoint: string,
    params: CreateGCSRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateGCS(
    endpoint: string,
    params: UpdateGCSRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  deleteGCS(
    endpoint: string,
    params: DeleteGCSRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateGCSStatus(
    endpoint: string,
    params: UpdateGCSStatusRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;

  updateGCSTemporary(
    endpoint: string,
    params: UpdateGCSTemporaryRequest,
    cancellationToken: CancelToken
  ): Promise<RequestResponse>;
}
