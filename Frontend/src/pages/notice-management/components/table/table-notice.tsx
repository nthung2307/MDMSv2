import { TableProps, Tag } from 'antd';

import { CheckboxCommon } from '@/infrastructure/common/components/Checkbox/checkbox-common';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime } from '@/infrastructure/utils/helpers.ts';

interface Props {
  dataSource: any;
  sendData: any;
  setSendData: any;
  setIsShowDialogPDF: any;
  loading: boolean;
}

// Helper kiem tra diem do co the gui email duoc khong
export const isPointSendable = (record: any): boolean => {
  const pointEmail =
    record?.measurementPoint?.email?.trim() ||
    record?.measurementPoint?.customer?.email?.trim();
  const hasValidEmail = Boolean(pointEmail && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(pointEmail));
  const isAlreadySentSuccess = record?.status === 'SEND_SUCCESS';

  // 1. Diem do CHUA cau hinh email -> Disable (khong the gui)
  // 2. Diem do DA gui thanh cong -> Disable (khong can gui lai)
  return hasValidEmail && !isAlreadySentSuccess;
};

const TableDetailNotice = (props: Props) => {
  const { dataSource, setSendData, sendData, setIsShowDialogPDF, loading } = props;

  // Danh sach cac ban ghi du dieu kien de gui email
  const sendableItems = (dataSource || []).filter(isPointSendable);
  const isAllSendableChecked =
    sendableItems.length > 0 &&
    sendableItems.every((item: any) =>
      sendData.listSend.some((selected: any) => selected.id === item.id)
    );

  const toggleSelectAll = () => {
    if (isAllSendableChecked) {
      setSendData({ isAll: false, listSend: [] });
    } else {
      setSendData({ isAll: false, listSend: [...sendableItems] });
    }
  };

  const addSendMail = (record: any) => {
    if (!isPointSendable(record)) return;
    let arr = [...sendData.listSend];
    const index = arr.findIndex((item) => item.id === record.id);
    if (index !== -1) {
      arr = arr.filter((item) => item.id !== record.id);
    } else {
      arr = [...arr, record];
    }
    setSendData({ isAll: false, listSend: arr });
  };

  const columns: TableProps['columns'] = [
    {
      title: (
        <div>
          <CheckboxCommon
            dataAttribute={isAllSendableChecked}
            setData={toggleSelectAll}
            disabled={sendableItems.length === 0}
            label={undefined}
          />
        </div>
      ),
      dataIndex: 'measurementPoint',
      width: '5%',
      render: (_, record: any) => {
        const canSend = isPointSendable(record);
        const isChecked =
          canSend && sendData.listSend.some((item: any) => item.id === record.id);

        return (
          <div>
            <CheckboxCommon
              dataAttribute={isChecked}
              disabled={!canSend}
              setData={() => {
                if (canSend) {
                  addSendMail(record);
                }
              }}
              label={undefined}
            />
          </div>
        );
      },
    },
    {
      title: 'Mã khách hàng',
      dataIndex: 'measurementPoint',
      render: (value) => {
        return <div>{value.customer.customerCode}</div>;
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'measurementPoint',
      render: (record) => {
        return <div>{record.customer.fullName}</div>;
      },
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPoint',
      render: (record) => {
        const email = record?.email?.trim() || record?.customer?.email?.trim();
        return (
          <div>
            <div className="font-medium">{record?.code}</div>
            {email ? (
              <div style={{ fontSize: '11px', color: '#1677ff' }}>{email}</div>
            ) : (
              <div style={{ fontSize: '11px', color: '#faad14' }}>Chưa có email</div>
            )}
          </div>
        );
      },
    },
    {
      title: 'Trạng thái gửi Email',
      dataIndex: 'status',
      render: (status, record: any) => {
        const pointEmail =
          record?.measurementPoint?.email?.trim() ||
          record?.measurementPoint?.customer?.email?.trim();
        const hasValidEmail = Boolean(pointEmail && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(pointEmail));

        // 1. Neu diem do chua co email hoac email khong hop le -> luon hien thi Chua cau hinh Email
        if (!hasValidEmail) {
          return <Tag color="warning">Chưa cấu hình Email</Tag>;
        }

        // 2. Neu diem do da co email hop le -> hien thi theo tien trinh gui
        switch (status) {
          case 'SEND_SUCCESS':
            return <Tag color="success">Thành công</Tag>;
          case 'SEND_FAILED':
            return <Tag color="error">Thất bại</Tag>;
          case 'PENDING':
          case 'PROCESSING':
            return <Tag color="processing">Chờ gửi</Tag>;
          case 'NO_EMAIL_CONFIGURED':
          default:
            return <Tag color="default">Chưa gửi</Tag>;
        }
      },
    },
    {
      title: 'Thời gian gửi gần nhất',
      dataIndex: 'sentTime',
      render: (sentTime, record: any) => {
        const pointEmail =
          record?.measurementPoint?.email?.trim() ||
          record?.measurementPoint?.customer?.email?.trim();
        const hasValidEmail = Boolean(pointEmail && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(pointEmail));

        // Chi hien thi thoi gian gui khi da thuc su gui thanh cong hoac that bai
        if (
          hasValidEmail &&
          (record?.status === 'SEND_SUCCESS' || record?.status === 'SEND_FAILED') &&
          sentTime
        ) {
          return <div>{convertDateTime(sentTime, 'dd/mm/yyyy hh:mi:ss')}</div>;
        }

        return <span style={{ color: '#bfbfbf' }}>-</span>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      // scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
      onRow={(record) => {
        return {
          onClick: (e) => {
            if ((e.target as HTMLInputElement).type !== 'checkbox') {
              setIsShowDialogPDF({ status: true, record });
            }
          },
        };
      }}
      loading={loading}
    />
  );
};

export default TableDetailNotice;
