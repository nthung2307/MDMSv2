import { Menu } from 'antd';
import Sider from 'antd/es/layout/Sider';
import SubMenu from 'antd/es/menu/SubMenu';
import { useState } from 'react';
import { RxHamburgerMenu } from 'react-icons/rx';
import { useLocation, useNavigate } from 'react-router-dom';

import Constants from '@/core/application/common/constants';
import GroupedMenuItem from '@/core/application/common/models/left-menu/GroupedMenuItem';
import MenuItem from '@/core/application/common/models/left-menu/MenuItem';
import { NavContextMenu } from '@/infrastructure/common/components/context-menu/NavContextMenu';
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
    return allowedPermissions.includes(role);
  };

  const findKeyByHyperlink = (
    items: (MenuItem | GroupedMenuItem)[],
    targetHyperlink: string,
    ancestors: string[] = []
  ): { key: string; ancestorKeys: string[] } | null => {
    for (const rawItem of items) {
      if (rawItem.type === 'item') {
        const item = rawItem as MenuItem;
        if (item.hyperlink && `${targetHyperlink}`.includes(item.hyperlink)) {
          return {
            key: item.key,
            ancestorKeys: ancestors,
          };
        }
      } else if (rawItem.type === 'group') {
        const group = rawItem as GroupedMenuItem;
        if (group.items && group.items.length > 0) {
          const result = findKeyByHyperlink(group.items, targetHyperlink, [
            ...ancestors,
            group.key,
          ]);
          if (result) {
            return result;
          }
        }
      }
    }
    return null;
  };

  const renderMenuNode = (node: MenuItem | GroupedMenuItem, isNested: boolean = false): any => {
    if (node.type === 'item') {
      const item = node as MenuItem;
      if (!isAllowed(item.allowedPermissions)) {
        return null;
      }

      return (
        <Menu.Item
          className={isNested ? 'flex items-center' : undefined}
          key={item.key}
          icon={item.icon && <item.icon size={20} />} // react-icons
          onClick={() => navigate(item.hyperlink)}
        >
          <NavContextMenu
            href={item.hyperlink}
            title={item.displayText}
            onClick={(e) => {
              e.stopPropagation();
              navigate(item.hyperlink);
            }}
          >
            <span className='text-14-20 select-none'>{item.displayText}</span>
          </NavContextMenu>
        </Menu.Item>
      );
    }

    if (node.type === 'group') {
      const group = node as GroupedMenuItem;
      const children = group.items
        .map((subItem) => renderMenuNode(subItem, true))
        .filter((child) => child !== null);

      if (children.length === 0) {
        return null;
      }

      return (
        <SubMenu
          key={group.key}
          icon={group.icon && <group.icon size={20} />} // react-icons
          title={group.displayText}
        >
          {children}
        </SubMenu>
      );
    }

    return null;
  };

  const activeMenuMatch = findKeyByHyperlink(root.items, location.pathname);

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
          selectedKeys={activeMenuMatch ? [activeMenuMatch.key] : []}
          defaultOpenKeys={activeMenuMatch ? activeMenuMatch.ancestorKeys : []}
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
          {root.items.map((value) => renderMenuNode(value, false))}
        </Menu>
      </div>
    </Sider>
  );
};

export default LeftMenu;
