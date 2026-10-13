import { Card } from 'antd';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { CreateGCSRequest } from '@/core/application/dto/gcs/request/CreateGCSRequest.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useCreateGCSHook from '@/infrastructure/repository/gcs/hooks/useCreateGCS.hook.tsx';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import GcsForm from '@/pages/gcs-management/components/gcs-form.tsx';

const AddGCS = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState<boolean>(false);
  const [createGCS] = useCreateGCSHook();
  const [dataForm, setDataForm] = useStateDataForm({ dayPeriod: [0] });
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const isDisabled =
    !isValidData(validate) ||
    isFormFieldsEmpty(['code', 'name'], dataForm) ||
    dataForm.dayPeriod.includes(0);
  const createGCSDetailsAsync = async () => {
    const params = new CreateGCSRequest({
      code: dataForm.code,
      description: dataForm.description,
      name: dataForm.name,
      periodNumber: Number(dataForm.dayPeriod.length),
      dayPeriod: dataForm.dayPeriod ?? [],
      status: dataForm.status ?? Constants.GCSStatus.Deactivate,
    });
    setSubmittedTime(new Date());
    setIsShowDialog(!isShowDialog);
    await createGCS(
      params,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', 'Thêm sổ thành công!');
      },
      () => {
        setIsShowDialog(!isShowDialog);
      },
      setLoading
    );
    setSubmittedTime(null);
  };
  return (
    <div>
      <PageTitle title={'Quản lý sổ GCS'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý sổ ghi chỉ số', link: ROUTE_PATH.GCS_MANAGEMENT },
            { name: 'Thêm sổ' },
          ]}
        />
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <GcsForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={false}
            title={'Thêm sổ'}
            createMode={true}
          />
          <div className='flex flex-row gap-2 justify-center mt-5'>
            <ButtonCommon
              title={_t('Huỷ')}
              onClick={() => {
                navigate(ROUTE_PATH.GCS_MANAGEMENT);
              }}
              colorClass={'firebrick-outline-bg'}
            />
            <ButtonCommon
              title={_t('Lưu')}
              onClick={() => {
                setIsShowDialog(!isShowDialog);
              }}
              colorClass={'firebrick-bg'}
              disabled={isDisabled}
            />
          </div>
        </Card>
      </div>
      <FullPageLoading isLoading={loading} />
      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(!isShowDialog);
        }}
        handleConfirm={() => {
          createGCSDetailsAsync();
        }}
        isButtonSubmit={true}
        showFooter={true}
        buttonLoading={loading}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận thêm sổ?')}</p>
            <p className=''>{_t('Sau khi xác nhận thông tin sổ sẽ được lưu vào hệ thống.')}</p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default AddGCS;
