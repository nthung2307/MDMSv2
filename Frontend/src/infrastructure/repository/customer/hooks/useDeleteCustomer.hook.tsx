import { Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { DeleteCustomerRequest } from '@/core/application/dto/customer/request/DeleteCustomerRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { CustomerManagementService } from '@/infrastructure/repository/customer/services/customer.service';

const useDeleteCustomerHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: DeleteCustomerRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new CustomerManagementService().deleteCustomer,
      Endpoint.Customer.DeleteCustomer.replace(
        Constants.ReplaceRouterConfig.id,
        params.id.toString()
      ),
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useDeleteCustomerHook;
