import { Form } from 'antd';
import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useForgotPasswordHook from '@/infrastructure/repository/auth/hooks/useForgotPassword.hook.tsx';
import { isEmpty } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import EmailSent from '@/pages/reset-password/components/email-sent.tsx';

const ForgotPassword = () => {
  const [forgotPasswordRequest] = useForgotPasswordHook();
  const [validate, setValidate] = useState<any>({});
  const [dataForm, setDataForm] = useStateDataForm({});
  const [isDisable, setIsDisable] = useState<boolean>(true);
  const [loading, setLoading] = useState<boolean>(false);
  const [isEmailSent, setIsEmailSent] = useState<boolean>(false); // State to track if the email was sent

  const isValidData = () => {
    let allRequestOK = true;
    setValidate({ ...validate });

    Object.values(validate).forEach((it: any) => {
      if (it.isError === true) {
        allRequestOK = false;
      }
    });

    return allRequestOK;
  };

  const forgotPasswordAsync = async () => {
    if (isValidData()) {
      await forgotPasswordRequest(
        {
          email: dataForm?.email,
        },
        () => {
          setIsEmailSent(true); // Set email sent state to true on success
        },
        () => {},
        setLoading
      );
    }
  };

  const onSubmit = async () => {
    await forgotPasswordAsync();
  };

  useEffect(() => {
    if (!validate.email?.isError) {
      setIsDisable(false);
    } else {
      setIsDisable(true);
    }
  }, [validate]);

  if (isEmailSent) {
    return <EmailSent />;
  }

  return (
    <div>
      <PageTitle title={'Quên mật khẩu'} />
      <FullPageLoading isLoading={loading} />
      <div className='w-screen h-screen flex justify-center items-center px-5'>
        <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10'>
          <h1 className='text-24-28 text-center font-bold'>{_t('Quên mật khẩu')}</h1>
          <Form onFinish={onSubmit}>
            <div className='flex flex-col gap-3 mt-5'>
              <div>
                <InputTextCommon
                  autoFocus
                  dataAttribute={dataForm.email}
                  attribute={'email'}
                  autoComplete='email'
                  isRequired={true}
                  label={_t('Nhập email liên kết tài khoản')}
                  setData={setDataForm}
                  placeholder={_t('Nhập email liên kết tài khoản')}
                  rules={[
                    {
                      message: _t('Email không đúng định dạng'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                  setValidate={setValidate}
                  validate={validate}
                  maxLength={255}
                />
              </div>
            </div>
            <ButtonCommon
              title={_t('Lấy lại mật khẩu')}
              colorClass='firebrick-bg'
              classButton='w-full mt-5'
              htmlType='submit'
              onClick={() => {}}
              disabled={isDisable || loading || isEmpty(dataForm.email)}
            />
          </Form>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
