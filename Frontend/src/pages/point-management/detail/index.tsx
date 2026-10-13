import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FaRegEdit } from 'react-icons/fa';
import { FaRegTrashCan } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useDeleteMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useDeleteMeasurementPoint.hook.tsx';
import useGetMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useGetMeasurementPoint.hook';
import { _t } from '@/infrastructure/utils/translation.ts';
import PointForm from '@/pages/point-management/components/point-form';

const PointDetail = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const { id } = useParams();
  const recordId = id ? Number(id) : 0;
  const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  const [deletePointRequest] = useDeleteMeasurementPointHook();
  const [getMeasurementPointDetail] = useGetMeasurementPointHook();
  const [data, setData] = useState<any>();

  const deleteMeasurementPointAsync = async (id: number) => {
    await deletePointRequest(
      { id },
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
        notifySuccess('', _t('Xoá điểm đo thành công.'));
        navigate(ROUTE_PATH.POINT_MANAGEMENT);
      },
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
      },
      setLoading
    );
  };

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

  useEffect(() => {
    if (id) getMeasurementPointDetailAsync({ id: recordId });
  }, [id]);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  return (
    <div>
      <PageTitle title={'Quản lý điểm đo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý điểm đo', link: ROUTE_PATH.POINT_MANAGEMENT },
            { name: 'Thông tin điểm đo' },
          ]}
        />
        <div className={'flex items-center gap-3'}>
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              icon={<FaRegEdit size={20} />}
              classButton={'text-button-variant'}
              onClick={() => {
                navigate(ROUTE_PATH.EDIT_POINT.replace(':id', recordId.toString()));
              }}
              tooltipText={'Sửa'}
            />
          </PrivateComponent>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              classButton={'text-button-variant'}
              onClick={() => {
                setIsShowDeleteDialog(!isShowDeleteDialog);
              }}
              icon={<FaRegTrashCan size={20} />}
              tooltipText={'Xoá'}
            />
          </PrivateComponent>
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <PointForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={true}
            statusUser={Constants.userStatus.View}
            title={'Thông tin điểm đo'}
          />
        </Card>
      </div>
      <DialogCommon
        title={''}
        visible={isShowDeleteDialog}
        handleCancel={() => {
          setIsShowDeleteDialog(!isShowDeleteDialog);
        }}
        handleConfirm={() => {
          deleteMeasurementPointAsync(Number(id));
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>Xác nhận xoá điểm đo</p>
            <p className='text-16-16'>{_t('Sau khi xác nhận điểm đo sẽ bị xoá khỏi hệ thống')}</p>
          </div>
        </div>
      </DialogCommon>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default PointDetail;
