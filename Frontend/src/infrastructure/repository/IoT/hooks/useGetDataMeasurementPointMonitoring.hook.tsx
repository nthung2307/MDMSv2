import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { GetDataMeasurementPointMonitoringRequest } from '@/core/application/dto/IoT/requests/GetDataMeasurementPointMonitoringRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { IotService } from '@/infrastructure/repository/IoT/services/iot.service.ts';

const useGetDataMeasurementPointMonitoringHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetDataMeasurementPointMonitoringRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new IotService().getData,
      Endpoint.IoT.GetData,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetDataMeasurementPointMonitoringHook;
