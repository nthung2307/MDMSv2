import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { CustomerManagementService } from '@/infrastructure/repository/customer/services/customer.service';

const useImportCustomerCsvHook = () => {
  async function request(
    file: File,
    onSuccess: (res: any) => void,
    onError: (err?: any) => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const response = await new CustomerManagementService().importCustomerCsv(
        Endpoint.Customer.ImportCustomerCsv,
        file
      );
      if (response && (response as any).success) {
        onSuccess((response as any).data);
      } else {
        onError(response);
      }
    } catch (e) {
      onError(e);
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  return [request] as const;
};

export default useImportCustomerCsvHook;
