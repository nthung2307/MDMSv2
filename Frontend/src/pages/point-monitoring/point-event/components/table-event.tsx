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
      width: '15%',
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
      title: 'Nguồn',
      render: (record) => {
        return (
          <span>{`${record.measurementPointCode ?? 'N/A'} - ${record.customerName ?? 'N/A'}`}</span>
        );
      },
    },
    {
      title: 'Tên cảnh báo',
      dataIndex: 'eventName',
      width: '15%',
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
      width: '5%',
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
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
      onRow={(record) => {
        return {
          onClick: () => {
            navigate(
              ROUTE_PATH.POINT_MONITORING_DETAIL.replace(
                Constants.ReplaceRouterConfig.type,
                record.measurementPoints.tagName.tagName
              )
            );
          },
        };
      }}
      loading={loading}
    />
  );
};

export default TableEvent;
