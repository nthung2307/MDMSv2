import { Button, Tooltip } from 'antd';

type Props = {
  title: string | null;
  icon?: any;
  disabled?: boolean;
  classButton?: string;
  htmlType?: 'submit' | 'reset' | 'button';
  ghost?: boolean;
  tooltipText?: string;
  onClick?: () => void;
  variant?: 'outlined' | 'dashed' | 'solid' | 'filled' | 'text' | 'link';
  color?: 'default' | 'primary' | 'danger' | undefined;
};
export const ButtonCommonCustomizable = (props: Props) => {
  const {
    title = null,
    icon,
    onClick,
    disabled,
    classButton,
    htmlType = 'button',
    ghost = false,
    tooltipText,
    color = 'primary',
    variant,
  } = props;
  return (
    <div className={`container-btn-common`}>
      {tooltipText ? (
        <Tooltip placement='bottomRight' title={tooltipText ?? ''}>
          <Button
            className={classButton || ''}
            onClick={onClick}
            icon={icon}
            disabled={disabled}
            htmlType={htmlType}
            ghost={ghost}
            color={color}
            variant={variant}
          >
            {title}
          </Button>
        </Tooltip>
      ) : (
        <Button
          className={classButton || ''}
          onClick={onClick}
          icon={icon}
          disabled={disabled}
          htmlType={htmlType}
          ghost={ghost}
          color={color}
          variant={variant}
        >
          {title}
        </Button>
      )}
    </div>
  );
};
