import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';

const useIoTConfigHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function getIoTConfig(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().getIoTConfigAsync,
      Endpoint.Config.GetIoTConfig,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function updateIoTConfig(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().updateIoTConfigAsync,
      Endpoint.Config.UpdateIoTConfig,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function generateIoTKey(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().generateIoTKeyAsync,
      Endpoint.Config.GenerateIoTKey,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  return {
    getIoTConfig,
    updateIoTConfig,
    generateIoTKey,
  };
};

export default useIoTConfigHook;

