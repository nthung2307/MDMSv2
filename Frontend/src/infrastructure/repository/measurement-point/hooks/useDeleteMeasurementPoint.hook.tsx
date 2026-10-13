import { Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { DeleteMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/DeleteMeasurementPointRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeasurementPointService } from '@/infrastructure/repository/measurement-point/services/measurementPoint.service.ts';

const useDeleteMeasurementPointHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: DeleteMeasurementPointRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new MeasurementPointService().deleteMeasurementPoint,
      Endpoint.MeasurementPoint.DeleteMeasurementPoint.replace(
        Constants.ReplaceRouterConfig.id,
        String(params.id)
      ),
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useDeleteMeasurementPointHook;
