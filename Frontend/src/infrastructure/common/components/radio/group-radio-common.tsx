import { Radio } from 'antd';
import { useEffect, useState } from 'react';

interface RadioCommonProps {
  dataAttribute?: any;
  setData(e: any): void;
  attribute: string;
  disabled?: boolean;
  listData: Array<{ label: string; value: string | number; disable?: boolean }>;
  onCallBackFunction?: any;
  isCol?: boolean;
}

const RadioCommon = (props: RadioCommonProps) => {
  const {
    setData,
    dataAttribute,
    disabled = false,
    listData,
    attribute,
    onCallBackFunction,
    isCol = false,
  } = props;

  const [value, setValue] = useState<string | number>('');

  const onChange = (e: any) => {
    setValue(e.target.value);
    if (onCallBackFunction) {
      onCallBackFunction(e.target.value);
    } else {
      setData({
        [attribute]: e.target.value,
      });
    }
  };

  useEffect(() => {
    if (dataAttribute) {
      setValue(dataAttribute);
    }
  }, [dataAttribute]);

  return (
    <Radio.Group value={value} disabled={disabled} onChange={onChange}>
      <div className={`flex gap-8 ${isCol ? 'flex-col' : 'flex-row'}`}>
        {listData.map((item, index) => (
          <Radio key={index} value={item.value} disabled={item.disable}>
            {item.label}
          </Radio>
        ))}
      </div>
    </Radio.Group>
  );
};

export default RadioCommon;
