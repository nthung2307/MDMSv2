import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { CreateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/CreateMeasurementPointRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeasurementPointService } from '@/infrastructure/repository/measurement-point/services/measurementPoint.service.ts';

const useCreateMeasurementPointHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: CreateMeasurementPointRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointService().createMeasurementPoint,
      Endpoint.MeasurementPoint.CreateMeasurementPoint,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useCreateMeasurementPointHook;
