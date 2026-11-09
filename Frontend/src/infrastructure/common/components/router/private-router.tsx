import jwtDecode from 'jwt-decode';
import { Navigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import LocalStorageService from '@/infrastructure/services/localStorage.service';

interface Props {
  component: React.ComponentType;
  path?: string;
  roles?: Array<any>;
}

export const PrivateRoute: React.FC<Props> = ({ component: RouteComponent, roles = [] }) => {
  let localStorageService = new LocalStorageService();
  let storage: any = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);

  if (storage) {
    try {
      const info: any = jwtDecode(storage);
      const permissions = info?.role;
      let checkRole = false;
      if (roles && roles.length) {
        roles.some((role) => {
          if (permissions === role) {
            checkRole = true;
            return true;
          }
        });
      }
      if (checkRole) {
        return <RouteComponent />;
      }
      return <Navigate to={ROUTE_PATH.FORBIDDEN_ACCESS} />;
    } catch {
      localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
      localStorageService.removeStorage(Constants.API_REFRESH_TOKEN_STORAGE);
      return <Navigate to={ROUTE_PATH.LOGIN} />;
    }
  }

  return <Navigate to={ROUTE_PATH.LOGIN} />;
};
