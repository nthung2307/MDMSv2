import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { GCSManagementService } from '@/infrastructure/repository/gcs/services/gcs.service';

export interface ImportGcsTemporaryParams {
  gcsId: number;
  month: number;
  year: number;
  periodNumber: number;
}

export interface ImportGcsTemporaryResult {
  totalRows: number;
  updatedCount: number;
  skippedCount: number;
  errors: string[];
  message: string;
}

const useImportGcsTemporaryExcelHook = () => {
  async function request(
    file: File,
    params: ImportGcsTemporaryParams,
    onSuccess: (res: ImportGcsTemporaryResult) => void,
    onError: (err?: any) => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const response = await new GCSManagementService().importExcelTemporary(
        Endpoint.GCS.ImportExcelTemporary,
        file,
        params
      );
      const payload = (response as any)?.data;
      if (response?.status === 200 && payload?.success) {
        onSuccess(payload.data);
      } else {
        const errorMsg =
          payload?.message ||
          (payload?.errors?.length && payload?.errors[0]?.error) ||
          (response as any)?.message ||
          'Có lỗi xảy ra khi nhập dữ liệu từ file Excel!';
        onError({ message: errorMsg, errors: payload?.errors });
      }
    } catch (e) {
      onError(e);
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  return [request] as const;
};

export default useImportGcsTemporaryExcelHook;
