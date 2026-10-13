import { Switch } from 'antd';
import { useEffect, useId, useState } from 'react';

interface SwitchTextValue {
  data: any;
  expectedData: [
    {
      key: boolean;
      value: string;
    },
    {
      key: boolean;
      value: string;
    },
  ];

  setData(e: any): void;

  attribute: string;
  onCallBackFunction?: any;
  disabled?: boolean;
  isLabel?: boolean;
  label: string;
  isRequired: boolean;
  isCol?: boolean;
  reverseLabel?: boolean;
  setValidate?: any;
  validate?: any;
  asterisk?: boolean;
}

const SwitchTextValue = (props: SwitchTextValue) => {
  const {
    data,
    setData,
    expectedData,
    attribute,
    onCallBackFunction,
    disabled,
    isLabel = true,
    label,
    isRequired,
    isCol = false,
    reverseLabel = false,
    asterisk = true,
  } = props;
  const id = useId();
  const [value, setValue] = useState<boolean>(false);

  const convertExpectedData = (value: boolean) => {
    if (expectedData) {
      const result = expectedData.find((item) => item.key === value);
      return result?.value;
    }
    return value;
  };

  const convertExpectedDataToBoolean = (value: string) => {
    const result = expectedData?.find((item) => item.value === value);
    return result?.key;
  };

  const onChange = (value: boolean) => {
    setValue(value);
    if (onCallBackFunction) {
      onCallBackFunction(value);
    } else {
      setData({
        [attribute]: convertExpectedData(value),
      });
    }
  };

  useEffect(() => {
    setValue(convertExpectedDataToBoolean(data[attribute]) ?? false);
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
                {isRequired && asterisk ? (
                  <span className='text-error text-14-20 pl-1'>*</span>
                ) : null}
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
                {isRequired && asterisk ? (
                  <span className='text-error text-14-20 pl-1'>*</span>
                ) : null}
              </label>
            </div>
          )}
          <Switch id={id} value={value} onChange={onChange} disabled={disabled} />
        </>
      )}
    </div>
  );
};

export default SwitchTextValue;
