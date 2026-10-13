import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { TbBook2 } from 'react-icons/tb';

import Constants from '@/core/application/common/constants';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useCreateUpdateSystemConfigHook from '@/infrastructure/repository/config/hooks/useCreateUpdateSystemConfig.hook';
import useGetSystemListHook from '@/infrastructure/repository/config/hooks/useGetSystemList.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const AuthLock = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [createUpdateSystemConfig] = useCreateUpdateSystemConfigHook();
  const [getSystemList] = useGetSystemListHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const isDisabled =
    !isValidData(validate) || isFormFieldsEmpty(['lockLogin', 'numberInvalid'], dataForm);

  const getSystemListAsync = async () => {
    await getSystemList(
      (res: any) => {
        const { data } = res;
        const lockLogin =
          data?.find((x: any) => x.key === Constants.LockAuth.LOCKLOGIN)?.value ??
          data?.[0]?.value ??
          5;
        const numberInvalid =
          data?.find((x: any) => x.key === Constants.LockAuth.NUMBEROFINCORRECTENTRIES)?.value ??
          data?.[1]?.value ??
          5;
        setDataForm({ lockLogin, numberInvalid });
      },
      () => {},
      setLoading
    );
  };

  const createGCSDetailsAsync = async () => {
    const params = new systemConfigRequest({
      configs: [
        {
          key: Constants.LockAuth.LOCKLOGIN,
          value: dataForm.lockLogin,
        },
        {
          key: Constants.LockAuth.NUMBEROFINCORRECTENTRIES,
          value: dataForm.numberInvalid,
        },
      ],
    });
    setSubmittedTime(new Date());
    setIsShowDialog(!isShowDialog);
    await createUpdateSystemConfig(
      params,
      () => {
        notifySuccess('', 'Lưu khóa đăng nhập thành công');
      },
      () => {},
      setLoading
    );
    setSubmittedTime(null);
  };

  useEffect(() => {
    getSystemListAsync();
  }, []);
  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Khóa đăng nhập' }]} />
        <div className='flex'>
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Lưu'}
            classButton='text-button-variant'
            disabled={isDisabled}
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={30} />}
          />
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div>
            <div className={'flex justify-between'}>
              <h1 className='font-bold text-20-20'>{'Khóa đăng nhập'}</h1>
            </div>
            <div className={'flex flex-col gap-y-5 mt-5'}>
              <div>
                <InputNumberCommon
                  label={_t('Thời gian khóa (phút)')}
                  attribute={'lockLogin'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.lockLogin}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  rules={[
                    {
                      message: _t('Thời gian khóa phải lớn hơn hoặc bằng 1'),
                      pattern: Constants.PATTERN_REGEX.MinNumber,
                    },
                  ]}
                  placeholder={'Nhập mã sổ'}
                />
              </div>
              <div>
                <InputNumberCommon
                  label={_t('Số lần nhập sai')}
                  attribute={'numberInvalid'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.numberInvalid}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  rules={[
                    {
                      message: _t('Số lần nhập sai phải lớn hơn hoặc bằng 1'),
                      pattern: Constants.PATTERN_REGEX.MinNumber,
                    },
                  ]}
                  placeholder={'Nhập tên sổ'}
                />
              </div>
            </div>
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
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận thay đổi khóa đăng nhập')}</p>
            <p className=''>{_t('Sau khi xác nhận khóa đăng nhập sẽ được lưu mới')}</p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default AuthLock;
