import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { ChangeCustomerStatusRequest } from '@/core/application/dto/customer/request/ChangeCustomerStatusRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook';
import { CustomerManagementService } from '@/infrastructure/repository/customer/services/customer.service';

const useChangeCustomerStatusHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: ChangeCustomerStatusRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new CustomerManagementService().changeCustomerStatus,
      Endpoint.Customer.ChangeCustomerStatus,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useChangeCustomerStatusHook;
