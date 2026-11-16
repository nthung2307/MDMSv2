import { Tag } from 'antd';
import Column from 'antd/es/table/Column';

import Constants from '@/core/application/common/constants';
import { getCanonicalObisInfo } from '@/core/application/common/obisCatalog';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import { MeterProfileTagMappingItem } from '@/infrastructure/repository/config/hooks/useIoTConfigHook';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  listTagName: any;
  setListTagName: any;
  typeTable: number;
  loading: boolean;
  mappingMode?: string;
  profileMappings?: MeterProfileTagMappingItem[];
};

const formatObisCode = (code?: string | null): string => {
  if (!code) return '';
  return code.replace(/_/g, '.');
};

const TableDetailAttribute = (props: Props) => {
  const {
    listTagName,
    setListTagName,
    typeTable,
    loading,
    mappingMode = 'HYBRID',
    profileMappings = [],
  } = props;

  const dataConfig = () => {
    switch (typeTable) {
      case Constants.TYPE_TABLE.powerParams.value:
        return Constants.dataMeasurementPoint.dataPowerParams;
      case Constants.TYPE_TABLE.warningParams.value:
        return Constants.dataMeasurementPoint.dataWarningParams;
      case Constants.TYPE_TABLE.dateTimeParams.value:
        return Constants.dataMeasurementPoint.dateTimeParams;
      default:
        return Constants.dataMeasurementPoint.dataOperatingParams;
    }
  };

  const setDataAttribute = (dt: any, record: any) => {
    const newListTagname = listTagName.map((item: any) => {
      if (item.description === record.value) {
        return {
          ...item,
          tagName: 'tagName' in dt ? dt.tagName : item.tagName,
          unit: dt.unit || item.unit,
          customName: 'customName' in dt ? dt.customName : item.customName,
        };
      }
      return item;
    });
    setListTagName(newListTagname);
  };

  const getTagName = (val: any) => {
    const result = listTagName?.find((item: any) => item.description === val);
    return result?.tagName || '';
  };

  const getCustomName = (record: any) => {
    const result = listTagName?.find((item: any) => item.description === record.value);
    return result?.customName || '';
  };

  const getUnit = (record: any) => {
    const result = listTagName?.find((item: any) => item.description === record.value);

    if (!result) return record?.unit && record.unit.length > 0 ? record.unit[0].value : null;

    return result.unit
      ? result.unit
      : record.unit && record.unit.length > 0
        ? record.unit[0].value
        : '';
  };

  const getProfileMapping = (paramCode: string) =>
    profileMappings.find((m) => m.parameterCode === paramCode);

  const isDateTimeTab = typeTable === Constants.TYPE_TABLE.dateTimeParams.value;
  const allowAuto = mappingMode !== 'MANUAL';

  return (
    <>
      <TableCommon
        dataSource={[...dataConfig()]}
        rowKey={(record: any) => record.value || record.idTable}
        pagination={false}
        className='table-common'
        loading={loading}
      >
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Dữ liệu')}</div>}
          dataIndex='label'
          key='label'
          width={isDateTimeTab ? '36%' : '32%'}
          render={(val) => (
            <div className={`attribute-input font-bold`}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Mã OBIS')}</div>}
          dataIndex='value'
          key='obisAuto'
          width={isDateTimeTab ? '18%' : '14%'}
          render={(val) => {
            const obisInfo = getCanonicalObisInfo(val, profileMappings);
            if (!obisInfo) {
              return <span className='text-gray-400 text-12'>---</span>;
            }
            if (isDateTimeTab && obisInfo.typeTag) {
              const isDate = obisInfo.typeTag === 'DATE';
              return (
                <div className='flex items-center gap-1.5'>
                  <span
                    className={`font-mono font-bold text-13 ${
                      isDate ? 'text-blue-700' : 'text-purple-700'
                    }`}
                  >
                    {obisInfo.obisCode}
                  </span>
                  <span
                    className={`text-[10px] font-semibold px-1.5 py-0.5 rounded border leading-none ${
                      isDate
                        ? 'bg-blue-50 text-blue-700 border-blue-200'
                        : 'bg-purple-50 text-purple-700 border-purple-200'
                    }`}
                  >
                    {isDate ? 'Date' : 'Time'}
                  </span>
                </div>
              );
            }
            return (
              <span className='font-mono font-bold text-blue-700 text-13'>
                {obisInfo.obisCode}
              </span>
            );
          }}
        />
        <Column
          title={<div className='text-14-20 font-bold '>{_t('Tag ID')}</div>}
          dataIndex='value'
          key='value'
          width={isDateTimeTab ? '46%' : '28%'}
          render={(val, record, index) => {
            const obisInfo = getCanonicalObisInfo(val, profileMappings);
            const tagHint = obisInfo
              ? obisInfo.typeTag
                ? `${obisInfo.obisCode} - ${obisInfo.typeTag === 'DATE' ? 'Date' : 'Time'}`
                : obisInfo.obisCode
              : '';
            return (
              <div className={`attribute-input`}>
                <InputTextCommon
                  key={index}
                  label={undefined}
                  isLabel={false}
                  attribute='tagName'
                  dataAttribute={getTagName(val)}
                  placeholder={
                    tagHint && allowAuto
                      ? `Để trống dùng Auto OBIS (${tagHint})`
                      : 'Nhập tag ID'
                  }
                  setData={(data) => {
                    setDataAttribute(data, record);
                  }}
                  isRequired={false}
                />
              </div>
            );
          }}
        />

        {!isDateTimeTab &&
          (typeTable != Constants.TYPE_TABLE.warningParams.value ? (
            <Column
              title={<div className='text-14-20 font-bold '>{_t('Đơn vị')}</div>}
              dataIndex='unit'
              key='unit'
              width={'22%'}
              render={(val, record) => (
                <div className={`attribute-input`}>
                  <InputSelectCommon
                    label={undefined}
                    attribute={'unit'}
                    setData={(data) => {
                      setDataAttribute(data, record);
                    }}
                    dataAttribute={getUnit(record)}
                    isRequired={false}
                    placeholder={val.length > 0 ? 'Chọn đơn vị' : ''}
                    listDataItem={val}
                    disabled={val.length <= 1}
                  />
                </div>
              )}
            />
          ) : (
            <Column
              title={<div className='text-14-20 font-bold '>{_t('Tên cảnh báo')}</div>}
              dataIndex='customName'
              key='customName'
              width={'22%'}
              render={(_, record, index) => (
                <div className={`attribute-input`}>
                  {index !== dataConfig().length - 1 && (
                    <InputTextCommon
                      key={index}
                      label={undefined}
                      isLabel={false}
                      attribute='customName'
                      dataAttribute={getCustomName(record)}
                      placeholder={'Nhập tên cảnh báo'}
                      setData={(data) => {
                        setDataAttribute(data, record);
                      }}
                      isRequired={false}
                    />
                  )}
                </div>
              )}
            />
          ))}
      </TableCommon>
    </>
  );
};

export default TableDetailAttribute;
