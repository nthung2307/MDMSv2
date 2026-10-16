import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { MeasurementPointService } from '@/infrastructure/repository/measurement-point/services/measurementPoint.service';

const useImportMeasurementPointCsvHook = () => {
  async function request(
    file: File,
    onSuccess: (res: any) => void,
    onError: (err?: any) => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const response = await new MeasurementPointService().importMeasurementPointCsv(
        Endpoint.MeasurementPoint.ImportMeasurementPointCsv,
        file
      );
      const payload = (response as any)?.data;
      if (response?.status === 200 && payload?.success) {
        onSuccess(payload.data);
      } else {
        const errorMsg =
          payload?.message ||
          (payload?.errors?.length && payload?.errors[0]?.error) ||
          (response as any)?.message ||
          'Có lỗi xảy ra khi nhập dữ liệu từ CSV!';
        onError({ message: errorMsg });
      }
    } catch (e) {
      onError(e);
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  return [request] as const;
};

export default useImportMeasurementPointCsvHook;
