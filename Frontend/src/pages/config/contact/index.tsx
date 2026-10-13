import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { TbBook2 } from 'react-icons/tb';

import Constants from '@/core/application/common/constants';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useCreateUpdateSystemConfigHook from '@/infrastructure/repository/config/hooks/useCreateUpdateSystemConfig.hook';
import useGetSystemListHook from '@/infrastructure/repository/config/hooks/useGetSystemList.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const ContactConfig = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [createUpdateSystemConfig] = useCreateUpdateSystemConfigHook();
  const [getSystemList] = useGetSystemListHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const isDisabled =
    !isValidData(validate) ||
    isFormFieldsEmpty(
      [
        'adminName',
        'adminNumber',
        'adminEmail',
        'supporterName',
        'supporterNumber',
        'supporterEmail',
      ],
      dataForm
    );

  const getSystemListAsync = async () => {
    await getSystemList(
      (res: any) => {
        const { data } = res;
        const findVal = (key: string, idx: number, defVal: string = '') =>
          data?.find((x: any) => x.key === key)?.value ?? data?.[idx]?.value ?? defVal;

        const resultData = {
          adminName: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONFULLNAME, 2, 'Administrator'),
          adminNumber: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONPHONE, 3, '0901234567'),
          adminEmail: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONEMAIL, 4, 'admin@mdms.vn'),
          supporterName: findVal(Constants.LockAuth.CUSTOMERCAREFULLNAME, 5, 'CSKH'),
          supporterNumber: findVal(Constants.LockAuth.CUSTOMERCAREPHONE, 6, '19001006'),
          supporterEmail: findVal(Constants.LockAuth.CUSTOMERCAREEMAIL, 7, 'cskh@mdms.vn'),
        };
        setDataForm(resultData);
      },
      () => {},
      setLoading
    );
  };

  const createGCSDetailsAsync = async () => {
    const params = new systemConfigRequest({
      configs: [
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONFULLNAME,
          value: dataForm.adminName,
        },
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONPHONE,
          value: dataForm.adminNumber,
        },
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONEMAIL,
          value: dataForm.adminEmail,
        },
        {
          key: Constants.LockAuth.CUSTOMERCAREFULLNAME,
          value: dataForm.supporterName,
        },
        {
          key: Constants.LockAuth.CUSTOMERCAREPHONE,
          value: dataForm.supporterNumber,
        },
        {
          key: Constants.LockAuth.CUSTOMERCAREEMAIL,
          value: dataForm.supporterEmail,
        },
      ],
    });
    setSubmittedTime(new Date());
    setIsShowDialog(!isShowDialog);
    await createUpdateSystemConfig(
      params,
      () => {
        notifySuccess('', 'Lưu thông tin thành công');
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
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Thông tin liên hệ' }]} />
        <div className='flex'>
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Lưu'}
            classButton='text-button-variant'
            disabled={isDisabled}
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={25} />}
          />
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div>
            <div className={'flex justify-between'}>
              <h1 className='font-bold text-20-20'>{'Quản trị hệ thống'}</h1>
            </div>
            <div className={'flex flex-col gap-y-5 mt-5'}>
              <div>
                <InputTextCommon
                  label={_t('Họ và tên')}
                  attribute={'adminName'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.adminName}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  placeholder={'Nhập họ và tên'}
                />
              </div>
              <div>
                <InputNumberCommon
                  label={_t('Số điện thoại')}
                  attribute={'adminNumber'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.adminNumber}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  placeholder={'Nhập số điện thoại'}
                />
              </div>
              <div>
                <InputTextCommon
                  label={_t('Email')}
                  attribute={'adminEmail'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.adminEmail}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  rules={[
                    {
                      message: _t('Vui lòng nhập đúng định dạng email'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                  placeholder={'Nhập Email'}
                />
              </div>
            </div>
          </div>
          <div className='mt-5'>
            <div className={'flex justify-between'}>
              <h1 className='font-bold text-20-20'>{'Chăm sóc khách hàng'}</h1>
            </div>
            <div className={'flex flex-col gap-y-5 mt-5'}>
              <div>
                <InputTextCommon
                  label={_t('Họ và tên')}
                  attribute={'supporterName'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.supporterName}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  placeholder={'Nhập họ và tên'}
                />
              </div>
              <div>
                <InputNumberCommon
                  label={_t('Số điện thoại')}
                  attribute={'supporterNumber'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.supporterNumber}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  placeholder={'Nhập số điện thoại'}
                />
              </div>
              <div>
                <InputTextCommon
                  label={_t('Email')}
                  attribute={'supporterEmail'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.supporterEmail}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  disabled={false}
                  rules={[
                    {
                      message: _t('Vui lòng nhập đúng định dạng email'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                  placeholder={'Nhập Email'}
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
            <p className='text-16-24 font-bold'>{_t('Xác nhận thay đổi thông tin liên hệ')}</p>
            <p className=''>{_t('Sau khi xác nhận thông tin liên hệ sẽ được lưu mới')}</p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default ContactConfig;
