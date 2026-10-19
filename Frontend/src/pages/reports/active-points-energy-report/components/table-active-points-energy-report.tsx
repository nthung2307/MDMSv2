import { Table, TableProps, Tooltip } from 'antd';
import dayjs from 'dayjs';
import React from 'react';

import { IActivePointsEnergyReportItem } from '@/core/application/interfaces/response/reports/report.interface.ts';

interface Props {
  dataSource: IActivePointsEnergyReportItem[];
  loading?: boolean;
}

const parseEnergyValue = (val: any) => {
  if (val === null || val === undefined || val === '') {
    return { isNull: true, isNotTopical: false, text: '-' };
  }

  const str = String(val).trim();
  if (str === '' || str === '-') {
    return { isNull: true, isNotTopical: false, text: '-' };
  }

  const isNotTopical = str.endsWith('*') || str.includes('Not Topical');
  const cleanStr = str.replace('*', '').replace('(Not Topical)', '').trim();

  let formatted = cleanStr;
  const numDirect = Number(cleanStr);
  if (!isNaN(numDirect) && cleanStr !== '') {
    formatted = numDirect.toLocaleString('vi-VN', {
      minimumFractionDigits: numDirect % 1 === 0 ? 0 : 2,
      maximumFractionDigits: 4,
    });
  }

  return {
    isNull: false,
    isNotTopical,
    text: formatted,
  };
};

const renderEnergyCell = (val: any) => {
  const { isNull, isNotTopical, text } = parseEnergyValue(val);

  if (isNull) {
    return (
      <div style={{ textAlign: 'center' }} className='text-gray-400 select-none whitespace-nowrap'>
        -
      </div>
    );
  }

  if (isNotTopical) {
    return (
      <Tooltip title='Số liệu bị nghi ngờ'>
        <div
          style={{ textAlign: 'right', paddingRight: '8px' }}
          className='text-amber-600 font-medium inline-flex items-center justify-end w-full gap-1 cursor-help whitespace-nowrap'
        >
          <span className='whitespace-nowrap'>{text}</span>
          <span className='text-amber-500 font-bold text-base leading-none'>*</span>
        </div>
      </Tooltip>
    );
  }

  return (
    <div style={{ textAlign: 'right', paddingRight: '8px' }} className='text-gray-800 whitespace-nowrap'>
      {text}
    </div>
  );
};

const TableActivePointsEnergyReport: React.FC<Props> = ({ dataSource, loading }) => {
  const columns: TableProps<IActivePointsEnergyReportItem>['columns'] = [
    {
      title: 'STT',
      key: 'stt',
      fixed: 'left',
      width: 60,
      align: 'center',
      className: 'whitespace-nowrap',
      render: (_val, _record, index) => <span className='whitespace-nowrap'>{index + 1}</span>,
    },
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      fixed: 'left',
      width: 150,
      align: 'center',
      className: 'whitespace-nowrap',
      render: (val: any) => {
        if (!val) return <span className='whitespace-nowrap'>-</span>;
        const d = dayjs(val);
        const text = d.isValid() ? d.format('DD/MM/YYYY HH:mm') : String(val);
        return <span className='whitespace-nowrap font-medium text-gray-700'>{text}</span>;
      },
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      fixed: 'left',
      width: 220,
      align: 'center',
      className: 'whitespace-nowrap',
      render: (val: any) => {
        const text = val || '-';
        return (
          <Tooltip title={text}>
            <div className='max-w-[210px] mx-auto truncate whitespace-nowrap font-medium text-gray-800'>
              {text}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Kênh giao (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
      ],
    },
    {
      title: 'Kênh nhận (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPTongKenhNhan',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhNhan',
          width: 130,
          align: 'center',
          className: 'whitespace-nowrap',
          render: renderEnergyCell,
        },
      ],
    },
  ];

  return (
    <div className='w-full overflow-x-auto rounded-lg border border-gray-200 bg-white'>
      <Table
        columns={columns}
        dataSource={dataSource}
        loading={loading}
        rowKey={(record, index) => `${record.measurementPointId}_${index}`}
        rowClassName={() => 'whitespace-nowrap'}
        pagination={{
          defaultPageSize: 10,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
          showTotal: (total) => `Tổng số: ${total} bản ghi`,
          position: ['bottomRight'],
        }}
        scroll={{ x: 'max-content' }}
        bordered
        size='small'
      />
    </div>
  );
};

export default TableActivePointsEnergyReport;
