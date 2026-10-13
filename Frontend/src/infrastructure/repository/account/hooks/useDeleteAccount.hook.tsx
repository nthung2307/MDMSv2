import { Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { Endpoint } from '@/core/application/common/endPoint';
import { DeleteAccountRequest } from '@/core/application/dto/account/request/DeleteAccountRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { AccountManagementService } from '@/infrastructure/repository/account/services/account.service';

const useDeleteAccountHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: DeleteAccountRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AccountManagementService().deleteAccount,
      Endpoint.Account.DeleteAccount.replace(Constants.ReplaceRouterConfig.id, String(params.id)),
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useDeleteAccountHook;
