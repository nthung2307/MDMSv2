import { SizeType } from 'antd/es/config-provider/SizeContext';
import { Dispatch, SetStateAction, useEffect, useState } from 'react';

import { InputSelectCommonStyled } from '@/infrastructure/common/components/input/input-select-common';

type Props = {
  data: any;
  visible?: boolean;
  setData: Dispatch<SetStateAction<any>>;
  disabled?: boolean;
  size?: SizeType;
  placeholder?: string;
  listDataItem: Array<{ value: string; label: string; disabled?: boolean }>;
};

const InputSelectSearchCommon = (props: Props) => {
  const { data, setData, disabled = false, size, placeholder, listDataItem } = props;
  const [value, setValue] = useState<any>();

  const onChange = (e: any) => {
    setValue(e || '');
    setData(e || '');
  };

  useEffect(() => {
    setValue(data);
  }, [data]);

  return (
    <InputSelectCommonStyled
      className='w-full'
      size={size ? size : 'large'}
      options={listDataItem ?? []}
      value={value ? value : null}
      onChange={onChange}
      disabled={disabled}
      placeholder={placeholder}
      allowClear
    />
  );
};
export default InputSelectSearchCommon;
