import { IoSearch } from 'react-icons/io5';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import { _t } from '@/infrastructure/utils/translation';

type Props = {
  handleClearSearch: any;
  setRefreshState: any;
  setDataForm: any;
  dataForm: any;
};

const SearchGCS = (props: Props) => {
  const { dataForm, setDataForm, handleClearSearch, setRefreshState } = props;

  return (
    <div className='grow'>
      <div className='flex items-center gap-1'>
        <div className='basis-3/5 flex items-center gap-3'>
          <div className='basis-1/2'>
            <InputTextCommon
              label={null}
              attribute={'measurementPointId'}
              dataAttribute={dataForm?.measurementPointId}
              placeholder={_t('Nhập mã điểm đo')}
              setData={setDataForm}
              onCallbackFunction={(data: any) => {
                setRefreshState(false);
                setDataForm({ measurementPointId: data });
              }}
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
              onCallbackFunction={(data: any) => {
                setRefreshState(false);
                setDataForm({ fullName: data });
              }}
              isRequired={false}
            />
          </div>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={null}
              attribute={'quanlity'}
              dataAttribute={dataForm?.quanlity}
              placeholder={_t('Trạng thái chỉ số')}
              setData={setDataForm}
              allowClear={true}
              isRequired={false}
              listDataItem={[{ label: 'Chỉ số lỗi', value: 1 }]}
              onCallBackFunction={(data: any) => {
                setRefreshState(false);
                setDataForm({ quanlity: data.value });
              }}
            />
          </div>
        </div>
        <div className='flex gap-1'>
          {dataForm?.fullName || dataForm?.quanlity || dataForm?.measurementPointId ? (
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              tooltipText='Xem tất cả'
              icon={<IoBackspaceOutline size={25} />}
              onClick={handleClearSearch}
              variant={'text'}
            />
          ) : null}
        </div>
      </div>
    </div>
  );
};

export default SearchGCS;
