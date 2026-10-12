import { getListRole } from '@/infrastructure/utils/helpers';

export const ButtonPermission = ({ allRoleAction = [], className = '', children }: any) => {
  let roleOfUser = getListRole();
  let checkRole = false;

  if (allRoleAction.length) {
    allRoleAction.map((it: any) => {
      if (roleOfUser?.some((itC: any) => itC == it)) {
        checkRole = true;
      }
    });
  }
  if (checkRole) {
    return <div className={className}>{children}</div>;
  }
  return null;
};
