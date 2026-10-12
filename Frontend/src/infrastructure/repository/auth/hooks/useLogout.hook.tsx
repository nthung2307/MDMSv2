import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { AuthManagementService } from '@/infrastructure/repository/auth/services/auth.service';

export const useLogoutHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: any,
    onSuccess: (res: any) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AuthManagementService().logoutAsync,
      Endpoint.Auth.Logout,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};
