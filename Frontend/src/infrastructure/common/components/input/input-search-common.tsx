import { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { IoSearch } from 'react-icons/io5';

import { InputCommonStyled } from '@/infrastructure/common/components/input/input-text-common';
type Props = {
  data: any;
  visible?: boolean;
  setData: Dispatch<SetStateAction<any>>;
  disabled?: boolean;
  size?: any;
  placeholder?: any;
  suffix?: React.ReactNode;
  className?: string;
};

const InputTextSearchCommon = (props: Props) => {
  const { data, setData, disabled = false, size, placeholder, suffix, className } = props;
  const [value, setValue] = useState<string>('');

  const onChange = async (e: any) => {
    setValue(e.target.value || '');
    setData(e || '');
  };

  useEffect(() => {
    setValue(data);
  }, [data]);

  return (
    <InputCommonStyled
      size={size ? size : 'large'}
      value={value ? value : ''}
      onChange={onChange}
      disabled={disabled}
      placeholder={placeholder}
      suffix={suffix}
      prefix={<IoSearch />}
      className={className}
    />
  );
};
export default InputTextSearchCommon;
