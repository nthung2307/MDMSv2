import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { CheckTokenForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/CheckTokenForgotPasswordRequest.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestCustomHook } from '@/infrastructure/common/libs/hooks/useApiRequestCustom.hook.tsx';
import { AuthManagementService } from '@/infrastructure/repository/auth/services/auth.service.ts';

const useCheckTokenForgotPasswordHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestCustomHook();

  async function request(
    params: CheckTokenForgotPasswordRequest,
    onSuccess: (res: any) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AuthManagementService().checkTokenForgotPassword,
      Endpoint.Auth.CheckTokenForgotPassword,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useCheckTokenForgotPasswordHook;
