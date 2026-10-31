import { Dropdown, MenuProps } from 'antd';
import React from 'react';
import { FiColumns, FiCopy, FiExternalLink, FiEye, FiMaximize2 } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';
import { useSetRecoilState } from 'recoil';

import { splitViewState } from '@/core/application/common/atoms/global/splitViewState';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';

interface ActionDetailLinkProps {
  href: string;
  title?: string;
  label?: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
  onClick?: (e: React.MouseEvent) => void;
  showIcon?: boolean;
}

export const ActionDetailLink: React.FC<ActionDetailLinkProps> = ({
  href,
  title,
  label = 'Chi tiết',
  icon,
  className,
  onClick,
  showIcon = true,
}) => {
  const navigate = useNavigate();
  const setSplitView = useSetRecoilState(splitViewState);
  const displayTitle = typeof title === 'string' ? title : typeof label === 'string' ? label : 'Chi tiết';

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
          title: displayTitle,
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
        className={
          className ||
          'text-blue-600 hover:text-blue-800 font-medium hover:underline inline-flex items-center gap-1.5 cursor-pointer select-none text-13-18'
        }
        onClick={(e) => {
          // If normal left-click without modifier keys
          if (!e.ctrlKey && !e.metaKey && e.button === 0) {
            e.preventDefault();
            if (onClick) {
              onClick(e);
            } else {
              navigate(href);
            }
          }
        }}
      >
        {showIcon && (icon || <FiEye size={14} className='flex-shrink-0' />)}
        <span>{label}</span>
      </a>
    </Dropdown>
  );
};

export default ActionDetailLink;
