import { Table, Tooltip, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import React, { useMemo } from 'react';

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
  const formatNumber = (val?: number | null, fractionDigits = 0) => {
    if (val === undefined || val === null || isNaN(Number(val))) return '-';
    return Math.round(Number(val)).toLocaleString('vi-VN', {
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

    const rounded = Math.round(num);
    const formatted = rounded.toLocaleString('vi-VN', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
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
      title: 'STT',
      key: 'stt',
      width: 60,
      align: 'center',
      className: 'whitespace-nowrap',
      render: (_val, _record, index) => <span className='whitespace-nowrap'>{index + 1}</span>,
    },
    {
      title: 'Thời gian',
      dataIndex: 'timeLabel',
      key: 'timeLabel',
      width: 140,
      align: 'center',
      className: 'whitespace-nowrap',
      render: (val: string) => <Text strong>{val || '-'}</Text>,
    },
    {
      title: 'Kênh giao (kWh)',
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
      title: 'Kênh nhận (kWh)',
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

  const summaryData = useMemo(() => {
    const keys: (keyof EnergyConsumptionItem)[] = [
      'sanLuongTacDungTongKenhGiao',
      'sanLuongTacDungBTKenhGiao',
      'sanLuongTacDungCDKenhGiao',
      'sanLuongTacDungTDKenhGiao',
      'sanLuongPhanKhangTongKenhGiao',
      'sanLuongTacDungTongKenhNhan',
      'sanLuongTacDungBTKenhNhan',
      'sanLuongTacDungCDKenhNhan',
      'sanLuongTacDungTDKenhNhan',
      'sanLuongPhanKhangTongKenhNhan',
    ];

    const sumRow: Record<string, any> = {};
    const minRow: Record<string, any> = {};
    const maxRow: Record<string, any> = {};

    keys.forEach((key) => {
      let sum = 0;
      let count = 0;
      let minVal = Infinity;
      let maxVal = -Infinity;

      data.forEach((it) => {
        const v = it[key];
        if (v !== undefined && v !== null && v !== '' && !isNaN(Number(v))) {
          const n = Number(v);
          sum += n;
          count++;
          if (n < minVal) minVal = n;
          if (n > maxVal) maxVal = n;
        }
      });

      if (count === 0) {
        sumRow[key] = null;
        minRow[key] = null;
        maxRow[key] = null;
      } else {
        if (
          total &&
          total[key] !== undefined &&
          total[key] !== null &&
          total[key] !== '' &&
          !isNaN(Number(total[key]))
        ) {
          sumRow[key] = Math.round(Number(total[key]));
        } else {
          sumRow[key] = Math.round(sum);
        }
        minRow[key] = Math.round(minVal);
        maxRow[key] = Math.round(maxVal);
      }
    });

    return { sumRow, minRow, maxRow };
  }, [data, total]);

  const renderSummaryCell = (val: any, isBold = false) => {
    if (val === undefined || val === null || val === '' || isNaN(Number(val))) {
      return <div className='text-gray-400 text-center select-none'>-</div>;
    }
    const formatted = Math.round(Number(val)).toLocaleString('vi-VN');
    return (
      <div className={isBold ? 'font-bold text-gray-900' : 'font-medium text-gray-800'}>
        {formatted}
      </div>
    );
  };

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
        scroll={{ x: 'max-content', y: 550 }}
        summary={() => {
          if (!data || data.length === 0) return null;
          const { sumRow, minRow, maxRow } = summaryData;
          return (
            <Table.Summary>
              {/* Dòng 1: Tổng (Đơn sắc, viền đậm) */}
              <Table.Summary.Row className='bg-gray-100 font-semibold border-t-2 border-gray-300'>
                <Table.Summary.Cell index={0} align='center'>
                  <span className='font-bold text-gray-900'>Tổng</span>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={1} align='center'>
                  <div className='text-gray-400 text-center select-none'>-</div>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={2} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungTongKenhGiao, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={3} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungBTKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={4} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungCDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={5} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungTDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={6} align='right'>
                  {renderSummaryCell(sumRow.sanLuongPhanKhangTongKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={7} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungTongKenhNhan, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={8} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungBTKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={9} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungCDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={10} align='right'>
                  {renderSummaryCell(sumRow.sanLuongTacDungTDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={11} align='right'>
                  {renderSummaryCell(sumRow.sanLuongPhanKhangTongKenhNhan)}
                </Table.Summary.Cell>
              </Table.Summary.Row>

              {/* Dòng 2: Min (Đơn sắc) */}
              <Table.Summary.Row className='bg-gray-50/80 font-medium'>
                <Table.Summary.Cell index={0} align='center'>
                  <span className='font-semibold text-gray-700'>Min</span>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={1} align='center'>
                  <div className='text-gray-400 text-center select-none'>-</div>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={2} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungTongKenhGiao, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={3} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungBTKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={4} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungCDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={5} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungTDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={6} align='right'>
                  {renderSummaryCell(minRow.sanLuongPhanKhangTongKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={7} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungTongKenhNhan, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={8} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungBTKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={9} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungCDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={10} align='right'>
                  {renderSummaryCell(minRow.sanLuongTacDungTDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={11} align='right'>
                  {renderSummaryCell(minRow.sanLuongPhanKhangTongKenhNhan)}
                </Table.Summary.Cell>
              </Table.Summary.Row>

              {/* Dòng 3: Max (Đơn sắc) */}
              <Table.Summary.Row className='bg-gray-50/80 font-medium'>
                <Table.Summary.Cell index={0} align='center'>
                  <span className='font-semibold text-gray-700'>Max</span>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={1} align='center'>
                  <div className='text-gray-400 text-center select-none'>-</div>
                </Table.Summary.Cell>
                <Table.Summary.Cell index={2} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungTongKenhGiao, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={3} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungBTKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={4} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungCDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={5} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungTDKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={6} align='right'>
                  {renderSummaryCell(maxRow.sanLuongPhanKhangTongKenhGiao)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={7} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungTongKenhNhan, true)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={8} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungBTKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={9} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungCDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={10} align='right'>
                  {renderSummaryCell(maxRow.sanLuongTacDungTDKenhNhan)}
                </Table.Summary.Cell>
                <Table.Summary.Cell index={11} align='right'>
                  {renderSummaryCell(maxRow.sanLuongPhanKhangTongKenhNhan)}
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
