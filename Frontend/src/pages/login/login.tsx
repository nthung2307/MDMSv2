import { Form } from 'antd';
import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { LogoLandscape } from '@/assets/icons';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { LoginRequest } from '@/core/application/dto/identity/auth/requests/LoginRequest';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import InputPasswordCommon from '@/infrastructure/common/components/input/input-password-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useLoginHook } from '@/infrastructure/repository/auth/hooks/useLogin.hook';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
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
        userName: dataUser.userName,
        password: dataUser.password,
      });
      // if (params.email === 'admin@gmail.com' && params.password === 'Abc@12345') {
      //   localStorageService.setStorage(Constants.API_TOKEN_STORAGE, Constants.API_TOKEN_STORAGE);
      //   navigate(ROUTE_PATH.DASHBOARD, { replace: true });
      //   return;
      // }
      await requestLogin(
        params,
        (res: DataResponse) => {
          if (res?.data?.accessToken) {
            localStorageService.setStorage(Constants.API_TOKEN_STORAGE, res?.data?.accessToken);
            localStorageService.setStorage(
              Constants.API_REFRESH_TOKEN_STORAGE,
              res?.data?.refreshToken
            );
            navigate('/', { replace: true });
            return;
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

  // const isDisable = isEmpty(dataUser?.userName) || isEmpty(dataUser?.password);

  return (
    <>
      <PageTitle title={'Đăng nhập'} />
      <div>
        <div className='w-screen h-screen flex justify-center items-center px-5 bg-[#f5f7fc]'>
          <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10'>
            <LogoLandscape width={100} className='mb-10' />
            <Form onFinish={onSubmit}>
              <div className='flex flex-col gap-3 mt-5'>
                <div>
                  <InputTextCommon
                    autoFocus
                    attribute='userName'
                    autoComplete='text'
                    isRequired={true}
                    label={_t('Tên đăng nhập hoặc email')}
                    setData={setDataUser}
                    placeholder={_t('Nhập tên đăng nhập hoặc email')}
                    // rules={[
                    //   {
                    //     message: _t('Không đúng định dạng email'),
                    //     pattern: Constants.PATTERN_REGEX.Email,
                    //   },
                    // ]}
                    setValidate={setValidate}
                    validate={validate}
                    maxLength={255}
                  />
                </div>
                <div>
                  <InputPasswordCommon
                    attribute='password'
                    isRequired={true}
                    label={_t('Nhập mật khẩu')}
                    setData={setDataUser}
                    placeholder={_t('Nhập mật khẩu')}
                    setValidate={setValidate}
                    validate={validate}
                    rules={[
                      {
                        message: _t(
                          'Mật khẩu không đúng định dạng. Bao gồm tối thiểu 8 kí tự(bao gồm số và chữ cái, chữ in hoa, kí tự đặc biệt).'
                        ),
                        pattern: Constants.PATTERN_REGEX.PasswordStrong,
                      },
                    ]}
                    maxLength={25}
                  />
                </div>
                <Link to={ROUTE_PATH.FORGOT_PASSWORD} className='text-right text-blue-500'>
                  {_t('Quên mật khẩu')}
                </Link>
              </div>
              <ButtonCommon
                title={_t('Đăng nhập')}
                colorClass='firebrick-bg'
                classButton='w-full mt-5'
                htmlType='submit'
                onClick={() => {}}
                loading={loading}
              />
            </Form>
          </div>
        </div>
      </div>
    </>
  );
};

export default LoginPage;
