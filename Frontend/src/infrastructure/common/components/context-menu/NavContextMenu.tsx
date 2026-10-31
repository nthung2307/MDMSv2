import { Dropdown, MenuProps } from 'antd';
import React from 'react';
import { FiColumns, FiCopy, FiExternalLink, FiMaximize2 } from 'react-icons/fi';
import { useSetRecoilState } from 'recoil';

import { splitViewState } from '@/core/application/common/atoms/global/splitViewState';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';

interface NavContextMenuProps {
  href: string;
  title: string;
  children: React.ReactNode;
  className?: string;
  onClick?: (e: React.MouseEvent) => void;
}

export const NavContextMenu: React.FC<NavContextMenuProps> = ({
  href,
  title,
  children,
  className,
  onClick,
}) => {
  const setSplitView = useSetRecoilState(splitViewState);

  const contextMenuItems: MenuProps['items'] = [
    {
      key: 'open-tab',
      label: <span className='font-medium text-13-18'>Mở liên kết trong tab mới</span>,
      icon: <FiExternalLink size={15} className='text-blue-600' />,
      onClick: () => {
        window.open(href, '_blank');
      },
    },
    {
      key: 'open-window',
      label: <span className='font-medium text-13-18'>Mở liên kết trong cửa sổ mới</span>,
      icon: <FiMaximize2 size={15} className='text-emerald-600' />,
      onClick: () => {
        const w = 1280;
        const h = 800;
        const left = Math.max(0, (window.screen.width - w) / 2);
        const top = Math.max(0, (window.screen.height - h) / 2);
        window.open(
          href,
          '_blank',
          `popup=yes,width=${w},height=${h},top=${top},left=${left},toolbar=no,menubar=no`
        );
      },
    },
    {
      key: 'open-split',
      label: <span className='font-medium text-13-18'>Mở liên kết ở dạng phân tách</span>,
      icon: <FiColumns size={15} className='text-purple-600' />,
      onClick: () => {
        setSplitView({
          isOpen: true,
          url: href,
          title: title || 'Chế độ xem phân tách',
          widthPercent: 50,
        });
      },
    },
    {
      type: 'divider',
    },
    {
      key: 'copy-link',
      label: <span className='text-gray-600 text-13-18'>Sao chép địa chỉ liên kết</span>,
      icon: <FiCopy size={15} className='text-gray-500' />,
      onClick: () => {
        const fullUrl = href.startsWith('http')
          ? href
          : `${window.location.origin}${href.startsWith('/') ? '' : '/'}${href}`;
        navigator.clipboard.writeText(fullUrl);
        notifySuccess('', 'Đã sao chép địa chỉ liên kết vào clipboard');
      },
    },
  ];

  return (
    <Dropdown menu={{ items: contextMenuItems }} trigger={['contextMenu']}>
      <a
        href={href}
        className={className || 'w-full h-full flex items-center text-inherit no-underline select-none'}
        onClick={(e) => {
          // If normal left-click without modifier keys, invoke onClick or prevent page reload
          if (!e.ctrlKey && !e.metaKey && e.button === 0) {
            e.preventDefault();
            if (onClick) {
              onClick(e);
            }
          }
        }}
      >
        {children}
      </a>
    </Dropdown>
  );
};

export default NavContextMenu;
