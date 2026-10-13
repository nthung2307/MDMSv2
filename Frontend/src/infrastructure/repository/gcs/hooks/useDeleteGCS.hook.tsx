import { Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { DeleteGCSRequest } from '@/core/application/dto/gcs/request/DeleteGCSRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

const useDeleteGCSHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: DeleteGCSRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().deleteGCS,
      Endpoint.GCS.DeleteGCS.replace(Constants.ReplaceRouterConfig.id, params.id.toString()),
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useDeleteGCSHook;
