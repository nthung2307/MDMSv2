import { saveAs } from 'file-saver';
import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

export interface ExportGcsTemporaryParams {
  gcsId: number;
  month: number;
  year: number;
  periodNumber: number;
  onlySuspicious?: boolean;
}

const useExportGcsTemporaryExcelHook = () => {
  const { newCancelToken } = useCancelToken();

  async function request(
    params: ExportGcsTemporaryParams,
    onSuccess: () => void,
    onError: (err: any) => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const blob = await new GCSManagementService().exportExcelTemporary(
        Endpoint.GCS.ExportExcelTemporary,
        params,
        newCancelToken()
      );

      const fileName = `MDMS_SoGCS_Ky${params.periodNumber}_T${String(params.month).padStart(2, '0')}_${params.year}.xlsx`;
      saveAs(blob, fileName);
      onSuccess();
    } catch (e) {
      onError(e);
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  return [request] as const;
};

export default useExportGcsTemporaryExcelHook;
