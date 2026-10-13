import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { MeasurementPointService } from '@/infrastructure/repository/measurement-point/services/measurementPoint.service.ts';

const useChangeMeasurementPointStatusHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointService().changeMeasurementPointStatus,
      Endpoint.MeasurementPoint.ChangeMeasurementPointStatus,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useChangeMeasurementPointStatusHook;
