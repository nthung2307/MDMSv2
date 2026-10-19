import { Table, Tooltip, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import React from 'react';

import { EnergyConsumptionItem } from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest';

const { Text } = Typography;

interface EnergyConsumptionTableProps {
  data: EnergyConsumptionItem[];
  total?: EnergyConsumptionItem;
  loading: boolean;
}

export const EnergyConsumptionTable: React.FC<EnergyConsumptionTableProps> = ({
  data,
  total,
  loading,
}) => {
  const formatNumber = (val?: number | null, fractionDigits = 2) => {
    if (val === undefined || val === null || isNaN(Number(val))) return '-';
    return Number(val).toLocaleString('vi-VN', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    });
  };

  const renderEnergyCell = (val: any, record: EnergyConsumptionItem, isBold = false) => {
    if (val === undefined || val === null || val === '') {
      return <div className='text-gray-400 text-center select-none'>-</div>;
    }
    const num = Number(val);
    if (isNaN(num)) {
      return <div className='text-gray-400 text-center select-none'>-</div>;
    }

    const formatted = num.toLocaleString('vi-VN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });

    if (record?.isSuspected) {
      return (
        <Tooltip title='Số liệu bị nghi ngờ'>
          <div className='text-amber-600 font-medium inline-flex items-center justify-end w-full gap-1 cursor-help'>
            <span className={isBold ? 'font-bold' : ''}>{formatted}</span>
            <span className='text-amber-500 font-bold text-base leading-none'>*</span>
          </div>
        </Tooltip>
      );
    }

    return <div className={isBold ? 'font-bold' : ''}>{formatted}</div>;
  };

  const columns: ColumnsType<EnergyConsumptionItem> = [
    {
      title: 'Thời gian',
      dataIndex: 'timeLabel',
      key: 'timeLabel',
      width: 140,
      fixed: 'left',
      align: 'center',
      className: 'whitespace-nowrap',
      render: (val: string) => <Text strong>{val || '-'}</Text>,
    },
    {
      title: 'Sản lượng Kênh Giao (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'sanLuongTacDungTongKenhGiao',
          key: 'sanLuongTacDungTongKenhGiao',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record, true),
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'sanLuongTacDungBTKenhGiao',
          key: 'sanLuongTacDungBTKenhGiao',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'sanLuongTacDungCDKenhGiao',
          key: 'sanLuongTacDungCDKenhGiao',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'sanLuongTacDungTDKenhGiao',
          key: 'sanLuongTacDungTDKenhGiao',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'sanLuongPhanKhangTongKenhGiao',
          key: 'sanLuongPhanKhangTongKenhGiao',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
      ],
    },
    {
      title: 'Sản lượng Kênh Nhận (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'sanLuongTacDungTongKenhNhan',
          key: 'sanLuongTacDungTongKenhNhan',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record, true),
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'sanLuongTacDungBTKenhNhan',
          key: 'sanLuongTacDungBTKenhNhan',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'sanLuongTacDungCDKenhNhan',
          key: 'sanLuongTacDungCDKenhNhan',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'sanLuongTacDungTDKenhNhan',
          key: 'sanLuongTacDungTDKenhNhan',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'sanLuongPhanKhangTongKenhNhan',
          key: 'sanLuongPhanKhangTongKenhNhan',
          width: 125,
          align: 'right',
          className: 'whitespace-nowrap',
          render: (val, record) => renderEnergyCell(val, record),
        },
      ],
    },
  ];

  return (
    <div className='w-full min-w-full overflow-x-auto rounded-lg border border-gray-200 bg-white'>
      <Table<EnergyConsumptionItem>
        columns={columns}
        dataSource={data}
        rowKey={(record, index) => record.timeLabel || index?.toString() || ''}
        loading={loading}
        bordered
        size='small'
        className='w-full'
        rowClassName={() => 'whitespace-nowrap'}
        pagination={{
          defaultPageSize: 48,
          showSizeChanger: true,
          pageSizeOptions: ['24', '48', '96', '192'],
          showTotal: (totalCount) => `Tổng số: ${totalCount} bản ghi`,
          position: ['bottomRight'],
        }}
        scroll={{ x: 1390, y: 550 }}
        summary={() => {
          if (!total) return null;
          return (
            <Table.Summary fixed='bottom'>
              <Table.Summary.Row className='bg-amber-50 font-semibold'>
                <Table.Summary.Cell index={0} align='center'>
                  <Text strong className='text-amber-900'>
                    TỔNG CỘNG
                  </Text>
                </Table.Summary.Cell>
                {/* Kênh Giao */}
                <Table.Summary.Cell index={1} align='right'>
                  <Text strong className='text-red-700'>
                    {formatNumber(total.sanLuongTacDungTongKenhGiao, 2)}
                  </Text>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={2} align='right'>
                  {formatNumber(total.sanLuongTacDungBTKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={3} align='right'>
                  {formatNumber(total.sanLuongTacDungCDKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={4} align='right'>
                  {formatNumber(total.sanLuongTacDungTDKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={5} align='right'>
                  {formatNumber(total.sanLuongPhanKhangTongKenhGiao, 2)}
                </Table.Summary.Cell>

                {/* Kênh Nhận */}
                <Table.Summary.Cell index={6} align='right'>
                  <Text strong className='text-emerald-700'>
                    {formatNumber(total.sanLuongTacDungTongKenhNhan, 2)}
                  </Text>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={7} align='right'>
                  {formatNumber(total.sanLuongTacDungBTKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={8} align='right'>
                  {formatNumber(total.sanLuongTacDungCDKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={9} align='right'>
                  {formatNumber(total.sanLuongTacDungTDKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={10} align='right'>
                  {formatNumber(total.sanLuongPhanKhangTongKenhNhan, 2)}
                </Table.Summary.Cell>
              </Table.Summary.Row>
            </Table.Summary>
          );
        }}
      />
    </div>
  );
};

export default EnergyConsumptionTable;
