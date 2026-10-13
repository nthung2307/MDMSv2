import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { UpdateCustomerRequest } from '@/core/application/dto/customer/request/UpdateCustomerRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { CustomerManagementService } from '@/infrastructure/repository/customer/services/customer.service';

const useUpdateCustomerHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: UpdateCustomerRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new CustomerManagementService().updateCustomer,
      Endpoint.Customer.UpdateCustomer,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useUpdateCustomerHook;
