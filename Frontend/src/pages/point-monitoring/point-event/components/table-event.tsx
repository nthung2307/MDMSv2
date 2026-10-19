import { TableProps, Tooltip } from 'antd';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime } from '@/infrastructure/utils/helpers';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableEvent = (props: Props) => {
  const { dataSource, loading } = props;
  const navigate = useNavigate();

  const columns: TableProps['columns'] = [
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      width: 170,
      className: 'whitespace-nowrap',
      render: (value) => {
        return value ? (
          <span className='whitespace-nowrap'>
            {convertDateTime(value, 'dd/mm/yyyy hh:mi:ss')}
          </span>
        ) : (
          <span>N/A</span>
        );
      },
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      align: 'left',
      width: 160,
      className: 'whitespace-nowrap',
      render: (val) => {
        const text = val ?? 'N/A';
        return (
          <Tooltip title={text}>
            <div className='max-w-[160px] truncate whitespace-nowrap font-medium'>{text}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customerName',
      align: 'left',
      width: 250,
      className: 'whitespace-nowrap',
      render: (val) => {
        const text = val ?? 'N/A';
        return (
          <Tooltip title={text}>
            <div className='max-w-[250px] truncate whitespace-nowrap'>{text}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên sự kiện',
      dataIndex: 'eventName',
      align: 'left',
      width: 280,
      className: 'whitespace-nowrap',
      render: (val) => {
        const text = val ?? 'N/A';
        return (
          <Tooltip title={text}>
            <div className='max-w-[280px] truncate whitespace-nowrap'>{text}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Trạng thái',
      align: 'center',
      dataIndex: 'status',
      width: 120,
      className: 'whitespace-nowrap',
      render: (record) => {
        if (!record) return <span>N/A</span>;
        return (
          <span className='whitespace-nowrap'>{`${record.charAt(0)}${record.slice(1).toLowerCase()}`}</span>
        );
      },
    },
    {
      title: 'Giá trị',
      dataIndex: 'value',
      align: 'center',
      width: 80,
      className: 'whitespace-nowrap',
      render: (value) => {
        return <span className='whitespace-nowrap'>{value == 'False' ? 0 : 1}</span>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      loading={loading}
      rowClassName={() => 'whitespace-nowrap cursor-pointer'}
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
      onRow={(record) => {
        return {
          onClick: () => {
            const code = record.measurementPointCode;
            if (code) {
              navigate(
                ROUTE_PATH.POINT_MONITORING_DETAIL.replace(
                  Constants.ReplaceRouterConfig.id,
                  code
                )
              );
            }
          },
        };
      }}
    />
  );
};

export default TableEvent;
