import { Menu } from 'antd';

import { getListRole } from '@/infrastructure/utils/helpers';

export const MenuItemPermission = ({ allRoleAction = [], children, ...props }: any) => {
  let roleOfUser = getListRole();
  let checkRole = false;

  if (allRoleAction.length) {
    allRoleAction.map((it: any) => {
      if (roleOfUser?.some((itC: any) => itC === it)) {
        checkRole = true;
      }
    });
  }

  if (checkRole) {
    return <Menu.Item {...props}>{children}</Menu.Item>;
  }
  return null;
};
