import { TableProps } from 'antd';
import dayjs from 'dayjs';

import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any[];
  loading?: boolean;
}

const TableYearlyReport = (props: Props) => {
  const { dataSource, loading } = props;

  const dataTable = dataSource.map((item: any, index: number) => {
    return {
      key: item.id || item.createdTime || index,
      createdTime: item.createdTime,
      // Kênh giao
      chiSoTichLuyDienNangTacDungAPTongKenhGiao: item.chiSoTichLuyDienNangTacDungAPTongKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao,
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiao: item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiao,
      // Kênh nhận
      chiSoTichLuyDienNangTacDungAPTongKenhNhan: item.chiSoTichLuyDienNangTacDungAPTongKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan,
      chiSoTichLuyDienNangPhanKhangAQTongKenhNhan: item.chiSoTichLuyDienNangPhanKhangAQTongKenhNhan,
    };
  });

  const columns: TableProps['columns'] = [
    {
      title: 'Mốc thời gian (0:00)',
      dataIndex: 'createdTime',
      fixed: 'left',
      width: 170,
      align: 'center',
      render: (record) => {
        return record ? dayjs(record).format('DD/MM/YYYY HH:mm') : 'N/A';
      },
    },
    {
      title: 'Kênh giao (kWh)',
      align: 'center',
      children: [
        {
          title: 'Tổng',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
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
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 1 (BT)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 2 (CD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 3 (TD)',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'VC (kVArh)',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhNhan',
          width: 120,
          align: 'center',
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
      ],
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataTable}
      pagination={false}
      loading={loading}
      scroll={{ x: 1350 }}
      bordered
    />
  );
};

export default TableYearlyReport;
