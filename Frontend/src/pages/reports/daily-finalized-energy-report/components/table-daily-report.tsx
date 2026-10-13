import { TableProps } from 'antd';
import dayjs from 'dayjs';

import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableDailyReport = (props: Props) => {
  const { dataSource, loading } = props;

  const dataTable = dataSource.map((item: any) => {
    return {
      createdTime: item.createdTime,
      // Kênh giao
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit,
      chiSoTichLuyDienNangTacDungAPTongKenhGiao: item.chiSoTichLuyDienNangTacDungAPTongKenhGiao,
      chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit,
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiao: item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiao,
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit:
        item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit,
      // Kênh nhận
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit,
      chiSoTichLuyDienNangTacDungAPTongKenhNhan: item.chiSoTichLuyDienNangTacDungAPTongKenhNhan,
      chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit,
      chiSoTichLuyDienNangPhanKhangAQTongKenhNhan: item.chiSoTichLuyDienNangPhanKhangAQTongKenhNhan,
      chiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit:
        item.chiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit,
      key: item.createdTime,
    };
  });

  const columns: TableProps['columns'] = [
    {
      title: 'Thời gian',
      dataIndex: 'createdTime',
      fixed: 'left',
      width: 150,
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
          align: 'center', // Header con căn giữa
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>, // Giá trị căn phải
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

export default TableDailyReport;
