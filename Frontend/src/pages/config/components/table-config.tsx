import { TableProps, Tooltip } from 'antd';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableConfigManagement = (props: Props) => {
  const { dataSource, loading } = props;
  const navigate = useNavigate();

  const dataTable = dataSource.map((item: any, index: number) => {
    return {
      index: index + 1,
      code: item.code,
      type: item.type,
      address: item.address,
      description: item.description,
      customer: item.customer,
      status: item.status,
      key: item.id,
    };
  });
  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'code',
      width: 170,
      className: 'whitespace-nowrap',
      render: (text) => {
        const val = text || 'N/A';
        return (
          <Tooltip title={val}>
            <div className='max-w-[170px] truncate whitespace-nowrap font-medium'>{val}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Loại điểm đo',
      dataIndex: 'type',
      width: 150,
      className: 'whitespace-nowrap',
      render: (text) => <span className='whitespace-nowrap'>{text || 'N/A'}</span>,
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      width: 250,
      className: 'whitespace-nowrap',
      render: (text) => {
        const val = text || 'N/A';
        return (
          <Tooltip title={val}>
            <div className='max-w-[250px] truncate whitespace-nowrap'>{val}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customer',
      width: 250,
      className: 'whitespace-nowrap',
      render: (record) => {
        const val = record?.fullName || 'N/A';
        return (
          <Tooltip title={val}>
            <div className='max-w-[250px] truncate whitespace-nowrap'>{val}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Trạng thái',
      width: 150,
      align: 'center',
      className: 'whitespace-nowrap',
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
      fixed: 'right',
      key: 'action',
      align: 'center',
      width: 90,
      render: (_, record) => {
        const url = `${ROUTE_PATH.DATA_MEASUREMENT_POINT.replace(
          Constants.ReplaceRouterConfig.id,
          record.key
        )}?code=${encodeURIComponent(record.code || '')}&name=${encodeURIComponent(record.customer?.fullName || '')}`;

        return (
          <ActionDetailLink
            href={url}
            title={`Cấu hình điểm đo: ${record.code}`}
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
      scroll={{ x: 'max-content', y: window.innerHeight * 0.69 }}
    />
  );
};

export default TableConfigManagement;
