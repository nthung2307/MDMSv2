import { TableProps } from 'antd';

// import Constants from '@/core/application/common/constants.ts';
import EditedIc from '@/assets/icons/edited-ic.svg';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { formatNumberWithCommas } from '@/infrastructure/utils/helpers.ts';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableCheckDetail = (props: Props) => {
  const { dataSource, loading } = props;
  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      align: 'center',
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
      render: (value) => {
        return (
          <div>{value === null || value === undefined ? 'N/A' : formatNumberWithCommas(value)}</div>
        );
      },
    },
    {
      title: 'Chỉ số mới',
      dataIndex: 'currentReading',
      render: (value, record) => {
        return (
          <div className={`flex gap-3 `}>
            <p>{formatNumberWithCommas(value)}</p>
            {record.isChange && <img src={EditedIc} alt='' className='w-6' />}
          </div>
        );
      },
    },
    {
      title: 'Hệ số nhân',
      dataIndex: 'multiplierFactor',
    },
    {
      title: 'Sản lượng',
      dataIndex: 'consumption',
      render: (val, record) => {
        return (
          <div>
            {record.previousReading === null || record.previousReading === undefined
              ? 'N/A'
              : formatNumberWithCommas(val ?? 0)}
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
      rowClassName={(_, index) => {
        const lastRecord =
          (index + 1) % 5 === 0 && dataSource.length !== index + 1 ? 'last-record' : '';
        return `${lastRecord}`;
      }}
      loading={loading}
    />
  );
};

export default TableCheckDetail;
