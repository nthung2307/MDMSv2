import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { ChangeGCSStatusRequest } from '@/core/application/dto/gcs/request/ChangeGCSStatusRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

const useChangeGCSStatusHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: ChangeGCSStatusRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new GCSManagementService().changeGCSStatus,
      Endpoint.GCS.ChangeGCSStatus,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useChangeGCSStatusHook;
