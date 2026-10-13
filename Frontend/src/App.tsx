import { ConfigProvider } from 'antd';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { RecoilRoot } from 'recoil';
import { useRecoilValue } from 'recoil';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { LoadingState } from '@/core/application/common/atoms/global/loadingState';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import RecoilOutsideComponent from '@/infrastructure/common/libs/recoil-outside/recoil.service';
import '@/App.css';
import { AdminPage } from '@/pages/admin';
import ForgotPassword from '@/pages/forgot-password';
import LoginPage from '@/pages/login/login';
import ResetPassword from '@/pages/reset-password';
import FailedDialog from '@/pages/reset-password/components/failed-dialog.tsx';
import SuccessDialog from '@/pages/reset-password/components/success-dialog.tsx';

const RouteRoot = () => {
  const loadingState = useRecoilValue(LoadingState);
  return (
    <BrowserRouter>
      <Routes>
        {/*<Route path={ROUTE_PATH.DASHBOARD} element={<DashBoardPage />} />*/}
        <Route path={ROUTE_PATH.LOGIN} element={<LoginPage />} />
        <Route path={ROUTE_PATH.FORGOT_PASSWORD} element={<ForgotPassword />} />
        <Route path={ROUTE_PATH.RESET_PASSWORD} element={<ResetPassword />} />
        <Route path={ROUTE_PATH.RESET_SUCCESS} element={<SuccessDialog />} />
        <Route path={ROUTE_PATH.RESET_ERROR} element={<FailedDialog />} />
        <Route path='*' element={<AdminPage />} />
        {/*<Route path='*' element={<NotFound />} />*/}
      </Routes>
      <FullPageLoading isLoading={loadingState.isLoading} />
    </BrowserRouter>
  );
};

function App() {
  // This comment is added to trigger Jenkins build. Please ignore this
  return (
    <RecoilRoot>
      <ConfigProvider
        theme={{
          token: {
            fontFamily: 'Mulish',
            colorPrimary: '#b22222',
            colorBgLayout:
              'linear-gradient(180deg, #FAC0B5 0%, #FDE9B7 15.77%, #FAFBFD 44.77%, #B4CAFB 92.27%)',
            colorBgContainer: '#f5f7fc',
          },
          components: {
            Breadcrumb: {
              lastItemColor: '#B22222',
              separatorColor: '#e98d8d',
            },
            Layout: {
              headerBg: '#f5f7fc',
              headerColor: '#000000',
              headerHeight: 50,
              siderBg: '#f5f7fc',
            },
            Table: {
              headerBg: '#d3dae5',
            },
            Menu: {
              itemBg: '#f5f7fc',
              subMenuItemBg: '#d0d3dd',
            },
          },
        }}
      >
        <ToastContainer style={{ width: '500px' }} />
        <RecoilOutsideComponent />
        <RouteRoot />
      </ConfigProvider>
    </RecoilRoot>
  );
}

export default App;
