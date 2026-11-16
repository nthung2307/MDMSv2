import { TableProps, Tooltip } from 'antd';
import { FaRegTrashCan } from 'react-icons/fa6';
import { FiRefreshCw } from 'react-icons/fi';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  setIsShowDialog: any;
  onOpenDeleteDialog?: (id: number, code: string) => void;
  loading?: boolean;
}

const TablePointManagement = (props: Props) => {
  const { dataSource, loading, onOpenDeleteDialog } = props;

  const dataTable = dataSource.map((item: any, index: number) => {
    return {
      index: index + 1,
      code: item.code,
      type: item.type,
      address: item.address,
      description: item.description,
      customer: item.customer?.fullName || '',
      status: item.status,
      meterSerial: item.meterSerial,
      meterProfileCode: item.meterProfileCode,
      mappingMode: item.mappingMode || 'HYBRID',
      hasDedicatedKey: Boolean(item.hasDedicatedKey),
      keyPrefix: item.keyPrefix,
      key: item.id,
    };
  });

  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'code',
      fixed: true,
      className: 'antd-action-table',
      width: 140,
    },
    {
      title: 'Số chế tạo',
      dataIndex: 'meterSerial',
      width: 160,
      render: (val) =>
        val ? (
          <span className='font-mono font-semibold text-blue-700'>{val}</span>
        ) : (
          <span className='text-gray-400 italic text-12'>Chưa gán</span>
        ),
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
      width: 100,
      render: (_, record) => {
        const url = ROUTE_PATH.POINT_DETAIL.replace(':id', record.key.toString());
        const replacementUrl = `${ROUTE_PATH.METER_REPLACEMENTS}?pointId=${record.key}`;
        return (
          <div className='flex items-center justify-center gap-2'>
            <ActionDetailLink
              href={url}
              title={`Chi tiết điểm đo: ${record.code}`}
            />
            <ActionDetailLink
              href={replacementUrl}
              title={`Lập biên bản treo tháo cho điểm đo: ${record.code}`}
              className='text-amber-600 hover:text-amber-800 hover:bg-amber-50 p-1.5 rounded transition-colors inline-flex items-center justify-center cursor-pointer'
              icon={<FiRefreshCw size={16} className='flex-shrink-0' />}
            />
          </div>
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
