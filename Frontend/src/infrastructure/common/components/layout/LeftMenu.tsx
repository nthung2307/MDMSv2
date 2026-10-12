import { Menu } from 'antd';
import Sider from 'antd/es/layout/Sider';
import SubMenu from 'antd/es/menu/SubMenu';
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import arrowLeft from '@/assets/images/arrow-left.svg';
import Constants from '@/core/application/common/constants';
import GroupedMenuItem from '@/core/application/common/models/left-menu/GroupedMenuItem';
import MenuItem from '@/core/application/common/models/left-menu/MenuItem';

const LeftMenu = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const root = Constants.MenuConfigs.MainMenu;
  const [collapsed, setCollapsed] = useState(false);

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
    <Sider collapsed={collapsed} width={260} className='bg-white'>
      <div className='flex flex-1 flex-col'>
        <Menu
          mode={root.mode}
          style={{ height: '100%', borderRight: 0 }}
          selectedKeys={[findKeyByHyperlink(root, location.pathname)?.key || null]}
          defaultOpenKeys={[findKeyByHyperlink(root, location.pathname)?.parentKey || null]}
        >
          <div className='flex pl-7'>
            <div
              className='flex gap-3 items-center cursor-pointer'
              onClick={() => setCollapsed(!collapsed)}
            >
              {!collapsed ? (
                <>
                  <img src={arrowLeft} alt='arrow' />
                  <p className='text-14-20 font-bold'>Menu</p>
                </>
              ) : (
                <img src={arrowLeft} alt='arrow' className='rotate-180' />
              )}
            </div>
          </div>
          {root.items.map((value) => {
            if (value.type === 'item') {
              let item = value as MenuItem;
              return (
                <Menu.Item
                  key={item.key}
                  icon={item.icon && <img src={item.icon} alt='icon' />}
                  onClick={() => navigate(item.hyperlink)}
                >
                  <p className='text-14-20'>{item.displayText}</p>
                </Menu.Item>
              );
            } else if (value.type === 'group') {
              let item = value as GroupedMenuItem;
              return (
                <SubMenu
                  key={item.key}
                  icon={item.icon && <img src={item.icon} alt='icon' />}
                  title={item.displayText}
                >
                  {item.items.map((subItem) => (
                    <Menu.Item
                      className='flex items-center'
                      key={subItem.key}
                      icon={subItem.icon && <img src={subItem.icon} alt='icon' />}
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
