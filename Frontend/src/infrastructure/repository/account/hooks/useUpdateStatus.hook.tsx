import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { UpdateAccountStatus } from '@/core/application/dto/account/request/UpdateAccountStatus';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { AccountManagementService } from '@/infrastructure/repository/account/services/account.service';

const useUpdateStatusHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: UpdateAccountStatus,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new AccountManagementService().updateStatus,
      Endpoint.Account.UpdateStatus,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useUpdateStatusHook;
