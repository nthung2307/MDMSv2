import { IoMdAdd } from 'react-icons/io';
import { RiSubtractFill } from 'react-icons/ri';

import Constants from '@/core/application/common/constants.ts';
import { CheckboxCommon } from '@/infrastructure/common/components/Checkbox/checkbox-common';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import SwitchTextValue from '@/infrastructure/common/components/switch/switch-text-value.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import TableGCSPoint from '@/pages/gcs-management/components/table-list-point';
interface IGcsForm {
  data: any;
  setData: any;
  validate: any;
  setValidate: any;
  submittedTime?: any;
  disabled?: boolean;
  title?: string;
  editMode?: boolean;
  createMode?: boolean;
}

const GcsForm = ({
  data,
  setData,
  validate,
  setValidate,
  submittedTime,
  disabled = false,
  title = 'Thông tin sổ',
  editMode = false,
  createMode = false,
}: IGcsForm) => {
  const isDetail = !editMode && !createMode;
  return (
    <div>
      <div className={'flex justify-between'}>
        <h1 className='font-bold text-20-20'>{title}</h1>
        <div>
          <SwitchTextValue
            data={data.status ? data : { status: 'DEACTIVATE' }}
            expectedData={[
              { key: true, value: Constants.GCSStatus.Active },
              { key: false, value: Constants.GCSStatus.Deactivate },
            ]}
            setData={setData}
            attribute={'status'}
            label={_t('Kích hoạt')}
            isRequired={editMode}
            reverseLabel={true}
            disabled={disabled}
            asterisk={false}
          />
        </div>
      </div>
      <div className={'flex flex-col gap-y-5 mt-5'}>
        <div className='flex w-full gap-2'>
          <InputTextCommon
            label={_t('Tên sổ')}
            attribute={'name'}
            setData={setData}
            dataAttribute={data?.name}
            isRequired={true}
            validate={validate}
            setValidate={setValidate}
            disabled={disabled}
            placeholder={'Nhập tên sổ'}
          />
          <InputTextCommon
            label={_t('Mã sổ')}
            attribute={'code'}
            setData={setData}
            dataAttribute={data?.code}
            isRequired={true}
            validate={validate}
            setValidate={setValidate}
            disabled={disabled}
            placeholder={'Nhập mã sổ'}
          />
        </div>
        <div>
          <InputTextCommon
            className='gap-10'
            label={_t('Mô tả')}
            attribute={'description'}
            setData={setData}
            dataAttribute={data?.description}
            isRequired={false}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={disabled}
            placeholder={'Nhập mô tả'}
          />
        </div>
        {data &&
          data.dayPeriod.map((val: any, index: number) => {
            return (
              <div>
                <p className='text-14-20 font-bold mb-3'>
                  Kì {index + 1} <span className='text-error text-14-20 pl-1'>*</span>
                </p>
                <div className='flex gap-4'>
                  <div className='flex items-center gap-3'>
                    <InputNumberCommon
                      label={_t(`Ngày GCS kì ${index + 1}`)}
                      attribute={`dayPeriod1`}
                      dataAttribute={val === 999 ? '' : val}
                      isCol={false}
                      setData={setData}
                      isRequired={false}
                      validate={validate}
                      setValidate={setValidate}
                      submittedTime={submittedTime}
                      onCallbackFunction={(dataInput) => {
                        let dayPeriod = data.dayPeriod;
                        if (dataInput !== '') {
                          dayPeriod[index] = Number(dataInput);
                          if (Number(dataInput) >= 28) {
                            dayPeriod.splice(index + 1);
                          }
                        } else {
                          if (dayPeriod[index] != 999) {
                            dayPeriod[index] = 0;
                          }
                        }
                        setData({ dayPeriod });
                      }}
                      disabled={data.dayPeriod[index] === 999 || isDetail}
                      placeholder={'Nhập ngày'}
                    />
                  </div>
                  <CheckboxCommon
                    dataAttribute={data.dayPeriod[index] === 999}
                    setData={(dataCheckbox) => {
                      let dayPeriod = data.dayPeriod;
                      dayPeriod[index] = dataCheckbox ? 999 : 0;
                      dayPeriod.splice(index + 1);
                      setData({ dayPeriod });
                    }}
                    disabled={isDetail}
                    label={'Cuối tháng'}
                  />
                  {!isDetail && index == data.dayPeriod.length - 1 && (
                    <div className='flex gap-2 items-center'>
                      {data.dayPeriod[index] !== 999 && data.dayPeriod[index] < 28 && (
                        <div
                          className='cursor-pointer'
                          onClick={() => {
                            let dayPeriod = data.dayPeriod;
                            dayPeriod.splice(index + 1, 0, 0);
                            setData({ dayPeriod });
                          }}
                        >
                          <IoMdAdd size={20} />
                        </div>
                      )}
                      {index != 0 && (
                        <div
                          className='cursor-pointer'
                          onClick={() => {
                            let dayPeriod = data.dayPeriod;
                            dayPeriod.splice(index, 1);
                            setData({ dayPeriod });
                          }}
                        >
                          <RiSubtractFill size={20} />
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            );
          })}

        {!createMode && (
          <div>
            <p className='font-bold font-14-20 text-[#3B3B3B] mb-3'>Danh sách điểm đo</p>
            <TableGCSPoint dataSource={data.measurementPoints ?? []} />
          </div>
        )}
      </div>
    </div>
  );
};

export default GcsForm;
