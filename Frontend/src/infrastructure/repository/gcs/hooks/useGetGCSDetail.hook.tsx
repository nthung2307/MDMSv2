import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetGCSDetailRequest } from '@/core/application/dto/gcs/request/GetGCSDetailRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

const useGetGCSDetailHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetGCSDetailRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().getGCSDetail,
      Endpoint.GCS.GetGCSDetail,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetGCSDetailHook;
