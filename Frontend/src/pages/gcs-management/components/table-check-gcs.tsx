import { TableProps } from 'antd';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
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
      title: 'Thao tác',
      key: 'action',
      fixed: 'right',
      align: 'center',
      width: 90,
      render: (_, record) => {
        const url = `${ROUTE_PATH.GCS_CHECK_DETAIL}?cycleId=${record.id}&month=${record.month || ''}&year=${record.year || ''}&period=${record.periodNumber || ''}&gcsName=${encodeURIComponent(GCSName || '')}&gcsId=${gcs?.id || ''}`;
        return (
          <ActionDetailLink
            href={url}
            title={`Chi tiết kì chốt: Tháng ${record.month}/${record.year} kì ${record.periodNumber}`}
            onClick={() => {
              navigate(url, { state: { record, GCSName, gcs } });
            }}
          />
        );
      },
    },
  ];

  return (
    <TableCommon columns={columns} dataSource={dataSource} pagination={false} loading={loading} />
  );
};

export default TableCheckGcs;
