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
}

export const ActionDetailLink: React.FC<ActionDetailLinkProps> = ({
  href,
  label = 'Chi tiết',
  icon,
  className,
  onClick,
  showIcon = true,
}) => {
  const navigate = useNavigate();

  return (
    <a
      href={href}
      className={
        className ||
        'text-blue-600 hover:text-blue-800 font-medium hover:underline inline-flex items-center gap-1.5 cursor-pointer text-13-18'
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
      {showIcon && (icon || <FiEye size={14} className='flex-shrink-0' />)}
      <span>{label}</span>
    </a>
  );
};

export default ActionDetailLink;
