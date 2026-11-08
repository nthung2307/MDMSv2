import React from 'react';

interface NavContextMenuProps {
  href: string;
  title?: string;
  children: React.ReactNode;
  className?: string;
  onClick?: (e: React.MouseEvent) => void;
}

export const NavContextMenu: React.FC<NavContextMenuProps> = ({
  href,
  children,
  className,
  onClick,
}) => {
  return (
    <a
      href={href}
      className={className || 'w-full h-full flex items-center text-inherit no-underline'}
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
  );
};

export default NavContextMenu;
