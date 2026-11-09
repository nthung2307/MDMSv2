import { TableProps, Tooltip } from 'antd';

// eslint-disable-next-line import/order
import EditedIc from '@/assets/icons/edited-ic.svg';

// import Constants from '@/core/application/common/constants.ts';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime, formatNumberWithCommas } from '@/infrastructure/utils/helpers.ts';

interface Props {
  dataSource: any;
  setIsShowDialogEdit: any;
  loading?: boolean;
}

const TableGcs = (props: Props) => {
  const { dataSource, setIsShowDialogEdit, loading } = props;

  const defineTooltip = (record: any) => {
    if (!record.previousDay) return 'Kì trước chưa chốt';

    return `Kì ${record.previousPeriodNumber}, ${record.previousDay >= 10 ? record.previousDay : '0' + record.previousDay}/${record.previousMonth}/${record.previousYear}`;
  };

  const newComsumtion = (
    newParam: any,
    previusParam: any,
    multiplier: any,
    isNewReading: boolean
  ) => {
    const result = (newParam - previusParam) * multiplier;
    return !isNewReading || previusParam === null || previusParam === undefined
      ? 'N/A'
      : formatNumberWithCommas(result);
  };

  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPoint',
      width: 200,
      render: (record) => {
        const code = record?.code || '';
        return (
          <Tooltip title={code}>
            <div className='max-w-[190px] truncate whitespace-nowrap font-medium text-gray-800'>
              {code}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'measurementPoint',
      width: 400,
      render: (record) => {
        const name = record?.customer?.fullName || '';
        return (
          <Tooltip title={name}>
            <div className='max-w-[390px] truncate whitespace-nowrap font-medium text-gray-700'>
              {name}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Bộ chỉ số',
      dataIndex: 'meterReadingSet',
      width: 90,
      render: (value) => {
        return <span className='font-semibold whitespace-nowrap'>{value}</span>;
      },
    },
    {
      title: 'Chỉ số cũ',
      dataIndex: 'previousReading',
      width: 130,
      render: (value, record) => {
        const displayValue = record.newPreviousReading ?? value;
        const isPmax = record.meterReadingSet === 'Pmax';
        return (
          <Tooltip placement='bottomLeft' title={defineTooltip(record)}>
            <button
              type='button'
              onClick={() => {
                setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'previous' });
              }}
              className='flex items-center gap-2 cursor-pointer whitespace-nowrap'
            >
              <span>
                {displayValue === null || displayValue === undefined
                  ? '-'
                  : formatNumberWithCommas(displayValue, isPmax ? 2 : undefined)}
              </span>
              {record.isChange && record.newPreviousReading !== null && record.newPreviousReading !== undefined && (
                <img src={EditedIc} alt='' className='w-5' />
              )}
            </button>
          </Tooltip>
        );
      },
    },
    {
      title: 'Chỉ số mới',
      dataIndex: 'currentReading',
      width: 140,
      render: (value, record) => {
        const isSuspicious =
          record.isSuspicious &&
          record.newCurrentReading == null &&
          record.newPreviousReading == null;

        const hasNoReading =
          (value === null || value === undefined) &&
          (record.newCurrentReading === null || record.newCurrentReading === undefined);

        const effectiveTime =
          record.meterReadingSet === 'Pmax' && record.pmaxTime
            ? record.pmaxTime
            : record.readingTime;

        const tooltipTitle = hasNoReading
          ? 'Chưa có chỉ số chốt từ công tơ (chưa từng được cập nhật)'
          : isSuspicious && record.suspiciousReason
          ? `Lý do nghi ngờ: ${record.suspiciousReason} (Thời điểm: ${convertDateTime(effectiveTime, 'dd/mm/yyyy hh:mi:ss')})`
          : record.meterReadingSet === 'Pmax' && record.pmaxTime
          ? `Thời điểm Pmax: ${convertDateTime(record.pmaxTime, 'dd/mm/yyyy hh:mi:ss')} | Thời điểm chốt: ${convertDateTime(record.readingTime, 'dd/mm/yyyy hh:mi:ss')}`
          : `Thời điểm chốt: ${convertDateTime(record.readingTime, 'dd/mm/yyyy hh:mi:ss')}`;

        if (hasNoReading) {
          return (
            <Tooltip placement='bottomLeft' title={tooltipTitle}>
              <button
                type='button'
                onClick={() => {
                  setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'current' });
                }}
                className='text-gray-400 font-medium inline-flex items-center gap-1 cursor-pointer whitespace-nowrap hover:text-primary'
              >
                <span>-</span>
              </button>
            </Tooltip>
          );
        }

        const isPmax = record.meterReadingSet === 'Pmax';
        const displayValue =
          record.newCurrentReading != null
            ? formatNumberWithCommas(record.newCurrentReading, isPmax ? 2 : undefined)
            : formatNumberWithCommas(value, isPmax ? 2 : undefined);

        if (isSuspicious) {
          return (
            <Tooltip placement='bottomLeft' title={tooltipTitle}>
              <button
                type='button'
                onClick={() => {
                  setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'current' });
                }}
                className='text-amber-600 font-medium inline-flex items-center gap-1 cursor-pointer whitespace-nowrap'
              >
                <span>{displayValue}</span>
                <span className='text-amber-500 font-bold text-base leading-none'>*</span>
              </button>
            </Tooltip>
          );
        }

        return (
          <Tooltip placement='bottomLeft' title={tooltipTitle}>
            <button
              type='button'
              onClick={() => {
                setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'current' });
              }}
              className='flex items-center gap-2 cursor-pointer whitespace-nowrap font-medium text-gray-800'
            >
              <span>{displayValue}</span>
              {record.isChange && record.newCurrentReading != null && (
                <img src={EditedIc} alt='Đã hiệu chỉnh' className='w-5' />
              )}
            </button>
          </Tooltip>
        );
      },
    },
    {
      title: 'Hệ số nhân',
      dataIndex: 'multiplierFactor',
      width: 100,
      render: (value, record) => {
        return <span className='whitespace-nowrap'>{record.meterReadingSet === 'Pmax' ? null : value}</span>;
      },
    },
    {
      title: 'Sản lượng',
      dataIndex: 'consumption',
      width: 150,
      render: (val, record) => {
        if (record.meterReadingSet === 'Pmax') {
          return <div className='whitespace-nowrap'>N/A</div>;
        }

        const hasNoReading =
          (record.currentReading === null || record.currentReading === undefined) &&
          (record.newCurrentReading === null || record.newCurrentReading === undefined);

        if (hasNoReading) {
          return <div className='text-gray-400 whitespace-nowrap'>-</div>;
        }

        const isSuspicious =
          record.isSuspicious &&
          record.newCurrentReading == null &&
          record.newPreviousReading == null;

        const consumptionVal =
          val !== null &&
          val !== undefined &&
          record.previousReading !== null &&
          record.previousReading !== undefined
            ? val
            : record.newCurrentReading != null
            ? (record.newCurrentReading - (record.newPreviousReading ?? record.previousReading ?? 0)) *
              record.multiplierFactor
            : null;

        if (isSuspicious) {
          return (
            <Tooltip title={record.suspiciousReason || 'Số liệu bị nghi ngờ'}>
              <div className='text-amber-600 font-medium inline-flex items-center gap-1 cursor-help whitespace-nowrap'>
                <span>
                  {consumptionVal !== null
                    ? formatNumberWithCommas(consumptionVal)
                    : newComsumtion(
                        record.newCurrentReading != null
                          ? record.newCurrentReading
                          : record.currentReading,
                        record.newPreviousReading ?? record.previousReading,
                        record.multiplierFactor,
                        record.newCurrentReading != null
                      )}
                </span>
                <span className='text-amber-500 font-bold text-base leading-none'>*</span>
              </div>
            </Tooltip>
          );
        }

        return (
          <div className='whitespace-nowrap font-medium text-gray-800'>
            {consumptionVal !== null
              ? formatNumberWithCommas(consumptionVal)
              : newComsumtion(
                  record.newCurrentReading != null
                    ? record.newCurrentReading
                    : record.currentReading,
                  record.newPreviousReading ?? record.previousReading,
                  record.multiplierFactor,
                  record.newCurrentReading != null
                )}
          </div>
        );
      },
    },
  ];

  return (
    <div className='overflow-x-auto'>
      <TableCommon
        columns={columns}
        dataSource={dataSource}
        pagination={false}
        scroll={{ x: 1200 }}
        rowClassName={(_record, index) => {
          const lastRecord =
            (index + 1) % 6 === 0 && dataSource.length !== index + 1 ? 'last-record' : '';
          return lastRecord;
        }}
        loading={loading}
      />
    </div>
  );
};

export default TableGcs;
