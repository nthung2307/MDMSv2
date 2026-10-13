import { useEffect, useId, useState } from 'react';
import { NumericFormat } from 'react-number-format';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError';
import { InputCommonStyled } from '@/infrastructure/common/components/input/input-text-common';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers';

type Props = {
  label: any;
  labelColor?: string;
  attribute: string;
  dataAttribute?: any;
  data?: any;
  setData: (data: any) => void;
  validate?: any;
  setValidate?: (validate: any) => void;
  isRequired: boolean;
  disabled?: boolean;
  size?: any;
  placeholder?: any;
  rules?: Array<RulesReg>;
  submittedTime?: any;
  type?: string;
  isLabel?: boolean;
  prefix?: any;
  onCallbackFunction?: (data: any) => void;
  name?: string;
  autoComplete?: string;
  isInteger?: boolean;
  addonAfter?: any;
  isCol?: boolean;
  min?: number;
  max?: number;
  allowNegative?: boolean;
  autoFocus?: boolean;
  maxLength?: number;
  allowLeadingZeros?: boolean;
};

const InputNumberCalculateCommon = (props: Props) => {
  const {
    label,
    labelColor = '#000000',
    attribute,
    isRequired,
    dataAttribute,
    setData,
    setValidate,
    validate,
    disabled = false,
    size,
    placeholder,
    rules,
    submittedTime,
    isLabel = true,
    onCallbackFunction,
    name,
    autoComplete,
    isInteger = true,
    addonAfter,
    isCol = true,
    // min,
    // max,
    allowNegative = false,
    autoFocus = false,
    maxLength = 255,
    allowLeadingZeros = true,
  } = props;
  const [value, setValue] = useState<number | null>(null);
  const id = useId();
  const onChange = async (e: any) => {
    setValue(e);
    if (onCallbackFunction) {
      onCallbackFunction(e);
    } else {
      setData({
        [attribute]: e,
      });
    }

    if (setValidate) {
      validateFields({
        isImplicitChange: false,
        label: label,
        key: attribute,
        value: e != undefined ? e.toString().trim() : '',
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
        value: value != undefined ? value.toString().trim() : '',
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules,
      });
    }
  };
  useEffect(() => {
    if (dataAttribute !== undefined) {
      setValue(dataAttribute);
    }
  }, [dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      onBlur(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common flex ${isCol ? 'flex-col gap-1' : 'flex-row gap-2'} h-full`}>
      {isLabel && (
        <div className='gap-1 truncate'>
          <div
            style={{ color: labelColor }}
            className={`text-14-20 font-bold ${isCol ? '' : 'mt-2'} whitespace-nowrap`}
          >
            {label}
            {isRequired ? <span className='text-error text-14-20 pl-1'>*</span> : null}
          </div>
        </div>
      )}
      <div className={'flex-1'}>
        <NumericFormat
          customInput={InputCommonStyled}
          id={id}
          autoFocus={autoFocus}
          allowNegative={allowNegative}
          name={name || attribute}
          autoComplete={autoComplete || attribute}
          size={size ? size : 'large'}
          decimalScale={isInteger ? 0 : 2}
          // fixedDecimalScale={!isInteger}
          thousandSeparator={'.'}
          decimalSeparator={','}
          value={value != undefined ? value : ''}
          onValueChange={(values) => {
            const { floatValue } = values;
            onChange(floatValue);
          }}
          onBlur={() => onBlur(false)}
          disabled={disabled}
          placeholder={placeholder}
          addonAfter={addonAfter}
          maxLength={maxLength}
          allowLeadingZeros={allowLeadingZeros}
        />
        {validate ? (
          <MessageError
            isError={validate[`${attribute}`]?.isError || false}
            message={validate[`${attribute}`]?.message || ''}
          />
        ) : null}
      </div>
    </div>
  );
};
export default InputNumberCalculateCommon;
