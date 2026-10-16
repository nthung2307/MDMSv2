import { InboxOutlined, DownloadOutlined } from '@ant-design/icons';
import { Modal, Upload, Button, Alert, Table, Tag, Typography } from 'antd';
import type { UploadFile } from 'antd/es/upload/interface';
import { useState } from 'react';

import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useImportCustomerCsvHook from '@/infrastructure/repository/customer/hooks/useImportCustomerCsv.hook';

const { Dragger } = Upload;
const { Text } = Typography;

interface Props {
  open: boolean;
  handleCancel: () => void;
  onSuccess: () => void;
}

interface ImportResult {
  totalRows: number;
  insertedCount: number;
  updatedCount: number;
  errorCount: number;
  errors: Array<{
    rowNumber: number;
    customerCode?: string;
    errorMessage: string;
  }>;
}

const DialogImportCustomer = ({ open, handleCancel, onSuccess }: Props) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [importCsv] = useImportCustomerCsvHook();

  const handleClose = () => {
    setFileList([]);
    setResult(null);
    handleCancel();
  };

  const handleDownloadTemplate = () => {
    const csvContent =
      '\uFEFFSTT,Tên KH,Mã KH,Số điện thoại,Email,Địa chỉ,Trạng thái\r\n' +
      '1,Công ty Điện lực Mẫu A,KH001,0912345678,kh001@example.com,Số 1 Đường Giải Phóng - Hà Nội,Đang hoạt động\r\n' +
      '2,Nhà máy Sản xuất Kyocera,KH002,0987654321,kyocera@example.com,KCN Thăng Long II - Hưng Yên,Đang hoạt động\r\n';

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'Template_KhachHang.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const handleUpload = async () => {
    if (fileList.length === 0) {
      notifyError('', 'Vui lòng chọn tệp CSV trước khi nhập!');
      return;
    }

    const selected = fileList[0];
    const file = ((selected as any)?.originFileObj || selected) as File;
    if (!file) {
      notifyError('', 'Tệp không hợp lệ, vui lòng chọn lại!');
      return;
    }

    await importCsv(
      file,
      (resData: ImportResult) => {
        setResult(resData);
        notifySuccess(
          '',
          `Nhập dữ liệu thành công! Cập nhật: ${resData.updatedCount}, Thêm mới: ${resData.insertedCount}, Lỗi: ${resData.errorCount}`
        );
        onSuccess();
      },
      (err: any) => {
        const errorMsg =
          err?.data?.message || err?.message || 'Có lỗi xảy ra khi nhập dữ liệu từ CSV!';
        notifyError('', errorMsg);
      },
      setLoading
    );
  };

  const errorColumns = [
    {
      title: 'Dòng',
      dataIndex: 'rowNumber',
      key: 'rowNumber',
      width: 70,
    },
    {
      title: 'Mã KH',
      dataIndex: 'customerCode',
      key: 'customerCode',
      width: 120,
      render: (code: string) => code || <Text type='secondary'>Trống</Text>,
    },
    {
      title: 'Chi tiết lỗi',
      dataIndex: 'errorMessage',
      key: 'errorMessage',
      render: (msg: string) => <Text type='danger'>{msg}</Text>,
    },
  ];

  return (
    <Modal
      title='Nhập dữ liệu khách hàng từ CSV'
      open={open}
      onCancel={handleClose}
      width={720}
      footer={[
        <Button key='cancel' onClick={handleClose}>
          {result ? 'Đóng' : 'Hủy'}
        </Button>,
        !result && (
          <Button
            key='submit'
            type='primary'
            loading={loading}
            disabled={fileList.length === 0}
            onClick={handleUpload}
          >
            Thực hiện nhập
          </Button>
        ),
      ]}
    >
      <div className='flex flex-col gap-4 py-2'>
        <Alert
          message='Quy tắc Nhập dữ liệu An toàn (Safe Upsert)'
          description='Hệ thống đối chiếu theo Mã khách hàng (Mã KH). Khách hàng đã tồn tại sẽ được cập nhật thông tin (bảo toàn 100% liên kết điểm đo và tài khoản). Khách hàng mới sẽ được thêm vào hệ thống.'
          type='info'
          showIcon
        />

        <div className='flex justify-between items-center'>
          <Text type='secondary'>Chỉ hỗ trợ tệp định dạng .csv (UTF-8)</Text>
          <Button
            icon={<DownloadOutlined />}
            size='small'
            type='dashed'
            onClick={handleDownloadTemplate}
          >
            Tải tệp mẫu CSV
          </Button>
        </div>

        {!result ? (
          <Dragger
            name='file'
            accept='.csv'
            maxCount={1}
            fileList={fileList}
            beforeUpload={(file) => {
              const fileName = file.name.toLowerCase();
              const isCsv =
                fileName.endsWith('.csv') ||
                file.type === 'text/csv' ||
                file.type === 'application/vnd.ms-excel';
              if (!isCsv) {
                notifyError('', 'Chỉ chấp nhận tệp định dạng .csv!');
                return Upload.LIST_IGNORE;
              }
              setFileList([file]);
              return false;
            }}
            onChange={(info) => {
              setFileList(info.fileList.slice(-1));
            }}
            onRemove={() => {
              setFileList([]);
            }}
          >
            <p className='ant-upload-drag-icon'>
              <InboxOutlined />
            </p>
            <p className='ant-upload-text'>Kéo thả tệp CSV vào đây hoặc bấm để chọn tệp</p>
            <p className='ant-upload-hint'>Hỗ trợ phân cách dấu phẩy (,) hoặc chấm phẩy (;)</p>
          </Dragger>
        ) : (
          <div className='flex flex-col gap-3'>
            <div className='flex gap-3 p-3 bg-gray-50 rounded border border-gray-200'>
              <Tag color='blue'>Tổng số: {result.totalRows}</Tag>
              <Tag color='green'>Đã cập nhật: {result.updatedCount}</Tag>
              <Tag color='cyan'>Thêm mới: {result.insertedCount}</Tag>
              {result.errorCount > 0 && <Tag color='red'>Lỗi: {result.errorCount}</Tag>}
            </div>

            {result.errors && result.errors.length > 0 && (
              <div className='mt-2'>
                <Text strong className='text-red-500 mb-2 block'>
                  Danh sách dòng bị lỗi ({result.errors.length}):
                </Text>
                <Table
                  dataSource={result.errors.map((e, idx) => ({ ...e, key: idx }))}
                  columns={errorColumns}
                  size='small'
                  pagination={{ pageSize: 5 }}
                />
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};

export default DialogImportCustomer;
