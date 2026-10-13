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

const TableWarning = (props: Props) => {
  const { dataSource, loading } = props;
  const navigate = useNavigate();

  const columns: TableProps['columns'] = [
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      width: '15%',
      render: (record) => {
        return record ? (
          <span>{convertDateTime(record, 'dd/mm/yyyy hh:mi:ss')}</span>
        ) : (
          <span>N/A</span>
        );
      },
    },
    {
      title: 'Nguồn',
      align: 'left',
      render: (record) => {
        return (
          <span>{`${record.measurementPointCode ?? 'N/A'} - ${record.customerName ?? 'N/A'}`}</span>
        );
      },
    },
    {
      title: 'Tên cảnh báo',
      dataIndex: 'eventName',
      align: 'left',
      width: '15%',
      render: (record) => {
        return record ? <span>{record}</span> : <span>N/A</span>;
      },
    },
    {
      title: 'Trạng thái',
      align: 'left',
      dataIndex: 'status',
      width: '10%',
      render: (record) => {
        return <span>{`${record.charAt(0)}${record.slice(1).toLowerCase()}`}</span>;
      },
    },
    {
      title: 'Giá trị',
      dataIndex: 'value',
      align: 'left',
      width: '10%',
      render: (record) => {
        return <span>{record ? 1 : 0}</span>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      loading={loading}
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
    />
  );
};

export default TableWarning;
