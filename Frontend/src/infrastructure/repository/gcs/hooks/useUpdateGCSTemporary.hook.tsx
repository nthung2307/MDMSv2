import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { UpdateGCSTemporaryRequest } from '@/core/application/dto/gcs/request/UpdateGCSTemporaryRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service.ts';

const useUpdateGcsTemporaryHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: UpdateGCSTemporaryRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().updateGCSTemporary,
      Endpoint.GCS.UpdateGCSTemporary,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useUpdateGcsTemporaryHook;
