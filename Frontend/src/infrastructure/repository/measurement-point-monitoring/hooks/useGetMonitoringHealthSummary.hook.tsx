import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import SuccessResponse, {
  DataResponse,
} from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { MeasurementPointMonitoringService } from '@/infrastructure/repository/measurement-point-monitoring/services/measurementPointMonitoring.service.ts';

const useGetMonitoringHealthSummaryHook = () => {
  const { newCancelToken } = useCancelToken();

  async function request(
    params: { timeoutMinutes?: number },
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    try {
      if (setLoading) {
        setLoading(true);
      }
      const response = await new MeasurementPointMonitoringService().getMonitoringHealthSummary(
        Endpoint.MeasurementPointMonitoring.GetHealthSummary,
        params,
        newCancelToken()
      );
      if (response && response.status === 200) {
        const res = (response as SuccessResponse).data;
        if (res && res.success && onSuccess) {
          onSuccess(res as DataResponse);
        }
      }
    } catch {
      if (onError) {
        onError();
      }
    } finally {
      if (setLoading) {
        setLoading(false);
      }
    }
  }

  return [request];
};

export default useGetMonitoringHealthSummaryHook;
