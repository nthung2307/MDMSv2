import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { IoIosWarning } from 'react-icons/io';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetGCSDetailRequest } from '@/core/application/dto/gcs/request/GetGCSDetailRequest.ts';
import { UpdateGCSRequest } from '@/core/application/dto/gcs/request/UpdateGCSRequest.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetGCSDetailHook from '@/infrastructure/repository/gcs/hooks/useGetGCSDetail.hook.tsx';
import useUpdateGCSHook from '@/infrastructure/repository/gcs/hooks/useUpdateGCS.hook.tsx';
import { isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import GcsForm from '@/pages/gcs-management/components/gcs-form.tsx';

const EditGCS = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [getGCSDetails] = useGetGCSDetailHook();
  const [updateGCSDetails] = useUpdateGCSHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [dataForm, setDataForm] = useStateDataForm({ dayPeriod: [0] });
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const isDisabled = !isValidData(validate);

  const getGCSDetailsAsync = async () => {
    const params = new GetGCSDetailRequest({ id: Number(id) });
    if (!isValidData(validate)) return;
    await getGCSDetails(
      params,
      (res) => {
        setDataForm(res?.data);
      },
      () => {},
      setLoading
    );
  };

  const updateGCSDetailsAsync = async () => {
    const params = new UpdateGCSRequest({
      id: Number(id),
      code: dataForm?.code,
      name: dataForm?.name,
      periodNumber: Number(dataForm?.dayPeriod.length),
      description: dataForm?.description,
      status: dataForm?.status,
      dayPeriod: dataForm.dayPeriod ?? [],
    });
    setSubmittedTime(new Date());
    setIsShowDialog(!isShowDialog);
    await updateGCSDetails(
      params,
      () => {
        notifySuccess('', 'Sửa thông tin sổ thành công!');
      },
      () => {},
      setLoading
    );
    setSubmittedTime(null);
  };

  useEffect(() => {
    getGCSDetailsAsync();
  }, []);

  return (
    <div>
      <PageTitle title={'Quản lý sổ GCS'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý sổ ghi chỉ số', link: ROUTE_PATH.GCS_MANAGEMENT },
            { name: 'Sửa thông tin sổ' },
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
            title={'Sửa thông tin sổ'}
            editMode={true}
          />
          <div className='flex flex-row gap-2 justify-center mt-5'>
            <ButtonCommon
              title={_t('Huỷ')}
              onClick={() => {
                navigate(ROUTE_PATH.GCS_MANAGEMENT);
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
          updateGCSDetailsAsync();
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <IoIosWarning className='text-firebrick self-center' size={140} />
            <p className='text-16-24 font-bold'>{_t('Xác nhận sửa sổ?')}</p>
            <p className=''>
              {_t('Sau khi xác nhận thông tin sổ sẽ được thay đổi trên hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default EditGCS;
