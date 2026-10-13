import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook';
import { useApiExportRequestHook } from '@/infrastructure/common/libs/hooks/useApiExportRequest.hook';
import { ExportManagementService } from '@/infrastructure/repository/export/services/export.service';

const useExportFilePdfHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiExportRequestHook();

  async function request(
    type: string,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>,
    subType?: string,
    params?: any,
    requestBody?: any
  ) {
    await requestCommon(
      newCancelToken(),
      new ExportManagementService().exportListPdf,
      `${type}/${Endpoint.Export.ExportAllPdf}-${subType ? subType : type}`,
      params ? params : {},
      requestBody ? requestBody : {},
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useExportFilePdfHook;
