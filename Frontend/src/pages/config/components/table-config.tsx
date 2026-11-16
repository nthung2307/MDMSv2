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
      meterSerial: item.meterSerial,
      meterProfileCode: item.meterProfileCode,
      mappingMode: item.mappingMode,
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
      width: 150,
      className: 'whitespace-nowrap',
      render: (text) => {
        const val = text || 'N/A';
        return (
          <Tooltip title={val}>
            <div className='max-w-[150px] truncate whitespace-nowrap font-medium'>{val}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Số chế tạo / Hồ sơ ánh xạ',
      key: 'meterSerialProfile',
      width: 230,
      className: 'whitespace-nowrap',
      render: (_, record) => {
        const profileDisplay =
          record.meterProfileCode === 'MV_KV_KW' ? 'Cao áp' : 'Hạ áp';
        return (
          <div className='flex items-center gap-1.5'>
            <span className='font-mono text-blue-700 font-semibold'>
              {record.meterSerial || 'Chưa gán'}
            </span>
            <span className='text-xs px-1.5 py-0.5 rounded bg-cyan-50 text-cyan-700 border border-cyan-200'>
              {profileDisplay} ({record.mappingMode || 'HYBRID'})
            </span>
          </div>
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
