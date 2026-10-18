import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetEnergyConsumptionReportRequest } from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ReportsService } from '@/infrastructure/repository/reports/services/reports.service.ts';

const useGetEnergyConsumptionReportHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetEnergyConsumptionReportRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ReportsService().getEnergyConsumptionReport,
      Endpoint.Report.GetEnergyConsumptionReport,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetEnergyConsumptionReportHook;
