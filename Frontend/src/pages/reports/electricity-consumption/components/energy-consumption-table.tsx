import { Table, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import React from 'react';

import { EnergyConsumptionItem } from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest';

const { Text } = Typography;

interface EnergyConsumptionTableProps {
  data: EnergyConsumptionItem[];
  total?: EnergyConsumptionItem;
  loading: boolean;
  compareWithPrevious?: boolean;
}

export const EnergyConsumptionTable: React.FC<EnergyConsumptionTableProps> = ({
  data,
  total,
  loading,
  compareWithPrevious = false,
}) => {
  const formatNumber = (val?: number, fractionDigits = 2) => {
    if (val === undefined || val === null || isNaN(val)) return '-';
    return Number(val).toLocaleString('vi-VN', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    });
  };

  const renderDelta = (delta?: number, pct?: number) => {
    if (delta === undefined || delta === null) return '-';
    const isPositive = delta > 0;
    const isNegative = delta < 0;
    const color = isPositive ? '#cf1322' : isNegative ? '#389e0d' : '#8c8c8c';
    const sign = isPositive ? '+' : '';
    const pctText = pct !== undefined && pct !== null ? ` (${sign}${formatNumber(pct, 1)}%)` : '';
    return (
      <span style={{ color, fontWeight: 500 }}>
        {sign}
        {formatNumber(delta, 2)}
        {pctText}
      </span>
    );
  };

  const columns: ColumnsType<EnergyConsumptionItem> = [
    {
      title: 'Thời gian',
      dataIndex: 'timeLabel',
      key: 'timeLabel',
      width: 140,
      fixed: 'left',
      align: 'center',
      render: (val: string) => <Text strong>{val || '-'}</Text>,
    },
    {
      title: 'Hệ số',
      dataIndex: 'multiplierFactor',
      key: 'multiplierFactor',
      width: 80,
      align: 'center',
      render: (val: number) => formatNumber(val, 0),
    },
    {
      title: 'Sản lượng Kênh Giao (kWh)',
      children: [
        {
          title: 'Tổng kỳ này',
          dataIndex: 'sanLuongTacDungTongKenhGiao',
          key: 'sanLuongTacDungTongKenhGiao',
          width: 115,
          align: 'right',
          render: (val) => <Text strong>{formatNumber(val, 2)}</Text>,
        },
        ...(compareWithPrevious
          ? [
              {
                title: 'Tổng kỳ trước',
                dataIndex: 'previousSanLuongTacDungTongKenhGiao',
                key: 'previousSanLuongTacDungTongKenhGiao',
                width: 115,
                align: 'right' as const,
                render: (val: number) => formatNumber(val, 2),
              },
              {
                title: 'So sánh (+/-)',
                key: 'deltaGiao',
                width: 130,
                align: 'right' as const,
                render: (_: any, record: EnergyConsumptionItem) =>
                  renderDelta(record.deltaKenhGiao, record.deltaPercentageKenhGiao),
              },
            ]
          : []),
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'sanLuongTacDungBTKenhGiao',
          key: 'sanLuongTacDungBTKenhGiao',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'sanLuongTacDungCDKenhGiao',
          key: 'sanLuongTacDungCDKenhGiao',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'sanLuongTacDungTDKenhGiao',
          key: 'sanLuongTacDungTDKenhGiao',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'sanLuongPhanKhangTongKenhGiao',
          key: 'sanLuongPhanKhangTongKenhGiao',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
      ],
    },
    {
      title: 'Sản lượng Kênh Nhận (kWh)',
      children: [
        {
          title: 'Tổng kỳ này',
          dataIndex: 'sanLuongTacDungTongKenhNhan',
          key: 'sanLuongTacDungTongKenhNhan',
          width: 115,
          align: 'right',
          render: (val) => <Text strong>{formatNumber(val, 2)}</Text>,
        },
        ...(compareWithPrevious
          ? [
              {
                title: 'Tổng kỳ trước',
                dataIndex: 'previousSanLuongTacDungTongKenhNhan',
                key: 'previousSanLuongTacDungTongKenhNhan',
                width: 115,
                align: 'right' as const,
                render: (val: number) => formatNumber(val, 2),
              },
              {
                title: 'So sánh (+/-)',
                key: 'deltaNhan',
                width: 130,
                align: 'right' as const,
                render: (_: any, record: EnergyConsumptionItem) =>
                  renderDelta(record.deltaKenhNhan, record.deltaPercentageKenhNhan),
              },
            ]
          : []),
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'sanLuongTacDungBTKenhNhan',
          key: 'sanLuongTacDungBTKenhNhan',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'sanLuongTacDungCDKenhNhan',
          key: 'sanLuongTacDungCDKenhNhan',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'sanLuongTacDungTDKenhNhan',
          key: 'sanLuongTacDungTDKenhNhan',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'sanLuongPhanKhangTongKenhNhan',
          key: 'sanLuongPhanKhangTongKenhNhan',
          width: 105,
          align: 'right',
          render: (val) => formatNumber(val, 2),
        },
      ],
    },
  ];

  return (
    <div className='w-full overflow-x-auto'>
      <Table<EnergyConsumptionItem>
        columns={columns}
        dataSource={data}
        rowKey={(record, index) => record.timeLabel || index?.toString() || ''}
        loading={loading}
        bordered
        size='small'
        pagination={false}
        scroll={{ x: compareWithPrevious ? 1600 : 1250, y: 550 }}
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
                <Table.Summary.Cell index={1} align='center'>
                  -
                </Table.Summary.Cell>
                {/* Kênh Giao */}
                <Table.Summary.Cell index={2} align='right'>
                  <Text strong className='text-red-700'>
                    {formatNumber(total.sanLuongTacDungTongKenhGiao, 2)}
                  </Text>
                </Table.Summary.Cell>
                {compareWithPrevious && (
                  <>
                    <Table.Summary.Cell index={3} align='right'>
                      <Text strong className='text-blue-700'>
                        {formatNumber(total.previousSanLuongTacDungTongKenhGiao, 2)}
                      </Text>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={4} align='right'>
                      {renderDelta(total.deltaKenhGiao, total.deltaPercentageKenhGiao)}
                    </Table.Summary.Cell>
                  </>
                )}
                <Table.Summary.Cell index={compareWithPrevious ? 5 : 3} align='right'>
                  {formatNumber(total.sanLuongTacDungBTKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 6 : 4} align='right'>
                  {formatNumber(total.sanLuongTacDungCDKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 7 : 5} align='right'>
                  {formatNumber(total.sanLuongTacDungTDKenhGiao, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 8 : 6} align='right'>
                  {formatNumber(total.sanLuongPhanKhangTongKenhGiao, 2)}
                </Table.Summary.Cell>

                {/* Kênh Nhận */}
                <Table.Summary.Cell index={compareWithPrevious ? 9 : 7} align='right'>
                  <Text strong className='text-emerald-700'>
                    {formatNumber(total.sanLuongTacDungTongKenhNhan, 2)}
                  </Text>
                </Table.Summary.Cell>
                {compareWithPrevious && (
                  <>
                    <Table.Summary.Cell index={10} align='right'>
                      <Text strong className='text-blue-700'>
                        {formatNumber(total.previousSanLuongTacDungTongKenhNhan, 2)}
                      </Text>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={11} align='right'>
                      {renderDelta(total.deltaKenhNhan, total.deltaPercentageKenhNhan)}
                    </Table.Summary.Cell>
                  </>
                )}
                <Table.Summary.Cell index={compareWithPrevious ? 12 : 8} align='right'>
                  {formatNumber(total.sanLuongTacDungBTKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 13 : 9} align='right'>
                  {formatNumber(total.sanLuongTacDungCDKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 14 : 10} align='right'>
                  {formatNumber(total.sanLuongTacDungTDKenhNhan, 2)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={compareWithPrevious ? 15 : 11} align='right'>
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
