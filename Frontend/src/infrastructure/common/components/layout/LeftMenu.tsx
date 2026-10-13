import { Menu } from 'antd';
import Sider from 'antd/es/layout/Sider';
import SubMenu from 'antd/es/menu/SubMenu';
import { useState } from 'react';
import { RxHamburgerMenu } from 'react-icons/rx';
import { useLocation, useNavigate } from 'react-router-dom';

import Constants from '@/core/application/common/constants';
import GroupedMenuItem from '@/core/application/common/models/left-menu/GroupedMenuItem';
import MenuItem from '@/core/application/common/models/left-menu/MenuItem';
import { getAccountRole } from '@/infrastructure/utils/helpers.ts';

const LeftMenu = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const root = Constants.MenuConfigs.MainMenu;
  const [collapsed, setCollapsed] = useState(false);
  const role = getAccountRole();

  // Check if the user is allowed to access the menu item
  const isAllowed = (allowedPermissions: string[]): boolean => {
    if (!role) return false;
    return allowedPermissions.some((permission) => role.includes(permission));
  };

  const findKeyByHyperlink = (menu: any, targetHyperlink: string): string | any => {
    for (const item of menu.items) {
      if (item.hyperlink && `${targetHyperlink}`.includes(item.hyperlink)) {
        return {
          key: item.key,
          parentKey: item.parentKey,
        };
      }
      // If the item has sub-items, search recursively
      if (item.items && item.items.length > 0) {
        const result = findKeyByHyperlink({ items: item.items }, targetHyperlink);
        if (result) {
          return result;
        }
      }
    }
    return null; // Return null if no matching hyperlink is found
  };

  return (
    <Sider
      collapsed={collapsed}
      width={260}
      breakpoint='lg'
      onBreakpoint={(broken) => {
        setCollapsed(broken);
      }}
      className='max-h-screen border-r border-gray-300 shadow-lg'
    >
      <div className='flex flex-col h-full justify-between'>
        <Menu
          mode={root.mode}
          selectedKeys={[findKeyByHyperlink(root, location.pathname)?.key || null]}
          defaultOpenKeys={[findKeyByHyperlink(root, location.pathname)?.parentKey || null]}
          className={'mt-5 overflow-y-auto'}
        >
          <div className='flex pl-7 mb-3'>
            <div
              className='flex gap-3 items-center cursor-pointer'
              onClick={() => setCollapsed(!collapsed)}
            >
              {!collapsed ? (
                <>
                  <RxHamburgerMenu size={25} />
                  <p className='text-14-20 font-bold'>Menu</p>
                </>
              ) : (
                <RxHamburgerMenu size={25} />
              )}
            </div>
          </div>
          {root.items.map((value) => {
            if (value.type === 'item') {
              const item = value as MenuItem;

              // Hide menu items that are not allowed
              if (!isAllowed(item.allowedPermissions)) {
                return null;
              }

              return (
                <Menu.Item
                  key={item.key}
                  icon={item.icon && <item.icon size={20} />} // react-icons
                  onClick={() => navigate(item.hyperlink)}
                >
                  <p className='text-14-20'>{item.displayText}</p>
                </Menu.Item>
              );
            } else if (value.type === 'group') {
              const group = value as GroupedMenuItem;

              // Filter child items based on permissions
              const allowedSubItems = group.items.filter((subItem) =>
                isAllowed(subItem.allowedPermissions)
              );

              // Hide group if no allowed items
              if (allowedSubItems.length === 0) {
                return null;
              }

              return (
                <SubMenu
                  key={group.key}
                  icon={group.icon && <group.icon size={20} />} // react-icons
                  title={group.displayText}
                >
                  {allowedSubItems.map((subItem) => (
                    <Menu.Item
                      className='flex items-center'
                      key={subItem.key}
                      icon={subItem.icon && <subItem.icon size={20} />}
                      onClick={() => navigate(subItem.hyperlink)}
                    >
                      <p className='text-14-20'>{subItem.displayText}</p>
                    </Menu.Item>
                  ))}
                </SubMenu>
              );
            }
          })}
        </Menu>
      </div>
    </Sider>
  );
};

export default LeftMenu;
