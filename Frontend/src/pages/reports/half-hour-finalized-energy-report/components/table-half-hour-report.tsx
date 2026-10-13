import { TableProps } from 'antd';
import dayjs from 'dayjs';

import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableHalfHourReport = (props: Props) => {
  const { dataSource, loading } = props;

  const dataTable = dataSource.map((item: any) => {
    return {
      createdTime: item.createdTime,
      // Kênh giao - Biểu giá TD
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit,
      // Kênh giao - Biểu giá CD
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit,
      // Kênh giao - Biểu giá BT
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit,
      // Kênh giao - Tổng
      chiSoTichLuyDienNangTacDungAPTongKenhGiao: item.chiSoTichLuyDienNangTacDungAPTongKenhGiao,
      chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit:
        item.chiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit,
      // Kênh giao - Phản kháng
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiao: item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiao,
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit:
        item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit,
      // Kênh nhận - Biểu giá TD
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit,
      // Kênh nhận - Biểu giá CD
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit,
      // Kênh nhận - Biểu giá BT
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan,
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit,
      // Kênh nhận - Tổng
      chiSoTichLuyDienNangTacDungAPTongKenhNhan: item.chiSoTichLuyDienNangTacDungAPTongKenhNhan,
      chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit:
        item.chiSoTichLuyDienNangTacDungAPTongKenhNhanUnit,
      // Kênh nhận - Phản kháng
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
          align: 'center',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
          width: 120,
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 1 (BT)',
          align: 'center',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
          width: 120,
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 2 (CD)',
          align: 'center',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
          width: 120,
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'Biểu 3 (TD)',
          align: 'center',
          dataIndex: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
          width: 120,
          render: (value) => <div style={{ textAlign: 'right' }}>{value ?? 'N/A'}</div>,
        },
        {
          title: 'VC (kVArh)',
          align: 'center',
          dataIndex: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
          width: 120,
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

export default TableHalfHourReport;
