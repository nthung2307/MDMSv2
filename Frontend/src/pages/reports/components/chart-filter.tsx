import { Col, DatePicker, Radio, Row, Select } from 'antd';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { Dispatch, SetStateAction, useEffect, useState } from 'react';
import styled from 'styled-components';

import Constants from '@/core/application/common/constants.ts';

// Extend dayjs with UTC plugin
dayjs.extend(utc);

// Styled components
const StyledRadioGroup = styled(Radio.Group)`
  .ant-radio-button-wrapper {
    background-color: white !important;
    color: black !important;
  }
  .ant-radio-button-wrapper-checked {
    background-color: #b22222 !important;
    color: white !important;
  }
`;

const StyledDatePicker = styled(DatePicker)`
  background-color: white !important;
`;

const StyledSelect = styled(Select)`
  .ant-select-selector {
    background-color: white !important;
    border: 1px solid #d9d9d9;
    border-radius: 6px !important;
    height: 44px;
  }
`;

// Props and State Types
interface ChartFilterProps {
  radioOptions: Array<{ label: string; value: string }>;
  chartTypeOptions: Array<{ label: string; value: string }>;
  setFilterData: Dispatch<SetStateAction<any>>;
  isPreviousPeriod?: boolean;
}

export interface FilterStateValue {
  radioValue: string;
  chartType?: string;
  date: string;
  previousPeriod: boolean;
}

// Helper to determine DatePicker type
const getDatePickerType = (radioValue: string): 'date' | 'month' | 'year' => {
  switch (radioValue) {
    case Constants.Reports.TimeSelector.General.Daily.value:
      return 'date';
    case Constants.Reports.TimeSelector.General.Monthly.value:
      return 'month';
    case Constants.Reports.TimeSelector.General.Yearly.value:
      return 'year';
    default:
      return 'date';
  }
};

// Component
const ChartFilter: React.FC<ChartFilterProps> = ({
  radioOptions = [],
  chartTypeOptions = [],
  setFilterData = () => {},
  isPreviousPeriod = false,
}) => {
  // Single state object
  const [filterValue, setFilterValue] = useState<FilterStateValue>({
    radioValue: radioOptions[0]?.value || '',
    chartType: chartTypeOptions[0]?.value || '',
    date: dayjs().toISOString(),
    previousPeriod: false,
  });

  // Update radioValue if radioOptions change
  useEffect(() => {
    if (radioOptions.length > 0 && radioOptions[0].value !== filterValue.radioValue) {
      const newFilterValue = {
        ...filterValue,
        radioValue: radioOptions[0].value,
      };
      setFilterValue(newFilterValue);
      setFilterData(newFilterValue);
    }
  }, [radioOptions]);

  // Handle filter changes
  const handleFilterChange = (key: keyof FilterStateValue, value: any) => {
    const processedValue = key === 'date' ? dayjs(value).toISOString() : value;

    // Skip update if value hasn't changed
    if (filterValue[key] === processedValue) {
      return;
    }

    const newFilterValue = {
      ...filterValue,
      [key]: processedValue,
    };

    setFilterValue(newFilterValue);
    setFilterData(newFilterValue);
  };

  return (
    <Row gutter={[12, 12]} className='gap-y-12 xl:gap-y-0'>
      {/* Left Section: Radio Group and Date Picker */}
      <Col xs={24} xl={12}>
        <div className='flex gap-3'>
          <StyledRadioGroup
            options={radioOptions}
            optionType='button'
            buttonStyle='solid'
            size='middle'
            value={filterValue.radioValue}
            onChange={(e) => handleFilterChange('radioValue', e.target.value)}
          />
          <StyledDatePicker
            format={
              getDatePickerType(filterValue.radioValue) === 'month'
                ? 'MM/YYYY'
                : getDatePickerType(filterValue.radioValue) === 'year'
                  ? 'YYYY'
                  : Constants.DateTime.DateFormat
            }
            value={dayjs(filterValue.date)}
            onChange={(date) => date && handleFilterChange('date', date)}
            allowClear={false}
            picker={getDatePickerType(filterValue.radioValue)}
          />
        </div>
      </Col>

      {/* Right Section: Chart Type Select and Optional Checkbox */}
      <Col xs={24} xl={12} className='xl:flex xl:justify-end'>
        <div className='flex gap-3 items-center'>
          {isPreviousPeriod && (
            <div>
              <input
                type='checkbox'
                checked={filterValue.previousPeriod}
                onChange={() => handleFilterChange('previousPeriod', !filterValue.previousPeriod)}
              />
              <label>Kì trước</label>
            </div>
          )}
          <StyledSelect
            className='min-w-[250px] bg-white'
            options={chartTypeOptions}
            value={filterValue.chartType}
            onChange={(value) => handleFilterChange('chartType', value)}
          />
        </div>
      </Col>
    </Row>
  );
};

export default ChartFilter;
