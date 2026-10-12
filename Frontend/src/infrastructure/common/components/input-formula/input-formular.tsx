import { SearchOutlined } from '@ant-design/icons';
import { Col, Input, Row, Tabs } from 'antd';
import { useEffect, useId, useRef, useState } from 'react';

import { MessageError } from '@/infrastructure/common/components/controls/MessageError.tsx';
import { InputCommonStyled } from '@/infrastructure/common/components/input/input-text-common';
import { RulesReg, validateFields } from '@/infrastructure/utils/helpers.ts';

type Props = {
  label: any;
  attribute: string;
  dataAttribute?: any;
  name?: string;
  autoComplete?: string;
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
  listParams?: Array<any>;
};

const InputFormula = (props: Props) => {
  const {
    label,
    attribute,
    isRequired,
    dataAttribute,
    name,
    autoComplete,
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
    listParams = [],
  } = props;

  const [value, setValue] = useState<string>('');
  const [isDropdown, setIsDropdown] = useState(false);
  const [searchValue, setSearchValue] = useState<string>('');
  const [paramsFilter, setParamsFilter] = useState<Array<any>>([]);
  const wrapperRef = useRef<HTMLDivElement>(null);
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

  const onSearch = (value: any) => {
    setSearchValue(value.target.value);
    let filter = listParams.filter((item) =>
      item.toLowerCase().includes(value.target.value.toLowerCase())
    );
    setParamsFilter(filter);
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

  const onClickItem = (item: any) => {
    const newValue = value ? value + item : item;
    setValue(newValue);
    setData({
      [attribute]: newValue,
    });

    document.getElementById(id)?.focus();
  };
  useEffect(() => {
    setValue(dataAttribute);
    if (listParams.length > 0) {
      setParamsFilter(listParams);
    }
  }, [listParams, dataAttribute]);

  useEffect(() => {
    if (submittedTime != null) {
      onBlur(true);
    }
  }, [submittedTime]);

  useEffect(() => {
    /**
     * Alert if clicked on outside of element
     */
    function handleClickOutside(event: { target: any }) {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setIsDropdown(false);
      }
    }
    // Bind the event listener
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      // Unbind the event listener on clean up
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [wrapperRef]);

  return (
    <div className={`input-common flex flex-col h-full ${cssRelative ? 'relative' : ''}`}>
      <Row className='mb-1'>
        <Col className={'font-bold text-14-20 text-[#3B3B3B]'}>
          <label htmlFor={id}>{label}</label>
        </Col>
        <Col className='is-required'>
          {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
        </Col>
      </Row>
      <div ref={wrapperRef} id='input-formula'>
        <InputCommonStyled
          id={id}
          onFocus={() => setIsDropdown(true)}
          name={name ?? attribute}
          autoComplete={autoComplete ?? attribute}
          size={size ? size : 'large'}
          value={value ? value : ''}
          onChange={onChange}
          disabled={disabled}
          placeholder={placeholder}
          maxLength={maxLength}
        />
        {isDropdown ? (
          <div className='absolute w-full bg-white z-max shadow-xl rounded-lg p-2 mt-1'>
            <Tabs defaultActiveKey='1'>
              <Tabs.TabPane tab='Tham số' key='1'>
                <div className='flex flex-col gap-4 p-2'>
                  <Input value={searchValue} onChange={onSearch} prefix={<SearchOutlined />} />
                  <div className='max-h-40 overflow-auto'>
                    {paramsFilter.map((item: any, index: number) => {
                      return (
                        <div
                          key={index}
                          className='flex items-center gap-1 p-2 border-b border-gray-200 hover:bg-white-004 cursor-pointer'
                          onClick={() => onClickItem(item)}
                        >
                          <p>f(x) = </p>
                          <p>{item}</p>
                        </div>
                      );
                    })}
                  </div>
                </div>
              </Tabs.TabPane>
              <Tabs.TabPane tab='Toán tử' key='2'>
                Content of Tab Pane 2
              </Tabs.TabPane>
            </Tabs>
          </div>
        ) : null}
      </div>
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
export default InputFormula;
