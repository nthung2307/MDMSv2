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
  const formatCycleDate = (record: any) => {
    if (!record) return '';
    const month = Number(record.month) || 1;
    const year = Number(record.year) || new Date().getFullYear();
    const lastDayOfMonth = new Date(year, month, 0).getDate();

    const gcsNameLower = (GCSName || '').toLowerCase();
    const isOnePeriod =
      record.totalPeriod === 1 ||
      gcsNameLower.includes('1 kỳ') ||
      gcsNameLower.includes('1 ky') ||
      (record.periodNumber === 1 && (!record.totalPeriod || record.totalPeriod <= 1));

    if (isOnePeriod) {
      // Sổ 1 kỳ: chu kỳ luôn trọn vẹn từ ngày 01 đến ngày cuối tháng
      return `Từ 01/${renderTextNumber(month)}/${year} đến ${renderTextNumber(lastDayOfMonth)}/${renderTextNumber(month)}/${year}`;
    }

    // Sổ nhiều kỳ: Kỳ 1 bắt đầu từ ngày 01 đầu tháng
    if (record.periodNumber === 1) {
      let toDay = record.day && record.day > 0 && record.day < lastDayOfMonth ? record.day : lastDayOfMonth;
      return `Từ 01/${renderTextNumber(month)}/${year} đến ${renderTextNumber(toDay)}/${renderTextNumber(month)}/${year}`;
    }

    // Các kỳ tiếp theo (kỳ 2, 3...)
    let fromDay = record.previousDay && record.previousDay > 0 ? record.previousDay + 1 : 1;
    let fromMonth = record.previousMonth || month;
    let fromYear = record.previousYear || year;
    const lastDayOfPrev = new Date(fromYear, fromMonth, 0).getDate();
    if (fromDay > lastDayOfPrev) {
      fromDay = lastDayOfPrev;
    }

    let toDay = record.day && record.day > 0 ? record.day : lastDayOfMonth;
    if (toDay >= 28 || toDay === 999) {
      toDay = lastDayOfMonth;
    }

    return `Từ ${renderTextNumber(fromDay)}/${renderTextNumber(fromMonth)}/${fromYear} đến ${renderTextNumber(toDay)}/${renderTextNumber(month)}/${year}`;
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
        return (
          <div className='whitespace-nowrap'>
            {formatCycleDate(record)}
          </div>
        );
      },
    },
    {
      title: 'Kì chốt sổ',
      dataIndex: 'periodNumber',
      align: 'center',
      render: (val, record) => {
        return <div className='whitespace-nowrap'>{`K${val} tháng ${record.month}/${record.year}`}</div>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'center',
      fixed: 'right',
      width: 90,
      render: (_, record) => {
        const date = formatCycleDate(record);
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
            title={`Chi tiết phiếu báo: Sổ ${GCSName} - Kì ${record.periodNumber}`}
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
