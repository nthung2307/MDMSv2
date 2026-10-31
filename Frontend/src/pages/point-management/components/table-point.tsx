import { TableProps, Tooltip } from 'antd';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  setIsShowDialog: any;
  loading?: boolean;
}

const TablePointManagement = (props: Props) => {
  const { dataSource, loading } = props;

  const dataTable = dataSource.map((item: any, index: number) => {
    return {
      index: index + 1,
      code: item.code,
      type: item.type,
      address: item.address,
      description: item.description,
      customer: item.customer.fullName,
      status: item.status,
      key: item.id,
    };
  });

  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'code',
      fixed: true,
      className: 'antd-action-table',
      width: 150,
    },
    {
      title: 'Loại điểm đo',
      dataIndex: 'type',
      width: 150,
    },
    // {
    //   title: 'Địa chỉ',
    //   dataIndex: 'address',
    // },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      width: 200,
      render: (record) => {
        return (
          <Tooltip title={`${record}`}>
            <div className='w-52 truncate'>{record ? <span>{record}</span> : <span></span>}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customer',
      width: 200,
      render: (record) => {
        return (
          <Tooltip title={`${record}`}>
            <div className='w-52 truncate'>{record ? <span>{record}</span> : <span></span>}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Trạng thái',
      width: 150,
      render: (_, record) => {
        const isActive = record.status === Constants.CustomerStatus.Active;
        const backgroundColor = isActive ? '#41A026' : '#B22222';
        const statusText = isActive ? 'Đang sử dụng' : 'Ngừng sử dụng';
        return (
          <StatusTagCommon status={statusText} bgColor={backgroundColor} textColor={'#FFFFFF'} />
        );
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      fixed: 'right',
      align: 'center',
      width: 90,
      render: (_, record) => {
        const url = ROUTE_PATH.POINT_DETAIL.replace(':id', record.key.toString());
        return (
          <ActionDetailLink
            href={url}
            title={`Chi tiết điểm đo: ${record.code}`}
          />
        );
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataTable}
      pagination={false}
      loading={loading}
      rowClassName={() => 'whitespace-nowrap cursor-pointer'}
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
    />
  );
};

export default TablePointManagement;
