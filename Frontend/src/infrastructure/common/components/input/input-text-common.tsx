import { Col, Input, Row } from 'antd';
import { useEffect, useId, useState } from 'react';
import styled from 'styled-components';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';
import { validateLengthForInput } from '@/infrastructure/utils/validate';

export const InputCommonStyled = styled(Input)`
  font-size: 14px;
  line-height: 20px;
  height: 38px;
  background-color: #ffffff;
  border: 1px solid #a6a6a7db !important;
  border-radius: 6px !important;
  &:focus {
    background-color: #ffffff;
  }

  &:hover {
    background-color: #ffffff;
  }

  &.ant-input-outlined[disabled] {
    color: black;
  }
`;

type Props = {
  label: any;
  isLabel?: boolean;
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
  className?: string;
  prefix?: any;
  onCallbackFunction?: any;
  suffix?: any;
  allowClear?: boolean;
};

const InputTextCommon = (props: Props) => {
  const {
    label,
    isLabel = true,
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
    className,
    prefix,
    onCallbackFunction,
    suffix,
    allowClear = false,
  } = props;

  const [value, setValue] = useState<string>('');
  const id = useId();

  const onChange = (e: any) => {
    setValue(e.target.value || '');
    if (onCallbackFunction) {
      onCallbackFunction(e.target.value);
    } else {
      setData({
        [attribute]: e.target.value || null,
      });
    }

    if (setValidate) {
      validateFields({
        isImplicitChange: false,
        label: label,
        key: attribute,
        value: e.target.value != null ? e.target.value.trim() : '',
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules: validateLengthForInput(maxLength, rules),
      });
    }
  };

  const validateOnBlur = (isImplicitChange = false) => {
    if (setValidate) {
      validateFields({
        isImplicitChange: isImplicitChange,
        label: label,
        key: attribute,
        value: value?.trim(),
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules: validateLengthForInput(maxLength, rules),
      });
    }
  };

  useEffect(() => {
    setValue(dataAttribute || '');
  }, [dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      validateOnBlur(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common size-full flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      {isLabel && label && (
        <Row className='mb-1 h-5'>
          <Col className={'font-bold text-14-20 text-[#3B3B3B]'}>
            <label htmlFor={id}>{label}</label>
          </Col>
          <Col className='is-required'>
            {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
          </Col>
        </Row>
      )}
      <InputCommonStyled
        id={id}
        name={name ?? attribute}
        autoComplete={autoComplete ?? attribute}
        autoFocus={autoFocus}
        size={size ? size : 'large'}
        value={value ? value : ''}
        onChange={onChange}
        onBlur={() => validateOnBlur(false)}
        disabled={disabled}
        placeholder={placeholder}
        className={className}
        prefix={prefix}
        suffix={suffix}
        allowClear={allowClear}
      />
      {validate ? (
        <MessageError
          isError={validate[`${attribute}`]?.isError || false}
          message={validate[`${attribute}`]?.message || ''}
          cssRelative={cssRelative || false}
        />
      ) : null}
    </div>
  );
};
export default InputTextCommon;
