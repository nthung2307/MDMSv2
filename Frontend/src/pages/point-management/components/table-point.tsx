import { Dropdown, MenuProps, TableProps, Tooltip } from 'antd';
import { BsThreeDots } from 'react-icons/bs';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
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
      title: '',
      key: 'action',
      fixed: 'right',
      align: 'center',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              const url = ROUTE_PATH.POINT_DETAIL.replace(':id', record.key.toString());
              window.open(url, '_blank');
            },
          },
        ];

        return (
          <div className={'flex justify-center cursor-pointer'}>
            <Dropdown menu={{ items }}>
              <BsThreeDots />
            </Dropdown>
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
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
    />
  );
};

export default TablePointManagement;
