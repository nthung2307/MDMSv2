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
    const info: any = jwtDecode(storage);
    let permissions = info?.role;
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
  }

  return <Navigate to={ROUTE_PATH.LOGIN} />;
};
