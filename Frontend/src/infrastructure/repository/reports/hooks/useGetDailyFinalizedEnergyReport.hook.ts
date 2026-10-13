import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetListDailyDailyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetDailyFinalizedEnergyReportRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ReportsService } from '@/infrastructure/repository/reports/services/reports.service.ts';

const useGetDailyFinalizedEnergyReportHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetListDailyDailyFinalizedEnergyReportRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ReportsService().getDailyFinalizedEnergyReport,
      Endpoint.Report.GetDailyFinalizedEnergyReport,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetDailyFinalizedEnergyReportHook;
