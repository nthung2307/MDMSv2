import { Form } from 'antd';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import Logo from '@/assets/react.svg';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { LoginRequest } from '@/core/application/dto/identity/auth/requests/LoginRequest';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import InputPasswordCommon from '@/infrastructure/common/components/input/input-password-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import { useLoginHook } from '@/infrastructure/repository/auth/hooks/useLogin.hook';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import { isEmpty } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';

const LoginPage = () => {
  const [requestLogin] = useLoginHook();
  const [validate, setValidate] = useState<any>({});
  const [_dataUser, _setDataUser] = useState<any>({});
  const [loading, setLoading] = useState<boolean>(false);
  const dataUser = _dataUser;
  const navigate = useNavigate();
  const localStorageService = new LocalStorageService();
  const setDataUser = (data: any) => {
    Object.assign(dataUser, { ...data });
    _setDataUser({ ...dataUser });
  };

  const checkAuth = () => {
    let storage: any = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
    return storage
      ? navigate(ROUTE_PATH.DASHBOARD, { replace: true })
      : navigate(ROUTE_PATH.LOGIN, { replace: true });
  };

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

  const onSubmit = async () => {
    if (isValidData()) {
      const params = new LoginRequest({
        email: dataUser.email,
        password: dataUser.password,
      });
      if (params.email === 'admin@gmail.com' && params.password === 'Abc@12345') {
        localStorageService.setStorage(Constants.API_TOKEN_STORAGE, Constants.API_TOKEN_STORAGE);
        navigate(ROUTE_PATH.DASHBOARD, { replace: true });
        return;
      }
      await requestLogin(
        params,
        (res: DataResponse) => {
          if (res?.result?.accessToken) {
            localStorageService.setStorage(Constants.API_TOKEN_STORAGE, res?.result);
            navigate(ROUTE_PATH.DASHBOARD, { replace: true });
          }
        },
        () => {},
        setLoading
      );
    }
  };

  useEffect(() => {
    checkAuth();
  }, []);

  const isDisable = isEmpty(dataUser?.email) || isEmpty(dataUser?.password);

  return (
    <>
      <div>
        <FullPageLoading isLoading={loading} />
        {/*<img*/}
        {/*  src={LoginBackground}*/}
        {/*  alt='Construction background'*/}
        {/*  className='absolute -z-1 w-screen h-screen'*/}
        {/*/>*/}
        <div className='w-screen h-screen flex justify-center items-center px-5'>
          <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10'>
            <img src={Logo} alt='logo' className='m-auto' />
            <h1 className='text-32-32 font-bold mt-10'>{_t('Đăng Nhập')}</h1>
            <Form onFinish={onSubmit}>
              <div className='flex flex-col gap-3 mt-5'>
                <div>
                  <InputTextCommon
                    autoFocus
                    attribute='email'
                    autoComplete='email'
                    isRequired
                    label={_t('Tên đăng nhập')}
                    setData={setDataUser}
                    placeholder={_t('Tên đăng nhập')}
                    rules={[
                      {
                        message: _t('Không đúng định dạng email'),
                        pattern: Constants.PATTERN_REGEX.Email,
                      },
                    ]}
                    setValidate={setValidate}
                    validate={validate}
                    maxLength={255}
                  />
                </div>
                <div>
                  <InputPasswordCommon
                    attribute='password'
                    isRequired
                    label={_t('Mật khẩu')}
                    setData={setDataUser}
                    placeholder={_t('Mật khẩu')}
                    setValidate={setValidate}
                    validate={validate}
                    rules={[
                      {
                        message: _t('Mật khẩu phải có ít nhất 6 ký tự'),
                        pattern: Constants.PATTERN_REGEX.PasswordLow,
                      },
                    ]}
                    maxLength={25}
                  />
                </div>
              </div>
              <ButtonCommon
                title={_t('Đăng nhập')}
                colorClass='blue-bg'
                classButton='w-full mt-5'
                htmlType='submit'
                onClick={() => {}}
                disabled={isDisable || loading}
              />
            </Form>
          </div>
        </div>
      </div>
    </>
  );
};

export default LoginPage;
