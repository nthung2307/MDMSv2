import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { SendAllMailNoticeRequest } from '@/core/application/dto/notice/request/SendAllMailRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { NoticeManagementService } from '@/infrastructure/repository/notice/services/notice.service';

const useSendAllMailNoticeHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: SendAllMailNoticeRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new NoticeManagementService().sendAllMailNoticeAsync,
      Endpoint.Notice.SendAllMailNotice,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useSendAllMailNoticeHook;
