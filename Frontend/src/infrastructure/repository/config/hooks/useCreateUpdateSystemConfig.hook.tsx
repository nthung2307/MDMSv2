import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';

const useCreateUpdateSystemConfigHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: systemConfigRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().createSystemConfigAsync,
      Endpoint.Config.CreateSystemConfig,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useCreateUpdateSystemConfigHook;
