import { Card } from 'antd';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { CreateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/CreateMeasurementPointRequest';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useCreateMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useCreateMeasurementPoint.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import PointForm from '@/pages/point-management/components/point-form';

const AddPoint = () => {
  const [data, setData] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  // const [loading, setLoading] = useState<boolean>(false);
  const [createMeasurementPointRequest] = useCreateMeasurementPointHook();
  const isDisabled =
    !isValidData(validate) ||
    isFormFieldsEmpty(['code', 'type', 'multiplierFactor', 'gcsId', 'customerId'], data);
  const navigate = useNavigate();

  const createCustomerAsync = async () => {
    if (!isValidData(validate)) return;
    const requestBody = new CreateMeasurementPointRequest({
      ...data,
      status: data.status ?? Constants.AccountStatus.Deactivate,
      multiplierFactor: Number(data.multiplierFactor),
    });
    await createMeasurementPointRequest(
      requestBody,
      () => {
        notifySuccess('', _t('Thêm điểm đo thành công'));
      },
      () => {},
      () => {}
    );
    setIsShowDialog(!isShowDialog);
  };

  return (
    <div>
      <PageTitle title={'Quản lý điểm đo'} />
      <BreadcrumbUI
        items={[
          { name: 'Quản lý điểm đo', link: ROUTE_PATH.POINT_MANAGEMENT },
          { name: 'Thêm điểm đo' },
        ]}
      />
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div className='flex flex-col gap-4'>
            <PointForm
              data={data}
              setData={setData}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              statusUser={Constants.userStatus.Add}
              editMode={true}
            />
            <div className='flex flex-row gap-2 justify-center'>
              <ButtonCommon
                title={_t('Huỷ')}
                onClick={() => {
                  navigate(ROUTE_PATH.POINT_MANAGEMENT);
                }}
                classButton={'w-14'}
                colorClass={'firebrick-outline-bg'}
              />
              <ButtonCommon
                title={_t('Lưu')}
                onClick={() => {
                  setIsShowDialog(!isShowDialog);
                }}
                classButton={'w-14'}
                disabled={isDisabled}
                colorClass={'firebrick-bg'}
              />
            </div>
          </div>
        </Card>
      </div>
      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(!isShowDialog);
        }}
        handleConfirm={() => {
          createCustomerAsync();
        }}
        isButtonSubmit={true}
        showFooter={true}
        buttonLoading={false}
      >
        <div className='flex flex-col gap-4 p-2'>
          <div className='flex flex-col justify-center items-center my-5 gap-2'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận thêm điểm đo?')}</p>
            <p className=''>{_t('Sau khi xác nhận điểm đo mới sẽ được thêm vào hệ thống.')}</p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default AddPoint;
