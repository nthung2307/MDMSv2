import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { UpdateGCSRequest } from '@/core/application/dto/gcs/request/UpdateGCSRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

const useUpdateGCSHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: UpdateGCSRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().updateGCS,
      Endpoint.GCS.UpdateGCS,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useUpdateGCSHook;
