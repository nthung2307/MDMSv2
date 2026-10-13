import { Col, Empty, Row, Spin, Tooltip } from 'antd';
import { LabeledValue } from 'antd/es/select';
import { useEffect, useId, useState } from 'react';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { InputSelectCommonStyled } from '@/infrastructure/common/components/input/input-select-common';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';

interface OptionType {
  value: string | number;
  label: string;
  disabled?: boolean;
  tooltipLabel?: string | number;
}

export interface InputSelectDebounceSearchCommonProps {
  label: any;
  attribute: string;
  dataAttribute?: any;
  setData: (data: any) => void;
  validate?: any;
  setValidate?: (validate: any) => void;
  isRequired: boolean;
  disabled?: boolean;
  size?: 'small' | 'middle' | 'large';
  isLabel?: boolean;
  placeholder?: string;
  rules?: Array<RulesReg>;
  submittedTime?: any;
  cssRelative?: boolean;
  listDataItem?: Array<OptionType>;
  autoFocus?: boolean;
  onCallBackFunction?: (value: string | number | (string | number)[]) => void;
  allowClear?: boolean;
  loading?: boolean;
  mode?: 'multiple' | 'tags' | undefined;
  onSearch?: (value: string) => void;
  fetchOptions?: (search: string) => Promise<OptionType[]>;
  debounceTimeout?: number;
}

const InputDebounceSelectCommon: React.FC<InputSelectDebounceSearchCommonProps> = ({
  label,
  attribute,
  isRequired,
  dataAttribute,
  setData,
  setValidate,
  validate,
  disabled = false,
  size = 'large',
  isLabel = true,
  placeholder,
  rules,
  submittedTime,
  cssRelative,
  listDataItem = [],
  autoFocus = false,
  onCallBackFunction,
  allowClear = false,
  loading = false,
  mode = undefined,
  onSearch,
  fetchOptions,
  debounceTimeout = 500, // Default debounce timeout is set to 500ms
}) => {
  const id = useId();
  const [value, setValue] = useState<string | LabeledValue[] | null>(dataAttribute || null);
  const [dataTooltip, setDataTooltip] = useState<any>(null);
  const [options, setOptions] = useState<OptionType[]>(listDataItem);
  const [fetching, setFetching] = useState(false);

  const debouncedSearchTerm = useDebounce(value, debounceTimeout); // Using the custom debounce hook

  useEffect(() => {
    // Only fetch options if the mode is multiple and fetchOptions is provided
    if (fetchOptions && mode === 'multiple' && debouncedSearchTerm) {
      setFetching(true);
      fetchOptions(debouncedSearchTerm as string)
        .then((newOptions) => {
          setOptions(newOptions);
          setFetching(false);
        })
        .catch(() => {
          setOptions([]);
          setFetching(false);
        });
    }
  }, [debouncedSearchTerm, fetchOptions, mode]);

  const handleSearch = (search: string) => {
    if (mode === 'multiple') {
      setValue(search);
    }
    if (onSearch) {
      onSearch(search);
    }
  };

  const onChange = async (e: any, items: any) => {
    setValue(e || null);
    setDataTooltip(items);

    if (onCallBackFunction) {
      onCallBackFunction(e || items);
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

  const onBlurHandler = (isImplicitChange = false) => {
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

  // Update value when dataAttribute changes
  useEffect(() => {
    setValue(dataAttribute || null);
  }, [dataAttribute]);

  // Validate on form submission
  useEffect(() => {
    if (submittedTime != null) {
      onBlurHandler(true);
    }
  }, [submittedTime]);

  return (
    <div className={`input-common flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      <Tooltip
        placement='bottomRight'
        title={dataTooltip?.tooltipLabel ?? dataTooltip?.label ?? null}
      >
        {isLabel ? (
          <Row className='flex-1'>
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
          size={size}
          value={value}
          onChange={onChange}
          onBlur={() => onBlurHandler(false)}
          disabled={disabled}
          placeholder={placeholder}
          options={mode === 'multiple' && options.length > 0 ? options : listDataItem}
          allowClear={allowClear}
          loading={fetching || loading}
          onSearch={mode === 'multiple' ? handleSearch : undefined}
          notFoundContent={
            fetching || loading ? <Spin size='small' /> : <Empty description='Không có dữ liệu' />
          }
          mode={mode}
          showSearch={mode === 'multiple'}
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

export default InputDebounceSelectCommon;
