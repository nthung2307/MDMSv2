import { Switch } from 'antd';
import { useEffect, useId, useState } from 'react';

interface SwitchCommonProps {
  data: any;

  setData(e: any): void;

  attribute: string;
  onCallBackFunction?: any;
  disabled?: boolean;
  isLabel?: boolean;
  label: string;
  isRequired: boolean;
  isCol?: boolean;
  reverseLabel?: boolean;
}

const SwitchCommon = (props: SwitchCommonProps) => {
  const {
    data,
    setData,
    attribute,
    onCallBackFunction,
    disabled,
    isLabel = true,
    label,
    isRequired,
    isCol = false,
    reverseLabel = false,
  } = props;
  const id = useId();
  const [value, setValue] = useState<boolean>(false);
  const onChange = (value: boolean) => {
    setValue(value);
    if (onCallBackFunction) {
      onCallBackFunction(value);
    } else {
      setData({
        [attribute]: value,
      });
    }
  };

  useEffect(() => {
    setValue(data[attribute]);
  }, [data[attribute]]);

  return (
    <div className={`flex ${isCol ? ' flex-col gap-1' : 'flex-row gap-4'} h-full`}>
      {reverseLabel ? (
        <>
          <Switch id={id} value={value} onChange={onChange} disabled={disabled} />
          {isLabel && (
            <div className=''>
              <label
                className={`text-14-20 font-bold ${isCol ? '' : 'mt-2'} whitespace-nowrap`}
                htmlFor={id}
              >
                {label}
                {isRequired ? <span className='text-error text-14-20 pl-1'>*</span> : null}
              </label>
            </div>
          )}
        </>
      ) : (
        <>
          {isLabel && (
            <div className=''>
              <label
                className={`text-14-20 font-bold ${isCol ? '' : 'mt-2'} whitespace-nowrap`}
                htmlFor={id}
              >
                {label}
                {isRequired ? <span className='text-error text-14-20 pl-1'>*</span> : null}
              </label>
            </div>
          )}
          <Switch id={id} value={value} onChange={onChange} disabled={disabled} />
        </>
      )}
    </div>
  );
};

export default SwitchCommon;
