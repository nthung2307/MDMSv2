import { Dropdown, MenuProps, TableProps } from 'antd';
import { BsThreeDots } from 'react-icons/bs';
import { useNavigate } from 'react-router-dom';

// import Constants from '@/core/application/common/constants.ts';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime } from '@/infrastructure/utils/helpers';

interface Props {
  dataSource: any;
  GCSName: string;
  gcs: any;
  loading: boolean;
}

const TableCheckGcs = (props: Props) => {
  const { dataSource, GCSName, gcs, loading } = props;
  const navigate = useNavigate();

  const columns: TableProps['columns'] = [
    {
      title: 'Mã kì chốt',
      dataIndex: 'id',
      align: 'center',
      render: (record) => {
        return <div>{record}</div>;
      },
    },
    {
      title: 'Kì chốt',
      dataIndex: 'periodNumber',
      align: 'center',
      render: (record) => {
        return <div>{record}</div>;
      },
    },
    {
      title: 'Tháng năm chốt',
      dataIndex: 'month',
      align: 'center',
      render: (_, record) => {
        return (
          <div>
            {record.month}/{record.year}
          </div>
        );
      },
    },
    {
      title: 'Thời gian lưu sổ',
      dataIndex: 'createdTime',
      align: 'center',
      width: '20%',
      render: (record) => {
        return <div>{convertDateTime(record, 'dd/mm/yyyy hh:mi:ss')}</div>;
      },
    },
    {
      title: <div className={'text-center'}></div>,
      key: 'action',
      fixed: 'right',
      align: 'left',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              navigate(ROUTE_PATH.GCS_CHECK_DETAIL, { state: { record, GCSName, gcs } });
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
    <TableCommon columns={columns} dataSource={dataSource} pagination={false} loading={loading} />
  );
};

export default TableCheckGcs;
