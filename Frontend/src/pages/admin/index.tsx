import { Route, Routes, Navigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import MainLayout from '@/infrastructure/common/components/layout/MainLayout';
import { PrivateRoute } from '@/infrastructure/common/components/router/private-router';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import PermissionDeniedPage from '@/pages/403';
import { NotFound } from '@/pages/404';
import DashBoardPage from '@/pages/dashboard';

export const AdminPage = () => {
  // 👇️ conditionally redirect based on isAuthenticated boolean
  const AuthWrapper = () => {
    let localStorageService = new LocalStorageService();
    let storage: any = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
    return storage && storage.accessToken ? (
      // window.location.href = `${window.location.origin}${ROUTE_PATH.AFTER_LOGIN}`
      <Navigate to={ROUTE_PATH.DASHBOARD} />
    ) : (
      <Navigate to={ROUTE_PATH.LOGIN} replace />
    );
  };

  return (
    <MainLayout>
      <Routes>
        <Route path='*' element={<NotFound />} />
        <Route path='/' element={<AuthWrapper />} />
        <Route path={ROUTE_PATH.FORBIDDEN_ACCESS} element={<PermissionDeniedPage />} />
        <Route
          path={ROUTE_PATH.DASHBOARD}
          element={<PrivateRoute component={DashBoardPage} roles={[]} />}
        />
      </Routes>
    </MainLayout>
  );
};
