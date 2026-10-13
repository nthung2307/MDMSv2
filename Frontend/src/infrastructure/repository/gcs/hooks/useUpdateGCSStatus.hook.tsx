import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { UpdateGCSStatusRequest } from '@/core/application/dto/gcs/request/UpdateGCSStatusRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service.ts';

const useUpdateGcsStatusHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: UpdateGCSStatusRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().updateGCSStatus,
      Endpoint.GCS.UpdateGCSStatus,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useUpdateGcsStatusHook;
