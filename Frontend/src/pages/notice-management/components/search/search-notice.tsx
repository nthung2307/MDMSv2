import { IoSearch } from 'react-icons/io5';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
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
    <div className='flex items-center gap-2 basis-2/3'>
      <div className='flex items-center gap-3 basis-11/12'>
        <div className='basis-1/3'>
          <InputTextCommon
            label={null}
            attribute={'customerCode'}
            dataAttribute={dataForm?.customerCode}
            placeholder={_t('Nhập mã khách hàng')}
            setData={setDataForm}
            isRequired={false}
            prefix={<IoSearch />}
            allowClear={true}
          />
        </div>
        <div className='basis-1/3'>
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
        <div className='basis-1/3'>
          <InputSelectCommon
            label={null}
            attribute={'status'}
            dataAttribute={dataForm?.status}
            placeholder={_t('Trạng thái gửi Email')}
            allowClear={true}
            setData={setDataForm}
            isRequired={false}
            listDataItem={[
              { label: 'Chưa gửi', value: 'UNSENT' },
              { label: 'Thành công', value: 'SEND_SUCCESS' },
              { label: 'Thất bại', value: 'SEND_FAILED' },
              { label: 'Chưa cấu hình Email', value: 'NO_EMAIL' },
            ]}
          />
        </div>
      </div>
      <div className='basis-1/12'>
        {dataForm?.customerCode || dataForm?.measurementPoint || dataForm?.status ? (
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
