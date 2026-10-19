import { Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import React, { useMemo } from 'react';

import { OperatingParameterItem } from '@/core/application/dto/reports/requests/GetOperatingParameterReportRequest';

interface OperatingParameterTableProps {
  data: OperatingParameterItem[];
  loading: boolean;
  total?: number;
  voltageUnit?: string;
}

export const OperatingParameterTable: React.FC<OperatingParameterTableProps> = ({
  data,
  loading,
  voltageUnit: propVoltageUnit,
}) => {
  const voltageUnit = useMemo(() => {
    if (propVoltageUnit) return propVoltageUnit;
    const itemWithUnit = data.find((d) => d.dienApPhaAUnit);
    return itemWithUnit?.dienApPhaAUnit || 'kV';
  }, [data, propVoltageUnit]);

  const formatNumber = (val?: number, fractionDigits = 2) => {
    if (val === undefined || val === null || isNaN(val)) return '-';
    return Number(val).toLocaleString('vi-VN', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    });
  };

  const columns: ColumnsType<OperatingParameterItem> = [
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      key: 'createdTime',
      width: '14%',
      align: 'center',
      render: (val: string) => {
        if (!val) return '-';
        const d = new Date(val);
        return d.toLocaleString('vi-VN', {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
        });
      },
    },
    {
      title: `Điện áp (${voltageUnit})`,
      align: 'center',
      children: [
        {
          title: 'Ua',
          dataIndex: 'dienApPhaA',
          key: 'dienApPhaA',
          width: '7.5%',
          align: 'center',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Ub',
          dataIndex: 'dienApPhaB',
          key: 'dienApPhaB',
          width: '7.5%',
          align: 'center',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Uc',
          dataIndex: 'dienApPhaC',
          key: 'dienApPhaC',
          width: '7.5%',
          align: 'center',
          render: (val) => formatNumber(val, 2),
        },
      ],
    },
    {
      title: 'Dòng điện (A)',
      align: 'center',
      children: [
        {
          title: 'Ia',
          dataIndex: 'dongDienPhaA',
          key: 'dongDienPhaA',
          width: '8%',
          align: 'center',
          render: (val) => formatNumber(val, 1),
        },
        {
          title: 'Ib',
          dataIndex: 'dongDienPhaB',
          key: 'dongDienPhaB',
          width: '8%',
          align: 'center',
          render: (val) => formatNumber(val, 1),
        },
        {
          title: 'Ic',
          dataIndex: 'dongDienPhaC',
          key: 'dongDienPhaC',
          width: '8%',
          align: 'center',
          render: (val) => formatNumber(val, 1),
        },
      ],
    },
    {
      title: 'Công suất tác dụng (kW)',
      align: 'center',
      children: [
        {
          title: 'Pt',
          dataIndex: 'congSuatTacDungTong3Pha',
          key: 'congSuatTacDungTong3Pha',
          width: '11%',
          align: 'center',
          render: (val) => formatNumber(val, 1),
        },
      ],
    },
    {
      title: 'Công suất phản kháng (kVAr)',
      align: 'center',
      children: [
        {
          title: 'Qt',
          dataIndex: 'congSuatPhanKhangTong3Pha',
          key: 'congSuatPhanKhangTong3Pha',
          width: '11%',
          align: 'center',
          render: (val) => formatNumber(val, 1),
        },
      ],
    },
    {
      title: 'Hệ số PF',
      dataIndex: 'heSoCongSuatPF',
      key: 'heSoCongSuatPF',
      width: '8.5%',
      align: 'center',
      render: (val) => formatNumber(val, 2),
    },
    {
      title: 'Tần số (Hz)',
      dataIndex: 'tanSoHeThong',
      key: 'tanSoHeThong',
      width: '9%',
      align: 'center',
      render: (val) => formatNumber(val, 2),
    },
  ];

  return (
    <div className='w-full overflow-x-auto rounded-lg border border-gray-200 bg-white'>
      <Table<OperatingParameterItem>
        columns={columns}
        dataSource={data}
        rowKey={(record, index) => record.id?.toString() || index?.toString() || ''}
        loading={loading}
        bordered
        size='small'
        className='w-full'
        pagination={{
          defaultPageSize: 48,
          showSizeChanger: true,
          pageSizeOptions: ['24', '48', '96', '192'],
          showTotal: (total) => `Tổng số: ${total} bản ghi`,
          position: ['bottomRight'],
        }}
        scroll={{ x: '100%' }}
      />
    </div>
  );
};

export default OperatingParameterTable;
