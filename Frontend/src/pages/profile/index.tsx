import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FiEdit3 } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetAccountDetailHook from '@/infrastructure/repository/account/hooks/useGetAccountDetail.hook.tsx';
import useAccountId from '@/infrastructure/repository/auth/hooks/useAccountId.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import ProfileForm from '@/pages/profile/components/profile-form.tsx';

const Profile = () => {
  const id = useAccountId();
  const [getAccountDetail] = useGetAccountDetailHook();
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [data, setData] = useState<any>();

  const getAccountDetailAsync = async () => {
    const params = new GetAccountDetailRequest({ id });
    await getAccountDetail(
      params,
      (res: any) => {
        setData(res?.data);
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    if (id) {
      getAccountDetailAsync();
    }
  }, []);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  return (
    <div>
      <PageTitle title={'Thông tin tài khoản'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Thông tin tài khoản' }]} />
        <div className='flex gap-3'>
          <ButtonCommonCustomizable
            title={''}
            onClick={() => {
              if (id != null) {
                navigate(ROUTE_PATH.EDIT_USER_PROFILE);
              }
            }}
            classButton={'text-button-variant'}
            icon={<FiEdit3 size={20} />}
            tooltipText={_t('Sửa thông tin')}
          />
        </div>
      </div>
      <div className='mt-10'>
        <Card>
          <ProfileForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={true}
            viewMode={true}
          />
        </Card>
      </div>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default Profile;
