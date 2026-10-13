import { Dispatch, SetStateAction } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { tagNameListRequest } from '@/core/application/dto/tag-name/requests/TagNameListRequest';
import { useCancelToken } from '@/infrastructure/common/libs/axios/cancelToken.hook.tsx';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';

const useGetTagNameListHook = () => {
  const { newCancelToken } = useCancelToken();
  const [requestCommon] = useApiRequestHook();

  async function request(
    params: tagNameListRequest,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      newCancelToken(),
      new ConfigManagementService().getTagNameListAsync,
      Endpoint.Config.GetTagNameList,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  return [request];
};

export default useGetTagNameListHook;
