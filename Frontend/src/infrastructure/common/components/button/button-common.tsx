import { Button, Tooltip } from 'antd';

import { onScrollToParentOfMessageError } from '@/infrastructure/utils/helpers';

type Props = {
  title: string | null;
  colorClass: 'blue-bg' | 'firebrick-bg' | 'firebrick-outline-bg' | 'cancel';
  icon?: any;
  disabled?: boolean;
  onClick: (callback?: () => void) => void;
  classButton?: string;
  isScrollToError?: boolean;
  htmlType?: 'submit' | 'reset' | 'button';
  ghost?: boolean;
  tooltipText?: string;
  loading?: boolean;
};
export const ButtonCommon = (props: Props) => {
  const {
    title = null,
    colorClass,
    icon,
    onClick,
    disabled,
    classButton,
    isScrollToError = false,
    htmlType = 'button',
    ghost = false,
    tooltipText,
    loading = false,
  } = props;
  return (
    //blue-bg: background xanh, chữ trắng
    //cancel: background trắng, chữ xanh
    //firebrick: background đỏ, chữ trắng
    <div className={`${colorClass}-button-common container-btn-common`}>
      {tooltipText ? (
        <Tooltip placement='bottomRight' title={tooltipText ?? ''}>
          <Button
            className={classButton || ''}
            onClick={() => {
              onClick(() => {
                if (isScrollToError) {
                  onScrollToParentOfMessageError();
                }
              });
            }}
            icon={icon}
            disabled={disabled}
            htmlType={htmlType}
            ghost={ghost}
            loading={loading}
          >
            {title}
          </Button>
        </Tooltip>
      ) : (
        <Button
          className={classButton || ''}
          onClick={() => {
            onClick(() => {
              if (isScrollToError) {
                onScrollToParentOfMessageError();
              }
            });
          }}
          icon={icon}
          disabled={disabled}
          htmlType={htmlType}
          ghost={ghost}
          loading={loading}
        >
          {title}
        </Button>
      )}
    </div>
  );
};
