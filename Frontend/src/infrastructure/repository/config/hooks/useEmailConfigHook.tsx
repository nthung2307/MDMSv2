import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';

const useEmailConfigHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function getActiveEmailConfig(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().getActiveEmailConfigAsync,
      Endpoint.Config.GetActiveEmailConfig,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function updateEmailConfig(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().updateEmailConfigAsync,
      Endpoint.Config.UpdateEmailConfig,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function testEmailConnection(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().testEmailConnectionAsync,
      Endpoint.Config.TestEmailConnection,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return { getActiveEmailConfig, updateEmailConfig, testEmailConnection };
};

export default useEmailConfigHook;
