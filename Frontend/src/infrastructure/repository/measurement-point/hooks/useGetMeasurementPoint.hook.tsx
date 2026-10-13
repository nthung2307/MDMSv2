import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { GetMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/GetMeasurementPointRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeasurementPointService } from '@/infrastructure/repository/measurement-point/services/measurementPoint.service.ts';

const useGetMeasurementPointHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetMeasurementPointRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointService().getMeasurementPointDetail,
      Endpoint.MeasurementPoint.GetMeasurementPointDetail,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetMeasurementPointHook;
