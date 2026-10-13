import { useEffect, useState } from 'react';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import useGCSYearCutOffCycleHook from '@/infrastructure/repository/gcs/hooks/useGCSYearCutOffCycle.hook';
import { _t } from '@/infrastructure/utils/translation';

type Props = {
  handleClearSearch: any;
  setDataForm: any;
  dataForm: any;
  gcsId: any;
};

const SearchDetailGCS = (props: Props) => {
  const { dataForm, setDataForm, handleClearSearch, gcsId } = props;
  const [GcsYearCutOffCycle] = useGCSYearCutOffCycleHook();
  const [listYear, SetListYear] = useState<any>();
  const getGCSYearCutOffCycle = async () => {
    await GcsYearCutOffCycle(
      { gcsId: gcsId },
      (res: any) => {
        const { data } = res;
        SetListYear(data);
      },
      () => {},
      () => {}
    );
  };

  useEffect(() => {
    getGCSYearCutOffCycle();
  }, []);
  return (
    <div className='flex items-center w-1/2 gap-3'>
      <div className='flex items-center gap-3 basis-4/5'>
        <div className='basis-1/2'>
          <InputSelectCommon
            label={null}
            attribute={'year'}
            dataAttribute={dataForm?.year}
            placeholder={_t('Tất cả năm')}
            allowClear={true}
            setData={setDataForm}
            isRequired={false}
            listDataItem={
              listYear
                ? listYear.map((item: any) => {
                    return {
                      label: item,
                      value: item,
                    };
                  })
                : []
            }
          />
        </div>
        <div>
          {dataForm?.month || dataForm?.year ? (
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              tooltipText='Xoá tất cả'
              icon={<IoBackspaceOutline size={25} />}
              onClick={() => {
                handleClearSearch();
              }}
              variant={'text'}
            />
          ) : null}
        </div>
      </div>
    </div>
  );
};

export default SearchDetailGCS;
