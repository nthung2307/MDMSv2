import Column from 'antd/es/table/Column';

import Constants from '@/core/application/common/constants';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  listTagName: any;
  setListTagName: any;
  typeTable: number;
  loading: boolean;
};

const TableDetailAttribute = (props: Props) => {
  const { listTagName, setListTagName, typeTable, loading } = props;
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

    return result.unit ? result.unit : record.unit && record.unit.length > 0 ? record.unit[0].value : '';
  };

  const isDateTimeTab = typeTable === Constants.TYPE_TABLE.dateTimeParams.value;

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
          width={isDateTimeTab ? '50%' : '45%'}
          render={(val) => (
            <div className={`attribute-input font-bold`}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold '>{_t('Tag ID')}</div>}
          dataIndex='value'
          key='value'
          width={isDateTimeTab ? '50%' : '30%'}
          render={(val, record, index) => (
            <div className={`attribute-input`}>
              <InputTextCommon
                key={index}
                label={undefined}
                isLabel={false}
                attribute='tagName'
                dataAttribute={getTagName(val)}
                placeholder={'Nhập tag ID'}
                setData={(data) => {
                  setDataAttribute(data, record);
                }}
                isRequired={false}
              />
            </div>
          )}
        />

        {!isDateTimeTab &&
          (typeTable != Constants.TYPE_TABLE.warningParams.value ? (
            <Column
              title={<div className='text-14-20 font-bold '>{_t('Đơn vị')}</div>}
              dataIndex='unit'
              key='unit'
              width={'25%'}
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
              width={'25%'}
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
