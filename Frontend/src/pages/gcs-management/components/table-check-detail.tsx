import { TableProps, Tooltip } from 'antd';

// import Constants from '@/core/application/common/constants.ts';
import EditedIc from '@/assets/icons/edited-ic.svg';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime, formatNumberWithCommas } from '@/infrastructure/utils/helpers.ts';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableCheckDetail = (props: Props) => {
  const { dataSource, loading } = props;
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
        const isPmax = record.meterReadingSet === 'Pmax';
        return (
          <span className='whitespace-nowrap'>
            {value === null || value === undefined
              ? '-'
              : formatNumberWithCommas(value, isPmax ? 2 : undefined)}
          </span>
        );
      },
    },
    {
      title: 'Chỉ số mới',
      dataIndex: 'currentReading',
      width: 140,
      render: (value, record) => {
        const timeTooltip = record.readingTime
          ? `Thời điểm chốt: ${convertDateTime(record.readingTime, 'dd/mm/yyyy hh:mi:ss')}`
          : undefined;
        const isPmax = record.meterReadingSet === 'Pmax';
        return (
          <Tooltip title={timeTooltip}>
            <div className='flex items-center gap-2 whitespace-nowrap'>
              <span>
                {value === null || value === undefined
                  ? '-'
                  : formatNumberWithCommas(value, isPmax ? 2 : undefined)}
              </span>
              {record.isChange && <img src={EditedIc} alt='Đã hiệu chỉnh' className='w-5' />}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Hệ số nhân',
      dataIndex: 'multiplierFactor',
      width: 100,
      render: (val, record) => {
        return <span className='whitespace-nowrap'>{record.meterReadingSet === 'Pmax' ? '' : val}</span>;
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
        return (
          <div className='whitespace-nowrap font-medium'>
            {val === null || val === undefined ? '-' : formatNumberWithCommas(val)}
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
        rowClassName={(_, index) => {
          const lastRecord =
            (index + 1) % 6 === 0 && dataSource.length !== index + 1 ? 'last-record' : '';
          return `${lastRecord}`;
        }}
        loading={loading}
      />
    </div>
  );
};

export default TableCheckDetail;
