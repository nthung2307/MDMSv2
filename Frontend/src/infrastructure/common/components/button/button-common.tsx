import { Button } from 'antd';

import { onScrollToParentOfMessageError } from '@/infrastructure/utils/helpers';

type Props = {
  title: string;
  colorClass: 'blue-bg' | 'cancel';
  icon?: any;
  disabled?: boolean;
  onClick: (callback?: () => void) => void;
  classButton?: string;
  isScrollToError?: boolean;
  htmlType?: 'submit' | 'reset' | 'button';
};
export const ButtonCommon = (props: Props) => {
  const {
    title,
    colorClass,
    icon,
    onClick,
    disabled,
    classButton,
    isScrollToError = false,
    htmlType = 'button',
  } = props;
  return (
    //blue-bg: background xanh, chữ trắng
    <div className={`${colorClass}-button-common container-btn-common`}>
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
      >
        {title}
      </Button>
    </div>
  );
};
