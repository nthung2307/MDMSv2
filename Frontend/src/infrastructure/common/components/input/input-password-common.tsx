import { Col, Input, Row } from 'antd';
import { useEffect, useId, useState } from 'react';
import styled from 'styled-components';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';

export const InputPasswordStyled = styled(Input.Password)`
  font-size: 14px;
  line-height: 20px;
  height: 44px;

  &.ant-input-outlined[disabled] {
    color: black;
  }
`;

type Props = {
  label: any;
  attribute: string;
  dataAttribute?: any;
  name?: string;
  autoComplete?: string;
  autoFocus?: boolean;
  setData: (data: any) => void;
  validate?: any;
  setValidate?: (validate: any) => void;
  isRequired: boolean;
  disabled?: boolean;
  size?: any;
  placeholder?: any;
  rules?: Array<RulesReg>;
  submittedTime?: any;
  cssRelative?: boolean;
  maxLength?: number;
};

const InputPasswordCommon = (props: Props) => {
  const {
    label,
    attribute,
    isRequired,
    dataAttribute,
    name,
    autoComplete,
    autoFocus = false,
    setData,
    setValidate,
    validate,
    disabled = false,
    size,
    placeholder,
    rules,
    submittedTime,
    cssRelative,
    maxLength = 255,
  } = props;

  const [value, setValue] = useState<string>('');
  const id = useId();

  const onChange = async (e: any) => {
    setValue(e.target.value || '');
    setData({
      [attribute]: e.target.value || null,
    });

    if (setValidate) {
      validateFields({
        isImplicitChange: false,
        label: label,
        key: attribute,
        value: e.target.value != null ? e.target.value.trim() : '',
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules,
      });
    }
  };

  const onBlur = (isImplicitChange = false) => {
    if (setValidate) {
      validateFields({
        isImplicitChange: isImplicitChange,
        label: label,
        key: attribute,
        value: value?.trim(),
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules,
      });
    }
  };

  useEffect(() => {
    setValue(dataAttribute || '');
  }, [dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      onBlur(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      <Row className='mb-1 flex-1'>
        <Col className={'font-bold text-14-20 text-[#3B3B3B]'}>
          <label htmlFor={id}>{label}</label>
        </Col>
        <Col className='is-required'>
          {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
        </Col>
      </Row>
      <InputPasswordStyled
        id={id}
        name={name ?? attribute}
        autoComplete={autoComplete ?? 'off'}
        autoFocus={autoFocus}
        size={size ? size : 'large'}
        value={value ? value : ''}
        onChange={onChange}
        onBlur={() => onBlur(false)}
        disabled={disabled}
        placeholder={placeholder}
        maxLength={maxLength}
      />
      {validate ? (
        <MessageError
          isError={validate[`${attribute}`]?.isError || false}
          message={validate[`${attribute}`]?.message || ''}
          cssRelative={cssRelative}
        />
      ) : null}
    </div>
  );
};
export default InputPasswordCommon;
