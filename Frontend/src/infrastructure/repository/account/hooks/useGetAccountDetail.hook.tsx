import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { AccountManagementService } from '@/infrastructure/repository/account/services/account.service.ts';

const useGetAccountDetailHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: GetAccountDetailRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AccountManagementService().getAccountDetail,
      Endpoint.Account.GetAccountDetail,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetAccountDetailHook;
