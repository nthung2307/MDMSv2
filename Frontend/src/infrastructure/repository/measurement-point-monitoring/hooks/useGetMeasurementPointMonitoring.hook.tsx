import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetMeasurementPointMonitoringRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeasurementPointMonitoringService } from '@/infrastructure/repository/measurement-point-monitoring/services/measurementPointMonitoring.service.ts';

const useGetMeasurementPointMonitoringHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetMeasurementPointMonitoringRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointMonitoringService().getMeasurementPointMonitoring,
      Endpoint.MeasurementPointMonitoring.GetMeasurementPointMonitoring,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetMeasurementPointMonitoringHook;
