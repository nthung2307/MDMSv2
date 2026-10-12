import { Layout } from 'antd';
import { useNavigate } from 'react-router-dom';

import logoutIcon from '@/assets/images/logout.svg';
import logo from '@/assets/react.svg';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import LocalStorageService from '@/infrastructure/services/localStorage.service';

const Header = () => {
  const navigate = useNavigate();
  const localStorageService = new LocalStorageService();

  const handleLogout = async () => {
    await localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
    navigate(ROUTE_PATH.LOGIN);

    // await requestLogout(
    //   {},
    //   () => {
    //     localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
    //   },
    //   () => {},
    //   setLoading
    // );
  };

  return (
    <Layout.Header className='bg-white flex justify-between'>
      <div className='flex items-center'>
        <img src={logo} alt='logo' />
      </div>
      <div className='flex gap-6 items-center'>
        <div className='flex items-center gap-2'>
          <div>
            <p className='text-14-20 font-normal'>
              {localStorageService.readStorage(Constants.API_TOKEN_STORAGE)?.name ?? ''}
            </p>
            <p className=' text-12-18 text-gray-909090'>Admin</p>
          </div>
        </div>
        <div className='cursor-pointer'>
          <img src={logoutIcon} alt='logout' onClick={handleLogout} />
        </div>
      </div>
      {/* <FullPageLoading isLoading={loading} /> */}
    </Layout.Header>
  );
};

export default Header;
