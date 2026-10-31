import { TableProps } from 'antd';
import { useNavigate } from 'react-router-dom';

// import Constants from '@/core/application/common/constants.ts';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime, renderTextNumber } from '@/infrastructure/utils/helpers';

interface Props {
  dataSource: any;
  setIsShowDialog: any;
  GCSName: string;
  gcsId: any;
  loading: boolean;
}

const TableDetailGcs = (props: Props) => {
  const { dataSource, GCSName, gcsId, loading } = props;
  const navigate = useNavigate();
  const getPreviousDate = (day: number, month: number, year: number) => {
    let date = new Date(year, month - 1, day);
    date.setDate(date.getDate() - 1);

    let prevDay = date.getDate();
    let prevMonth = date.getMonth() + 1;
    let prevYear = date.getFullYear();

    return `${renderTextNumber(prevDay)}/${renderTextNumber(prevMonth)}/${prevYear}`;
  };
  const columns: TableProps['columns'] = [
    {
      title: 'Số phiếu báo',
      dataIndex: 'id',
      align: 'center',
      render: (record) => {
        return <div>{record}</div>;
      },
    },
    {
      title: 'Thời gian',
      dataIndex: 'month',
      align: 'center',
      render: (_, record) => {
        const previousTime =
          record.previousDay !== 0
            ? `${renderTextNumber(record.previousDay)}/${renderTextNumber(record.previousMonth)}/${record.previousYear}`
            : convertDateTime(record.createdTime, 'dd/mm/yyyy');
        return (
          <div>
            {`ngày ${previousTime} - ${getPreviousDate(record.day, record.month, record.year)}`}
          </div>
        );
      },
    },
    {
      title: 'Kì chốt sổ',
      dataIndex: 'periodNumber',
      align: 'center',
      render: (val, record) => {
        return <div>{`K${val} tháng ${record.month}/${record.year}`}</div>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'center',
      fixed: 'right',
      width: 90,
      render: (_, record) => {
        const previousTime =
          record.previousDay !== 0
            ? `${renderTextNumber(record.previousDay)}/${renderTextNumber(record.previousMonth)}/${record.previousYear}`
            : convertDateTime(record.createdTime, 'dd/mm/yyyy');
        const date = `Từ ${previousTime} đến ${getPreviousDate(record.day, record.month, record.year)}`;
        const url = `${ROUTE_PATH.NOTICE_DETAIl}?gcsId=${gcsId}&cycleId=${record.id}`;

        const handlePrepareStorage = () => {
          const stateData = { record, GCSName, gcsId, breadcumbDate: date };
          const storageKey = `notice_detail_${gcsId}_${record.id}`;
          try {
            sessionStorage.setItem(storageKey, JSON.stringify(stateData));
            localStorage.setItem(storageKey, JSON.stringify(stateData));
            sessionStorage.setItem('notice_detail_state', JSON.stringify(stateData));
            localStorage.setItem('notice_detail_state', JSON.stringify(stateData));
          } catch {
            // Ignore storage error
          }
        };

        return (
          <ActionDetailLink
            href={url}
            title={`Chi tiết cảnh báo: Sổ ${GCSName} - Kì ${record.periodNumber}`}
            onClick={() => {
              handlePrepareStorage();
              navigate(url, { state: { record, GCSName, gcsId, breadcumbDate: date } });
            }}
          />
        );
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
    />
  );
};

export default TableDetailGcs;
