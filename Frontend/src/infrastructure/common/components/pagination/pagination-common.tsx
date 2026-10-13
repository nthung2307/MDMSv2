import { Button } from 'antd';
import { SetStateAction } from 'jotai';
import { Dispatch } from 'react';
import { IoChevronBackOutline, IoChevronForwardOutline } from 'react-icons/io5';

import { InputSelectCommonStyled } from '@/infrastructure/common/components/input/input-select-common';

interface Props {
  pageSize?: number | 10 | 20 | 50 | 100;
  setSize: Dispatch<SetStateAction<number>>;
  hasNext: boolean;
  hasPrevious: boolean;
  //   onClick?: (callback?: () => void) => void;
  onClickPrevious: (callback?: () => void) => void;
  onClickNext: (callback?: () => void) => void;
}

const PaginationCommon = ({
  pageSize = 10,
  hasNext = false,
  hasPrevious = false,
  onClickNext,
  onClickPrevious,
  setSize,
}: Props) => {
  return (
    <>
      <div className='flex items-center gap-5'>
        <div>
          <Button
            type={'text'}
            icon={<IoChevronBackOutline />}
            onClick={() => {
              onClickPrevious();
            }}
            disabled={!hasPrevious}
            size='large'
          />
          <Button
            type={'text'}
            icon={<IoChevronForwardOutline />}
            onClick={() => {
              onClickNext();
            }}
            disabled={!hasNext}
            size='large'
          />
        </div>
        <div>
          <InputSelectCommonStyled
            defaultValue={pageSize}
            onChange={(value: any) => {
              setSize(value);
            }}
            options={[
              { value: 10, label: '10 / Page' },
              { value: 20, label: '20 / Page' },
              { value: 50, label: '50 / Page' },
              { value: 100, label: '100 / Page' },
            ]}
          />
        </div>
      </div>
    </>
  );
};

export default PaginationCommon;
