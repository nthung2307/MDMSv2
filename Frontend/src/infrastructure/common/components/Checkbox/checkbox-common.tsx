import { Checkbox, Row } from 'antd';
import React from 'react';

type PropsCheckbox = {
  dataAttribute: any;
  setData: (data: any) => void;
  label: React.ReactNode;
  disabled?: boolean;
};

export const CheckboxCommon = (props: PropsCheckbox) => {
  const { label, dataAttribute, setData, disabled } = props;

  const onChange = (e: any) => {
    setData(e.target.checked);
  };

  return (
    <Row justify='start' align='middle' className='checkbox-container gap-3 text-14-20 font-bold'>
      <Checkbox checked={dataAttribute} disabled={disabled} onChange={onChange} />
      {label}
    </Row>
  );
};
