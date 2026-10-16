import { Dropdown, MenuProps, TableProps } from 'antd';
// import { FaRegEdit } from 'react-icons/fa';
// import { FiEye } from 'react-icons/fi';
import { BsThreeDots } from 'react-icons/bs';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
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
    },
    {
      title: 'Loại điểm đo',
      dataIndex: 'type',
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      width: '20%',
      render: (text) => {
        return text ? <span>{text}</span> : <span>N/A</span>;
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customer',
      width: '20%',
      render: (record) => {
        return record ? <span>{record.fullName}</span> : <span>N/A</span>;
      },
    },
    {
      title: 'Trạng thái',
      width: '15%',
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
      fixed: 'right',
      key: 'action',
      align: 'center',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              const url = `${ROUTE_PATH.DATA_MEASUREMENT_POINT.replace(
                Constants.ReplaceRouterConfig.id,
                record.key
              )}?code=${encodeURIComponent(record.code || '')}&name=${encodeURIComponent(record.customer?.fullName || '')}`;
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
      scroll={{ x: 'max-content', y: window.innerHeight * 0.69 }}
    />
  );
};

export default TableConfigManagement;
