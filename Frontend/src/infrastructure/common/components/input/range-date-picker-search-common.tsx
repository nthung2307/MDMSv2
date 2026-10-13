import { DatePicker } from 'antd';
import { useEffect, useState } from 'react';
import styled from 'styled-components';

const { RangePicker } = DatePicker;

export const RangePickerStyled = styled(RangePicker)`
  height: 44px;
  border-radius: 6px;
  background-color: white !important;

  .ant-picker-input > input {
    font-size: 14px;
    line-height: 20px;
  }

  &.ant-picker .ant-picker-input > input[disabled] {
    color: black;
  }
`;

type Props = {
  data: any;
  setData: (data: any) => void;
  disabled?: boolean;
  placeholder?: any;
  allowClear?: boolean;
  onCallBackFunction?: (data: any) => void;
  picker?: any;
  disableDate?: any;
  format?: string;
};

const RangeDatePickerSearchCommon = (props: Props) => {
  const {
    data,
    allowClear,
    setData,
    disabled,
    placeholder,
    onCallBackFunction,
    picker,
    disableDate,
    format,
  } = props;

  const [value, setValue] = useState<any>([]);
  const onChange = async (val: any) => {
    setData(val);
    if (onCallBackFunction) {
      onCallBackFunction(val);
    } else {
      setValue(val);
    }
  };

  useEffect(() => {
    setValue(data);
  }, [data]);

  return (
    <RangePickerStyled
      className='w-full'
      size={'large'}
      value={value ? value : []}
      onChange={onChange}
      disabledDate={disableDate}
      disabled={disabled}
      format={format || 'DD/MM/YYYY'}
      allowClear={allowClear}
      placeholder={placeholder}
      picker={picker}
    />
  );
};
export default RangeDatePickerSearchCommon;
