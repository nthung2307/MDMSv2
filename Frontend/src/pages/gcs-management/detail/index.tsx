import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FaRegEdit } from 'react-icons/fa';
import { FaRegTrashCan } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetGCSDetailRequest } from '@/core/application/dto/gcs/request/GetGCSDetailRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useDeleteGCSHook from '@/infrastructure/repository/gcs/hooks/useDeleteGCS.hook.tsx';
import useGetGCSDetailHook from '@/infrastructure/repository/gcs/hooks/useGetGCSDetail.hook.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import GcsForm from '@/pages/gcs-management/components/gcs-form.tsx';

const GCSDetails = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({ dayPeriod: [0] });
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const { id } = useParams();
  const [getGCSDetails] = useGetGCSDetailHook();
  const [deleteGCSRequest] = useDeleteGCSHook();
  const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);

  const deleteGCSRequestAsync = async (id: number) => {
    await deleteGCSRequest(
      { id },
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
        notifySuccess('', 'Xoá sổ ghi chỉ số thành công.');
        navigate(ROUTE_PATH.GCS_MANAGEMENT, { replace: true });
      },
      () => {},
      setLoading
    );
  };

  const getGCSDetailsAsync = async () => {
    const params = new GetGCSDetailRequest({ id: Number(id) });
    setSubmittedTime(new Date());
    await getGCSDetails(
      params,
      (res) => {
        setDataForm(res?.data);
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
            { name: 'Thông tin sổ' },
          ]}
        />
        <div className={'flex gap-3 items-center'}>
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <div className='flex gap-3'>
              <ButtonCommonCustomizable
                title={null}
                onClick={() => {
                  navigate(ROUTE_PATH.EDIT_GCS.replace(':id', String(id)), {
                    replace: true,
                  });
                }}
                icon={<FaRegEdit size={20} />}
                classButton={'text-button-variant'}
                tooltipText={'Sửa'}
              />
            </div>
          </PrivateComponent>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              onClick={() => {
                setIsShowDeleteDialog(!isShowDeleteDialog);
              }}
              classButton={'text-button-variant'}
              icon={<FaRegTrashCan size={20} />}
              tooltipText={'Xoá'}
            />
          </PrivateComponent>
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <GcsForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={true}
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
          deleteGCSRequestAsync(Number(id));
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>Xác nhận xoá sổ ghi chỉ số</p>
            <p className='text-16-16'>
              {_t('Sau khi xác nhận sổ ghi chỉ số sẽ bị xoá khỏi hệ thống')}
            </p>
          </div>
        </div>
      </DialogCommon>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default GCSDetails;
