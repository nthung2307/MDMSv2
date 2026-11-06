import { TableProps, Tag, Tooltip } from 'antd';

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

interface EmailRecipientResult {
  email: string;
  status: 'SUCCESS' | 'FAILED' | 'PENDING';
  error?: string | null;
}

interface ParsedStatementStatus {
  overall: string;
  sentTime?: string | null;
  details: EmailRecipientResult[];
}

// Helper phan tich trang thai json hoac chuoi truyen thong
const parseStatusDetail = (rawStatus: any): ParsedStatementStatus => {
  if (!rawStatus) {
    return { overall: 'UNSENT', details: [] };
  }

  if (typeof rawStatus === 'string') {
    const trimmed = rawStatus.trim();
    if (trimmed.startsWith('{') && trimmed.endsWith('}')) {
      try {
        const parsed = JSON.parse(trimmed);
        return {
          overall: parsed.overall || parsed.Overall || 'UNSENT',
          sentTime: parsed.sentTime || parsed.SentTime || null,
          details: (parsed.details || parsed.Details || []).map((d: any) => ({
            email: d.email || d.Email || '',
            status: d.status || d.Status || 'PENDING',
            error: d.error || d.Error || null,
          })),
        };
      } catch (e) {
        // Fallback
      }
    }

    if (
      trimmed === 'SEND_SUCCESS' ||
      trimmed === 'SEND_FAILED' ||
      trimmed === 'PENDING' ||
      trimmed === 'PROCESSING' ||
      trimmed === 'PARTIAL_SUCCESS'
    ) {
      return { overall: trimmed, details: [] };
    }
  }

  return { overall: 'UNSENT', details: [] };
};

// Helper lay toan bo danh sach email cua diem do (toi da 3 email hoac email fallback tu khach hang)
const getPointEmails = (record: any): string[] => {
  const mp = record?.measurementPoint;
  const emails: string[] = [];
  if (mp?.email?.trim()) emails.push(mp.email.trim());
  if (mp?.email2?.trim()) emails.push(mp.email2.trim());
  if (mp?.email3?.trim()) emails.push(mp.email3.trim());

  if (emails.length === 0 && mp?.customer?.email?.trim()) {
    emails.push(mp.customer.email.trim());
  }
  return emails;
};

const isEmailFormatValid = (email: string): boolean => {
  return Boolean(email && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim()));
};

// Helper kiem tra diem do co the gui email duoc khong
const isPointSendable = (record: any): boolean => {
  const emails = getPointEmails(record);
  // Co it nhat 1 email va tat ca email deu dung dinh dang
  const hasValidEmails = emails.length > 0 && emails.every(isEmailFormatValid);
  const parsedStatus = parseStatusDetail(record?.status);

  // 1. Diem do CHUA cau hinh email hoac email khong hop le -> Disable (khong the gui)
  // 2. Diem do DA gui thanh cong tat ca email -> Disable (khong can gui lai)
  // 3. Diem do That bai hoac Gui mot phan -> Enable (cho phep gui bu cac email chua thanh cong)
  return hasValidEmails && parsedStatus.overall !== 'SEND_SUCCESS';
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
      width: 50,
      align: 'center',
      render: (_, record: any) => {
        const canSend = isPointSendable(record);
        const isChecked = canSend && sendData.listSend.some((item: any) => item.id === record.id);

        return (
          <div className='flex justify-center'>
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
      title: 'Mã điểm đo',
      dataIndex: 'measurementPoint',
      width: 160,
      render: (mp: any) => {
        const code = mp?.code || '';
        return (
          <Tooltip title={code}>
            <div className='max-w-[150px] truncate whitespace-nowrap font-medium text-gray-800'>
              {code}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'measurementPoint',
      width: 280,
      render: (mp: any) => {
        const name = mp?.customer?.fullName || '';
        return (
          <Tooltip title={name}>
            <div className='max-w-[270px] truncate whitespace-nowrap font-medium text-gray-700'>
              {name}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Email nhận phiếu báo',
      dataIndex: 'measurementPoint',
      width: 280,
      render: (_, record: any) => {
        const emails = getPointEmails(record);
        if (emails.length === 0) {
          return <span className='text-xs text-amber-500 whitespace-nowrap'>Chưa có email</span>;
        }

        const parsedStatus = parseStatusDetail(record?.status);

        const tooltipContent = (
          <div className='flex flex-col gap-1 py-1 max-w-[320px]'>
            {emails.map((email, idx) => {
              const trimmed = email.trim();
              const isValid = isEmailFormatValid(trimmed);
              const detail = parsedStatus.details.find(
                (d) => d.email.trim().toLowerCase() === trimmed.toLowerCase()
              );
              let statusText = 'Chờ gửi';
              let colorClass = 'text-gray-300';
              if (!isValid) {
                statusText = 'Sai cú pháp';
                colorClass = 'text-red-400';
              } else if (detail?.status === 'SUCCESS' || parsedStatus.overall === 'SEND_SUCCESS') {
                statusText = 'Đã gửi';
                colorClass = 'text-green-400';
              } else if (detail?.status === 'FAILED' || parsedStatus.overall === 'SEND_FAILED') {
                statusText = `Thất bại${detail?.error ? `: ${detail.error}` : ''}`;
                colorClass = 'text-red-400';
              }
              return (
                <div
                  key={idx}
                  className='text-xs flex items-center justify-between gap-2 border-b border-gray-600 last:border-none pb-0.5'
                >
                  <span className='truncate'>{trimmed}</span>
                  <span className={`font-medium ${colorClass} shrink-0`}>[{statusText}]</span>
                </div>
              );
            })}
          </div>
        );

        return (
          <Tooltip title={tooltipContent} placement='topLeft'>
            <div className='max-w-[270px] truncate whitespace-nowrap text-xs text-blue-600 font-normal cursor-pointer hover:underline'>
              {emails.join('; ')}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Trạng thái gửi Email',
      dataIndex: 'status',
      width: 180,
      render: (status, record: any) => {
        const emails = getPointEmails(record);
        const hasValidEmails = emails.length > 0 && emails.every(isEmailFormatValid);

        // 1. Neu diem do chua co email hoac email khong hop le -> luon hien thi canh bao
        if (emails.length === 0) {
          return <Tag color='warning' className='whitespace-nowrap'>Chưa cấu hình Email</Tag>;
        }
        if (!hasValidEmails) {
          return <Tag color='error' className='whitespace-nowrap'>Email không hợp lệ</Tag>;
        }

        const parsed = parseStatusDetail(status);
        switch (parsed.overall) {
          case 'SEND_SUCCESS':
            return <Tag color='success' className='whitespace-nowrap'>Thành công</Tag>;
          case 'PARTIAL_SUCCESS': {
            const successCount = parsed.details.filter((d) => d.status === 'SUCCESS').length;
            const totalCount = emails.length || parsed.details.length;
            return <Tag color='orange' className='whitespace-nowrap'>Gửi một phần ({successCount}/{totalCount})</Tag>;
          }
          case 'SEND_FAILED':
            return <Tag color='error' className='whitespace-nowrap'>Thất bại</Tag>;
          case 'PENDING':
          case 'PROCESSING':
            return <Tag color='processing' className='whitespace-nowrap'>Chờ gửi</Tag>;
          case 'NO_EMAIL_CONFIGURED':
            return <Tag color='warning' className='whitespace-nowrap'>Chưa cấu hình Email</Tag>;
          default:
            return <Tag color='default' className='whitespace-nowrap'>Chưa gửi</Tag>;
        }
      },
    },
    {
      title: 'Thời gian gửi gần nhất',
      dataIndex: 'sentTime',
      width: 180,
      render: (sentTime, record: any) => {
        const emails = getPointEmails(record);
        const hasValidEmails = emails.length > 0 && emails.every(isEmailFormatValid);
        const parsed = parseStatusDetail(record?.status);
        const latestTime = parsed.sentTime || sentTime;

        // Chi hien thi thoi gian gui khi da thuc su gui
        if (
          hasValidEmails &&
          (parsed.overall === 'SEND_SUCCESS' ||
            parsed.overall === 'PARTIAL_SUCCESS' ||
            parsed.overall === 'SEND_FAILED') &&
          latestTime
        ) {
          const timeStr = convertDateTime(latestTime, 'dd/mm/yyyy hh:mi:ss');
          return (
            <Tooltip title={timeStr}>
              <div className='whitespace-nowrap text-gray-700'>{timeStr}</div>
            </Tooltip>
          );
        }

        return <span className='text-gray-400 whitespace-nowrap'>-</span>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      scroll={{ x: 'max-content' }}
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
