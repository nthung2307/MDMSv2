import { Card } from 'antd';
import { useEffect, useState } from 'react';
// import { IoIosWarning } from 'react-icons/io';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
// import { UpdateCustomerRequest } from '@/core/application/dto/customer/request/UpdateCustomerRequest';
import { UpdateMeasurementPointRequest } from '@/core/application/dto/measurement-point/requests/UpdateMeasurementPointRequest';
import { ICustomerDetailResponseInterface } from '@/core/application/interfaces/response/customer/customerResponse.interface';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useGetMeasurementPoint.hook';
import useUpdateMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useUpdateMeasurementPoint.hook';
import { isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import PointForm from '@/pages/point-management/components/point-form';

const EditPoint = () => {
  const [getMeasurementPointDetail] = useGetMeasurementPointHook();
  const [updateMeasurementPointRequest] = useUpdateMeasurementPointHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const { id } = useParams();
  const [data, setData] = useState<ICustomerDetailResponseInterface>();
  const isDisabled = !isValidData(validate);
  const navigate = useNavigate();

  const getMeasurementPointDetailAsync = async ({ id }: any) => {
    await getMeasurementPointDetail(
      { id },
      (data: DataResponse) => {
        setData(data?.data);
      },
      () => {},
      setLoading
    );
  };

  const updateCustomerAsync = async () => {
    setSubmittedTime(new Date());
    if (!isValidData(validate)) return;
    const requestBody = new UpdateMeasurementPointRequest({
      id: dataForm.id,
      code: dataForm.code,
      type: dataForm.type,
      address: dataForm.address,
      description: dataForm.description,
      customerId: dataForm.customerId,
      status: dataForm.status,
      gcsId: dataForm.gcsId,
      multiplierFactor: Number(dataForm.multiplierFactor),
    });

    await updateMeasurementPointRequest(
      requestBody,
      () => {
        notifySuccess('', _t('Cập nhật thông tin điểm đo thành công'));
      },
      () => {},
      setLoading
    );
    setIsShowDialog(!isShowDialog);
  };

  useEffect(() => {
    if (id) getMeasurementPointDetailAsync({ id: Number(id) });
  }, [id]);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  return (
    <div>
      <PageTitle title={'Quản lý điểm đo'} />
      <div>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý điểm đo', link: ROUTE_PATH.POINT_MANAGEMENT },
            { name: 'Sửa thông tin điểm đo' },
          ]}
        />
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <PointForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            statusUser={Constants.userStatus.Edit}
            title={'Sửa điểm đo'}
            editMode={true}
          />
          <div className='flex flex-row gap-2 justify-center mt-5'>
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
        </Card>
        <DialogCommon
          title={''}
          visible={isShowDialog}
          handleCancel={() => {
            setIsShowDialog(!isShowDialog);
          }}
          handleConfirm={() => {
            updateCustomerAsync();
          }}
          isButtonSubmit={true}
          showFooter={true}
        >
          <div className='flex flex-col gap-4'>
            <div className='flex flex-col justify-center items-center my-5'>
              {/* <IoIosWarning className='text-firebrick self-center' size={140} /> */}
              <p className='text-16-24 font-bold'>{_t('Xác nhận sửa thông tin điểm đo?')}</p>
              <p className=''>
                {_t('Sau khi xác nhận thông tin điểm đo sẽ thay đổi trên hệ thống.')}
              </p>
            </div>
          </div>
        </DialogCommon>
      </div>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default EditPoint;
