import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { GetListDataGCSRequest } from '@/core/application/dto/IoT/requests/GetDataGcsRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { IotService } from '@/infrastructure/repository/IoT/services/iot.service.ts';

const useGetDataGcsHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetListDataGCSRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new IotService().getListDataGcs,
      Endpoint.IoT.GetListGCS,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetDataGcsHook;
