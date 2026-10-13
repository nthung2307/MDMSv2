import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { ChangePasswordRequest } from '@/core/application/dto/identity/auth/requests/ChangePassword.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { AuthManagementService } from '@/infrastructure/repository/auth/services/auth.service.ts';

const useChangePasswordHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: ChangePasswordRequest,
    onSuccess: (res: any) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AuthManagementService().changePassword,
      Endpoint.Auth.ChangePassword,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useChangePasswordHook;
