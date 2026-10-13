import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeasurementPointMonitoringService } from '@/infrastructure/repository/measurement-point-monitoring/services/measurementPointMonitoring.service.ts';

const useGetListMeasurementPointAlertHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetListMeasurementPointMonitoringRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointMonitoringService().getMeasurementPointAlertList,
      Endpoint.MeasurementPointMonitoring.GetListMeasurementPointAlert,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetListMeasurementPointAlertHook;
