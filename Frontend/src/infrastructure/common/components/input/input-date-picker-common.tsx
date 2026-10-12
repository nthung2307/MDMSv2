import { Col, DatePicker, Row, Tooltip } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useId, useState } from 'react';
import styled from 'styled-components';

import Constants from '@/core/application/common/constants';
import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';

export const DatePickerStyled = styled(DatePicker)`
  height: 44px;

  .ant-picker-input > input {
    font-size: 14px;
    line-height: 20px;
  }

  &.ant-picker .ant-picker-input > input[disabled] {
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
  onCallBackFunction?: (data: any) => void;
  disabledDate?: (current: any) => boolean;
};

const InputDatePickerCommon = (props: Props) => {
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
    onCallBackFunction,
    disabledDate,
  } = props;

  const [value, setValue] = useState<any>(null);
  const [dateString, setDateString] = useState<string | string[]>('');
  const id = useId();
  const onChange = (e: any, dateString: string | string[]) => {
    setDateString(dateString);
    setValue(e || null);
    if (onCallBackFunction) {
      onCallBackFunction(e || null);
    } else {
      setData({
        [attribute]: dayjs(e).isValid() ? dayjs(e).toDate() : null,
      });
    }
    if (setValidate) {
      validateFields({
        isImplicitChange: false,
        label: label,
        key: attribute,
        value: dayjs(e).isValid() ? dayjs(e).format(Constants.DateTime.DateFormat) : null,
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
        value: dayjs(value).isValid() ? dayjs(value).format(Constants.DateTime.DateFormat) : null,
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules,
      });
    }
  };

  useEffect(() => {
    if (dataAttribute) {
      setValue(dayjs(dataAttribute));
    }
  }, [dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      onBlur(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      <Tooltip placement='bottomRight' title={dateString}>
        <Row className='mb-1 flex-1'>
          <Col className={'font-bold font-14-20 text-[#3B3B3B]'}>
            <label htmlFor={id}>{label}</label>
          </Col>
          <Col className='is-required'>
            {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
          </Col>
        </Row>
        <DatePickerStyled
          id={id}
          className='w-full'
          name={name ?? attribute}
          disabledDate={disabledDate}
          autoComplete={autoComplete ?? 'off'}
          autoFocus={autoFocus}
          format={['DD/MM/YYYY']}
          size={size ? size : 'large'}
          value={value ?? null}
          onChange={onChange}
          // onBlur={() => onBlur(false)}
          disabled={disabled}
          placeholder={placeholder}
        />
      </Tooltip>
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
export default InputDatePickerCommon;
