import { Form } from 'antd';
import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { CheckTokenForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/CheckTokenForgotPasswordRequest.ts';
import { ConfirmForgotPasswordRequest } from '@/core/application/dto/identity/auth/requests/ConfirmForgotPasswordRequest.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import InputPasswordCommon from '@/infrastructure/common/components/input/input-password-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useCheckTokenForgotPasswordHook from '@/infrastructure/repository/auth/hooks/useCheckTokenForgotPassword.hook.tsx';
import useConfirmForgotPasswordHook from '@/infrastructure/repository/auth/hooks/useConfirmForgotPassword.hook.tsx';
import { isEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import FailedDialog from '@/pages/reset-password/components/failed-dialog.tsx';
import SuccessDialog from '@/pages/reset-password/components/success-dialog.tsx';

const ResetPassword = () => {
  const [confirmForgotPasswordRequest] = useConfirmForgotPasswordHook();
  const [checkTokenForgotPasswordRequest] = useCheckTokenForgotPasswordHook();
  const [searchParams] = useSearchParams();
  const tokenParam = searchParams.get('token');
  const emailParam = searchParams.get('email');
  const [validate, setValidate] = useState<any>({});
  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();
  const [isFailed, setIsFailed] = useState<boolean>(false);
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false);
  const [isTokenValid, setIsTokenValid] = useState<boolean>(false);

  const confirmForgotPasswordAsync = async () => {
    const params = new ConfirmForgotPasswordRequest({
      email: emailParam || '',
      password: dataForm?.password,
      confirmPassword: dataForm?.confirmPassword,
      token: tokenParam || '',
    });
    if (isValidData(validate)) {
      await confirmForgotPasswordRequest(
        params,
        () => {
          setIsSubmitted(true);
          setIsFailed(false);
        },
        () => {
          setIsSubmitted(true);
          setIsFailed(true);
        },
        setLoading
      );
    }
  };

  const checkTokenForgotPasswordAsync = async () => {
    const params = new CheckTokenForgotPasswordRequest({
      token: tokenParam || '',
    });
    await checkTokenForgotPasswordRequest(
      params,
      () => {
        setIsTokenValid(true);
      },
      () => {
        setIsTokenValid(false);
      },
      setLoading
    );
  };

  const checkSearchParams = () => {
    if (isEmpty(tokenParam) || isEmpty(emailParam) || !tokenParam || !emailParam) {
      navigate(ROUTE_PATH.FORGOT_PASSWORD, { replace: true });
    }
  };

  const onSubmit = async () => {
    await confirmForgotPasswordAsync();
  };

  const isDisable =
    isEmpty(dataForm.password) ||
    isEmpty(dataForm.confirmPassword) ||
    dataForm.password !== dataForm.confirmPassword;

  const resultRenderer = () => {
    if ((isFailed && !isSubmitted) || !isTokenValid) {
      return <FailedDialog />;
    }
    if (isSubmitted && !isFailed) {
      return <SuccessDialog />;
    }
    return (
      <>
        <PageTitle title={'Lấy lại mật khẩu'} />
        <div className='w-screen h-screen flex justify-center items-center px-5'>
          <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10'>
            <h1 className='text-24-28 text-center font-bold my-5'>{_t('Đặt lại mật khẩu')}</h1>
            <Form onFinish={onSubmit}>
              <div className='flex flex-col gap-3 mt-5'>
                <div>
                  <InputPasswordCommon
                    autoFocus={true}
                    dataAttribute={dataForm.password}
                    attribute={'password'}
                    isRequired={true}
                    label={_t('Mật khẩu mới')}
                    setData={setDataForm}
                    placeholder={_t('Nhập mật khẩu mới')}
                    setValidate={setValidate}
                    validate={validate}
                    rules={[
                      {
                        message:
                          'Mật khẩu không đúng định dạng. Bao gồm tối thiểu 8 kí tự (bao gồm số và chữ cái, chữ in hoa, kí tự đặc biệt).',
                        pattern: Constants.PATTERN_REGEX.PasswordStrong,
                      },
                    ]}
                  />
                </div>
                <div>
                  <InputPasswordCommon
                    dataAttribute={dataForm.confirmPassword}
                    attribute={'confirmPassword'}
                    isRequired={true}
                    label={_t('Nhập lại mật khẩu mới')}
                    setData={setDataForm}
                    placeholder={_t('Nhập lại mật khẩu mới')}
                    setValidate={setValidate}
                    validate={validate}
                    validateOnBlur={false}
                    // rules={[
                    //   {
                    //     message:
                    //       'Mật khẩu không đúng định dạng. Bao gồm tối thiểu 8 kí tự (bao gồm số và chữ cái, chữ in hoa, kí tự đặc biệt).',
                    //     pattern: Constants.PATTERN_REGEX.PasswordStrong,
                    //   },
                    // ]}
                  />
                  {dataForm?.confirmPassword && dataForm.password !== dataForm.confirmPassword && (
                    <div className='message-error'>
                      <div className='message'>Mật khẩu nhập lại không khớp.</div>
                    </div>
                  )}
                </div>
              </div>
              <ButtonCommon
                title={_t('Tiếp theo')}
                colorClass='firebrick-bg'
                classButton='w-full mt-5'
                htmlType='submit'
                onClick={() => {}}
                disabled={isDisable}
              />
            </Form>
          </div>
        </div>
      </>
    );
  };

  useEffect(() => {
    checkSearchParams();
    checkTokenForgotPasswordAsync();
  }, []);

  return (
    <div>
      {resultRenderer()}
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default ResetPassword;
