import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetFinalizedOutputElectricityChartRequest } from '@/core/application/dto/reports/requests/GetFinalizedOutputElectricityChartRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ReportsService } from '@/infrastructure/repository/reports/services/reports.service.ts';

const useGetFinalizedOutputElectricityChartHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetFinalizedOutputElectricityChartRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ReportsService().getFinalizedOutputElectricityChart,
      Endpoint.Report.GetFinalizedOutputElectricityChart,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetFinalizedOutputElectricityChartHook;
