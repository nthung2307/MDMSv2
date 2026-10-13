import { IoSearch } from 'react-icons/io5';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import { _t } from '@/infrastructure/utils/translation';

type Props = {
  handleClearSearch: any;
  setDataForm: any;
  dataForm: any;
  handleSearch: () => void;
};

const SearchCheckDetail = (props: Props) => {
  const { dataForm, setDataForm, handleClearSearch } = props;

  return (
    <div className={'grow'}>
      <div className='flex gap-1 items-center'>
        <div className='basis-1/2 flex items-center gap-3'>
          <div className='basis-1/2'>
            <InputTextCommon
              label={null}
              attribute={'measurementPointId'}
              dataAttribute={dataForm?.measurementPointId}
              placeholder={_t('Nhập mã điểm đo')}
              setData={setDataForm}
              isRequired={false}
              prefix={<IoSearch />}
            />
          </div>
          <div className='basis-1/2'>
            <InputTextCommon
              label={null}
              attribute={'fullName'}
              dataAttribute={dataForm?.fullName}
              placeholder={_t('Nhập tên khách hàng')}
              setData={setDataForm}
              isRequired={false}
            />
          </div>
          {/* <div className='basis-1/2'>
            <InputSelectCommon
              label={null}
              attribute={'isChange'}
              dataAttribute={dataForm?.isChange}
              placeholder={_t('Chỉ số bị lỗi')}
              setData={setDataForm}
              allowClear={true}
              isRequired={false}
              listDataItem={[
                { label: 'Có lỗi', value: 1 },
                { label: 'Không có lối', value: 2 },
              ]}
            />
          </div> */}
        </div>
        <div className='flex gap-1'>
          {(dataForm?.measurementPointId || dataForm?.fullName) && (
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              icon={<IoBackspaceOutline size={25} />}
              onClick={() => {
                handleClearSearch();
              }}
              variant={'text'}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default SearchCheckDetail;
