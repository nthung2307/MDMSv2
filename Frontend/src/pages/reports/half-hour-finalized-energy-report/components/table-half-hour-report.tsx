import { Table, TableProps, Tooltip } from 'antd';
import dayjs from 'dayjs';
import React from 'react';

interface Props {
  dataSource: any[];
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
      <div style={{ textAlign: 'center' }} className='text-gray-400 select-none'>
        -
      </div>
    );
  }

  if (isNotTopical) {
    return (
      <Tooltip title='Số liệu bị nghi ngờ'>
        <div
          style={{ textAlign: 'right', paddingRight: '8px' }}
          className='text-amber-600 font-medium inline-flex items-center justify-end w-full gap-1 cursor-help'
        >
          <span>{text}</span>
          <span className='text-amber-500 font-bold text-base leading-none'>*</span>
        </div>
      </Tooltip>
    );
  }

  return (
    <div style={{ textAlign: 'right', paddingRight: '8px' }} className='text-gray-800'>
      {text}
    </div>
  );
};

const TableHalfHourReport: React.FC<Props> = ({ dataSource, loading }) => {
  const columns: TableProps<any>['columns'] = [
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      fixed: 'left',
      width: 160,
      align: 'center',
      render: (val: any) => (val ? dayjs(val).format('DD/MM/YYYY HH:mm') : '-'),
    },
    {
      title: 'Kênh giao (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
          width: 125,
          align: 'center',
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
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan',
          width: 125,
          align: 'center',
          render: renderEnergyCell,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhNhan',
          width: 125,
          align: 'center',
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
        rowKey={(record, index) => `${record.createdTime || record.id || index}_${index}`}
        pagination={{
          defaultPageSize: 48,
          showSizeChanger: true,
          pageSizeOptions: ['24', '48', '96', '192'],
          showTotal: (total) => `Tổng số: ${total} bản ghi`,
          position: ['bottomRight'],
        }}
        scroll={{ x: 1410 }}
        bordered
        size='small'
      />
    </div>
  );
};

export default TableHalfHourReport;
