import { Checkbox, Row } from 'antd';
import React from 'react';

type PropsCheckbox = {
  dataAttribute: any;
  setData: any;
  label: React.ReactNode;
  disabled?: boolean;
  className?: string;
  value?: any;
  isLabel: boolean;
};

export const CheckboxCustomCommon = (props: PropsCheckbox) => {
  const { label, dataAttribute, setData, disabled, className, value, isLabel = true } = props;

  const onChange = (e: any) => {
    setData(e.target.value);
  };

  return (
    <Row
      justify='start'
      align='middle'
      className={`${className || 'checkbox-container'} flex flex-nowrap`}
    >
      <Checkbox value={value} checked={dataAttribute} disabled={disabled} onChange={onChange} />
      {isLabel && <p className='truncate max-w-full'>{label}</p>}
    </Row>
  );
};
