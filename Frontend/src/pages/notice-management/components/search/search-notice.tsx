import { IoSearch } from 'react-icons/io5';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import { _t } from '@/infrastructure/utils/translation';

type Props = {
  handleClearSearch: any;
  setDataForm: any;
  dataForm: any;
};

const SearchNotice = (props: Props) => {
  const { dataForm, setDataForm, handleClearSearch } = props;

  return (
    <div className='flex items-center gap-1 basis-1/2'>
      <div className='flex items-center gap-3 basis-4/5'>
        <div className='basis-1/2'>
          <InputTextCommon
            label={null}
            attribute={'fullName'}
            dataAttribute={dataForm?.fullName}
            placeholder={_t('Nhập tên khách hàng')}
            setData={setDataForm}
            isRequired={false}
            prefix={<IoSearch />}
            allowClear={true}
          />
        </div>
        <div className='basis-1/2'>
          <InputTextCommon
            label={null}
            attribute={'measurementPoint'}
            dataAttribute={dataForm?.measurementPoint}
            placeholder={_t('Nhập mã điểm đo')}
            setData={setDataForm}
            isRequired={false}
            prefix={<IoSearch />}
            allowClear={true}
          />
        </div>
      </div>
      <div className='basis-1/5'>
        {dataForm?.fullName || dataForm?.measurementPoint ? (
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xem tất cả'
            icon={<IoBackspaceOutline size={20} />}
            onClick={() => {
              handleClearSearch();
            }}
            variant={'text'}
          />
        ) : null}
      </div>
    </div>
  );
};

export default SearchNotice;
