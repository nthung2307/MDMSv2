import { Col, Empty, Row, Select, Tooltip } from 'antd';
import { useEffect, useId, useState } from 'react';
import styled from 'styled-components';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';

export const InputSelectCommonStyled = styled(Select)`
  height: 44px !important;
  &.ant-select-outlined.ant-select-disabled:not(.ant-select-customize-input) .ant-select-selector {
    color: black;
  }
  &.ant-select-single.ant-select-lg .ant-select-selector {
    font-size: 14px !important;
    line-height: 20px !important;
  }
`;

type Props = {
  label: any;
  attribute: string;
  dataAttribute?: any;
  setData: (data: any) => void;
  validate?: any;
  setValidate?: (validate: any) => void;
  isRequired: boolean;
  disabled?: boolean;
  size?: any;
  isLabel?: boolean;
  placeholder?: any;
  rules?: Array<RulesReg>;
  submittedTime?: any;
  cssRelative?: boolean;
  listDataItem: Array<{
    value: string | number;
    label: string;
    disabled?: boolean;
    tooltipLabel?: string | number;
  }>;
  autoFocus?: boolean;
  onCallBackFunction?: (value: string) => void;
  allowClear?: boolean;
  loading?: boolean;
};

const InputSelectCommon = (props: Props) => {
  const {
    label,
    attribute,
    isRequired,
    dataAttribute,
    setData,
    setValidate,
    validate,
    disabled = false,
    size,
    isLabel = true,
    placeholder,
    rules,
    submittedTime,
    cssRelative,
    listDataItem,
    autoFocus = false,
    onCallBackFunction,
    allowClear = false,
    loading = false,
  } = props;

  const id = useId();
  const [value, setValue] = useState<string | null>(null);
  const [dataTooltip, setDataTooltip] = useState<any>(null);

  const onChange = async (e: any, items: any) => {
    setValue(e || null);
    setDataTooltip(items);

    if (onCallBackFunction) {
      onCallBackFunction(items);
    } else {
      setData({
        [attribute]: e || null,
      });
    }

    if (setValidate) {
      validateFields({
        isImplicitChange: false,
        label: label,
        key: attribute,
        value: e != null ? e : '',
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
        value: value != null ? value : '',
        isRequired: isRequired,
        setError: setValidate,
        error: validate,
        rules,
      });
    }
  };

  useEffect(() => {
    setValue(dataAttribute || null);
  }, [dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      onBlur(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      <Tooltip
        placement='bottomRight'
        title={dataTooltip?.tooltipLabel ?? dataTooltip?.label ?? null}
      >
        {isLabel ? (
          <Row className='mb-1 flex-1'>
            <Col className={'font-bold font-14-20 text-[#3B3B3B]'}>
              <label htmlFor={id}>{label}</label>
            </Col>
            <Col className='is-required'>
              {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
            </Col>
          </Row>
        ) : null}
        <InputSelectCommonStyled
          id={id}
          className='w-full'
          listHeight={160}
          autoFocus={autoFocus}
          size={size ? size : 'large'}
          value={value ? value : null}
          onChange={onChange}
          onBlur={() => onBlur(false)}
          disabled={disabled}
          placeholder={placeholder}
          options={listDataItem ?? []}
          allowClear={allowClear}
          loading={loading}
          notFoundContent={
            loading ? 'Đang tải dữ liệu...' : <Empty description='Không có dữ liệu' />
          }
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
export default InputSelectCommon;
