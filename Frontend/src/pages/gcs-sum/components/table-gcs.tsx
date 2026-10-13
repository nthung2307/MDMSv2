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
  // const navigate = useNavigate();

  const checkStatusPoint = (quanlity: any, currentReading: any, newCurrentReading: any) => {
    if (newCurrentReading != null) return `${formatNumberWithCommas(newCurrentReading)}`;
    if (!quanlity) {
      return currentReading || currentReading == 0
        ? `${formatNumberWithCommas(currentReading)}!`
        : '????';
    }
    return formatNumberWithCommas(currentReading);
  };

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
      render: (record) => {
        return <div>{record.code}</div>;
      },
    },
    {
      title: 'Mô tả',
      dataIndex: 'measurementPoint',
      render: (record) => {
        return <div>{record.description}</div>;
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'measurementPoint',
      render: (record) => {
        return <div>{record.customer.fullName}</div>;
      },
    },
    {
      title: 'Bộ chỉ số',
      dataIndex: 'meterReadingSet',
    },
    {
      title: 'Chỉ số cũ',
      dataIndex: 'previousReading',
      render: (value, record) => {
        const displayValue = record.newPreviousReading ?? value;
        return (
          <Tooltip placement='bottomLeft' title={defineTooltip(record)}>
            <button
              type='button'
              onClick={() => {
                setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'previous' });
              }}
              className='flex gap-3 cursor-pointer'
            >
              <p>
                {displayValue === null || displayValue === undefined
                  ? 'N/A'
                  : formatNumberWithCommas(displayValue)}
              </p>
              {record.newPreviousReading !== null && record.newPreviousReading !== undefined && (
                <img src={EditedIc} alt='' className='w-6' />
              )}
            </button>
          </Tooltip>
        );
      },
    },
    {
      title: 'Chỉ số mới',
      dataIndex: 'currentReading',
      render: (value, record) => {
        return (
          <>
            {record.currentReading == null ? (
              <button
                type='button'
                onClick={() => {
                  setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'current' });
                }}
                className='flex gap-3 cursor-pointer'
              >
                <p>{checkStatusPoint(record.quanlity, value, record.newCurrentReading)}</p>
                {record.newCurrentReading != null && <img src={EditedIc} alt='' className='w-6' />}
              </button>
            ) : (
              <Tooltip
                placement='bottomLeft'
                title={convertDateTime(record.readingTime, 'dd/mm/yyyy')}
              >
                <button
                  type='button'
                  onClick={() => {
                    setIsShowDialogEdit({ isOpen: true, record: record, fieldType: 'current' });
                  }}
                  className='flex gap-3 cursor-pointer'
                >
                  <p>{checkStatusPoint(record.quanlity, value, record.newCurrentReading)}</p>
                  {record.newCurrentReading != null && (
                    <img src={EditedIc} alt='' className='w-6' />
                  )}
                </button>
              </Tooltip>
            )}
          </>
        );
      },
    },
    {
      title: 'Hệ số nhân',
      dataIndex: 'multiplierFactor',
      render: (value, record) => {
        return record.meterReadingSet === 'Pmax' ? null : value;
      },
    },
    {
      title: 'Sản lượng',
      dataIndex: 'consumption',
      render: (val, record) => {
        if (record.meterReadingSet === 'Pmax') {
          return <div>N/A</div>;
        }
        return (
          <div>
            {val !== null &&
            val !== undefined &&
            record.previousReading !== null &&
            record.previousReading !== undefined
              ? formatNumberWithCommas(val)
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
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      rowClassName={(record, index) => {
        const errorRow =
          record.newCurrentReading == null && (record.currentReading == null || !record.quanlity)
            ? 'error-row'
            : '';
        const lastRecord =
          (index + 1) % 5 === 0 && dataSource.length !== index + 1 ? 'last-record' : '';
        return `${errorRow} ${lastRecord}`;
      }}
      loading={loading}
    />
  );
};

export default TableGcs;
