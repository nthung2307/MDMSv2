import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GCSMonthlyDetailRequest } from '@/core/application/dto/gcs/request/GcsMonthlyDetailRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

const useGcsMonthlyDetailHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GCSMonthlyDetailRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().GcsMonthlyDetailAsync,
      Endpoint.GCS.GCSMonthlyDetail,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGcsMonthlyDetailHook;
