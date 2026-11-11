import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { MeterReplacementService } from '@/infrastructure/repository/meter-replacement/services/meterReplacement.service';

const useMeterReplacementHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();
  const service = new MeterReplacementService();

  async function getList(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      service.getListAsync.bind(service),
      Endpoint.MeterReplacement.GetList,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function getDetail(
    id: number | string,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.GetDetail.replace(':id', `${id}`);
    await requestCommon(
      newCancelToken(),
      service.getDetailAsync.bind(service),
      endpoint,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function createReplacement(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      service.createAsync.bind(service),
      Endpoint.MeterReplacement.Create,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function updateReplacement(
    id: number | string,
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.Update.replace(':id', `${id}`);
    await requestCommon(
      newCancelToken(),
      service.updateAsync.bind(service),
      endpoint,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function deleteReplacement(
    id: number | string,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.Delete.replace(':id', `${id}`);
    await requestCommon(
      newCancelToken(),
      service.deleteAsync.bind(service),
      endpoint,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function approveReplacement(
    id: number | string,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.Approve.replace(':id', `${id}`);
    await requestCommon(
      newCancelToken(),
      service.approveAsync.bind(service),
      endpoint,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function cancelReplacement(
    id: number | string,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.Cancel.replace(':id', `${id}`);
    await requestCommon(
      newCancelToken(),
      service.cancelAsync.bind(service),
      endpoint,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function getPointMeterInfo(
    pointId: number | string,
    onSuccess: (res: DataResponse) => void,
    onError?: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    const endpoint = Endpoint.MeterReplacement.PointMeterInfo.replace(':pointId', `${pointId}`);
    await requestCommon(
      newCancelToken(),
      service.getPointMeterInfoAsync.bind(service),
      endpoint,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function uploadDocument(
    file: File,
    onSuccess: (url: string) => void,
    onError?: (errorMessage?: string) => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const response: any = await service.uploadDocumentAsync(
        Endpoint.MeterReplacement.UploadDocument,
        file
      );
      if (response && (response.status === 200 || response.status === 201)) {
        const resData = response.data;
        if (resData && typeof resData === 'object' && resData.success === false) {
          const msg = resData.errors?.[0]?.error || 'Tải lên biên bản hiện trường thất bại.';
          if (onError) onError(msg);
          return;
        }
        const fileUrl = typeof resData === 'string' ? resData : (resData?.data || '');
        if (fileUrl) {
          onSuccess(fileUrl);
        } else {
          if (onError) onError('Không nhận được đường dẫn tệp từ máy chủ.');
        }
      } else {
        const msg = response?.message || 'Tải lên biên bản hiện trường thất bại.';
        if (onError) onError(msg);
      }
    } catch (e: any) {
      if (onError) onError(e?.message || 'Tải lên biên bản hiện trường thất bại.');
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  async function extractOcr(
    params: { imageBase64?: string; documentUrl?: string; apiKey?: string },
    onSuccess: (data: any) => void,
    onError?: (errorMessage?: string) => void | Promise<void>,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    if (setLoading) setLoading(true);
    try {
      const response: any = await service.extractOcrAsync(
        Endpoint.MeterReplacement.ExtractOcr,
        params,
        newCancelToken()
      );
      if (response && response.status === 200) {
        const resData = response.data;
        if (resData && typeof resData === 'object' && resData.success === false) {
          const msg = resData.errors?.[0]?.error || 'Trích xuất dữ liệu thất bại.';
          if (onError) await onError(msg);
          return;
        }
        const data = resData?.data || resData;
        onSuccess(data);
      } else {
        const msg = response?.message || 'Trích xuất dữ liệu thất bại.';
        if (onError) await onError(msg);
      }
    } catch (e: any) {
      if (onError) await onError(e?.message || 'Trích xuất dữ liệu thất bại.');
    } finally {
      if (setLoading) setLoading(false);
    }
  }

  return {
    getList,
    getDetail,
    createReplacement,
    updateReplacement,
    deleteReplacement,
    approveReplacement,
    cancelReplacement,
    getPointMeterInfo,
    uploadDocument,
    extractOcr,
  };
};

export default useMeterReplacementHook;

