import { TableProps } from 'antd';
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
      width: '16%',
      render: (value) => {
        return value ? (
          <p>
            {convertDateTime(value, 'dd/mm/yyyy hh:mi:ss')}{' '}
            {/* {!record.quanlity && <span className='text-[#FF0000] text-18-20 font-bold'>!</span>} */}
          </p>
        ) : (
          <span>N/A</span>
        );
      },
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      align: 'left',
      width: '18%',
      render: (val) => {
        return <span>{val ?? 'N/A'}</span>;
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customerName',
      align: 'left',
      width: '30%',
      render: (val) => {
        return <span>{val ?? 'N/A'}</span>;
      },
    },
    {
      title: 'Tên sự kiện',
      dataIndex: 'eventName',
      align: 'left',
      width: '20%',
      render: (val) => {
        return <span>{val}</span>;
      },
    },
    {
      title: 'Trạng thái',
      align: 'center',
      dataIndex: 'status',
      width: '10%',
      render: (record) => {
        return <span>{`${record.charAt(0)}${record.slice(1).toLowerCase()}`}</span>;
      },
    },
    {
      title: 'Giá trị',
      dataIndex: 'value',
      align: 'center',
      width: '6%',
      render: (value) => {
        return <span>{value == 'False' ? 0 : 1}</span>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      loading={loading}
      scroll={{ x: 1000, y: window.innerHeight * 0.66 }}
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
      loading={loading}
    />
  );
};

export default TableEvent;
