import { Tooltip } from 'antd';
import React from 'react';
import { FiEye } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';

interface ActionDetailLinkProps {
  href: string;
  title?: string;
  label?: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
  onClick?: (e: React.MouseEvent) => void;
  showIcon?: boolean;
  showLabel?: boolean;
}

export const ActionDetailLink: React.FC<ActionDetailLinkProps> = ({
  href,
  title = 'Xem chi tiết',
  label = 'Chi tiết',
  icon,
  className,
  onClick,
  showIcon = true,
  showLabel = false,
}) => {
  const navigate = useNavigate();

  const linkElement = (
    <a
      href={href}
      className={
        className ||
        'text-blue-600 hover:text-blue-800 hover:bg-blue-50 p-1.5 rounded transition-colors inline-flex items-center justify-center gap-1.5 cursor-pointer text-13-18'
      }
      onClick={(e) => {
        // If normal left-click without modifier keys, navigate using client-side routing
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
      {showIcon && (icon || <FiEye size={16} className='flex-shrink-0' />)}
      {showLabel && <span>{label}</span>}
    </a>
  );

  return title ? <Tooltip title={title}>{linkElement}</Tooltip> : linkElement;
};

export default ActionDetailLink;
