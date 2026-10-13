import { CancelToken } from 'axios';
import { Dispatch, SetStateAction } from 'react';

import { ErrorDataState } from '@/core/application/common/atoms/global/errorTable';
import { LoadingState } from '@/core/application/common/atoms/global/loadingState';
import FailureResponse from '@/core/application/dto/common/responses/failureResponse';
import InvalidModelStateResponse from '@/core/application/dto/common/responses/invalidModelStateResponse';
import SuccessResponse, {
  DataResponse,
} from '@/core/application/dto/common/responses/successResponse';
import { CodesMap } from '@/core/domain/enums/CodesMap';
import { notifyError } from '@/infrastructure/common/components/toast/toastMessage';
import { setRecoilStateAsync } from '@/infrastructure/common/libs/recoil-outside/recoil.service';
import LoggerService from '@/infrastructure/services/logger.service';
import { refactorFormDataCommon } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';

export const useApiRequestCustomHook = () => {
  const loggerService = new LoggerService();

  async function makeRequest(
    newCancelToken: CancelToken,
    serviceInstance: any,
    endpoint: string,
    params: any,
    onSuccess: (res: any) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    try {
      if (setLoading) {
        setLoading(true);
      } else {
        await setRecoilStateAsync(LoadingState, { isLoading: true, uri: endpoint });
      }
      setRecoilStateAsync(ErrorDataState, false);
      const response = await serviceInstance(
        endpoint,
        refactorFormDataCommon(params),
        newCancelToken
      );
      if ((response as FailureResponse)?.code !== CodesMap.CANCEL_TOKEN) {
        switch (response.status) {
          case 200: {
            const res = (response as SuccessResponse).data;
            if (res.success) {
              if (onSuccess) {
                onSuccess(res as DataResponse);
              }
            } else {
              onError();
              // notifyError(
              //   '',
              //   _t(
              //     (res.errors?.length && res.errors[0]?.error) ||
              //       res.message ||
              //       'Có lỗi xảy ra. Vui lòng liên hệ quản trị viên'
              //   )
              // );
            }
            break;
          }
          case 202: {
            notifyError(
              '',
              _t(
                (response as FailureResponse).message.toString() ||
                  'Có lỗi xảy ra. Vui lòng liên hệ quản trị viên'
              )
            );
            onError();
            break;
          }
          case 400: {
            notifyError(
              '',
              _t(
                (response as InvalidModelStateResponse).message[0] ||
                  'Có lỗi xảy ra. Vui lòng liên hệ quản trị viên'
              )
            );
            loggerService.info(response as InvalidModelStateResponse);
            onError();
            break;
          }
          default: {
            // notifyError('', _t('Có lỗi xảy ra. Vui lòng liên hệ quản trị viên'));
            onError();
          }
        }
      }
    } catch (error) {
      setRecoilStateAsync(ErrorDataState, true);
      notifyError('', _t('Có lỗi xảy ra. Vui lòng liên hệ quản trị viên'));
      onError();
    } finally {
      if (setLoading) {
        setLoading(false);
      } else {
        await setRecoilStateAsync(LoadingState, { isLoading: false, uri: endpoint });
      }
    }
  }

  return [makeRequest];
};
