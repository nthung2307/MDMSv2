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
import LoginPage from '@/pages/login/login';

const RouteRoot = () => {
  const loadingState = useRecoilValue(LoadingState);
  return (
    <BrowserRouter>
      <Routes>
        {/*<Route path={ROUTE_PATH.DASHBOARD} element={<DashBoardPage />} />*/}
        <Route path={ROUTE_PATH.LOGIN} element={<LoginPage />} />
        <Route path='*' element={<AdminPage />} />
        {/*<Route path='*' element={<NotFound />} />*/}
      </Routes>
      <FullPageLoading isLoading={loadingState.isLoading} />
    </BrowserRouter>
  );
};

function App() {
  return (
    <RecoilRoot>
      <ConfigProvider
        theme={{
          token: {
            fontFamily: 'Mulish',
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
