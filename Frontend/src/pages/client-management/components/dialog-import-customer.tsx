import { InboxOutlined, DownloadOutlined } from '@ant-design/icons';
import { Modal, Upload, Button, Alert, Table, Tag, Typography } from 'antd';
import type { UploadFile } from 'antd/es/upload/interface';
import { useState } from 'react';

import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useImportCustomerCsvHook from '@/infrastructure/repository/customer/hooks/useImportCustomerCsv.hook';
import { convertExcelFileToCsvFile, downloadExcelTemplate } from '@/infrastructure/utils/excelHelper';

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

  const handleDownloadTemplate = async () => {
    try {
      await downloadExcelTemplate(
        'Template_KhachHang.xlsx',
        'KhachHang',
        [
          { header: 'STT', key: 'stt', width: 8 },
          { header: 'Tên KH', key: 'tenKh', width: 32 },
          { header: 'Mã KH', key: 'maKh', width: 16 },
          { header: 'Số điện thoại', key: 'sdt', width: 16 },
          { header: 'Email', key: 'email', width: 28 },
          { header: 'Địa chỉ', key: 'diaChi', width: 36 },
          { header: 'Trạng thái', key: 'trangThai', width: 16 },
        ],
        [
          {
            stt: 1,
            tenKh: 'Công ty Điện lực Mẫu A',
            maKh: 'KH001',
            sdt: '0912345678',
            email: 'kh001@example.com',
            diaChi: 'Số 1 Đường Giải Phóng - Hà Nội',
            trangThai: 'Đang hoạt động',
          },
          {
            stt: 2,
            tenKh: 'Nhà máy Sản xuất Kyocera',
            maKh: 'KH002',
            sdt: '0987654321',
            email: 'kyocera@example.com',
            diaChi: 'KCN Thăng Long II - Hưng Yên',
            trangThai: 'Đang hoạt động',
          },
        ]
      );
    } catch (err: any) {
      notifyError(err, 'Lỗi khi tải tệp mẫu Excel!');
    }
  };

  const handleUpload = async () => {
    if (fileList.length === 0) {
      notifyError('', 'Vui lòng chọn tệp Excel trước khi nhập!');
      return;
    }

    const selected = fileList[0];
    const file = ((selected as any)?.originFileObj || selected) as File;
    if (!file) {
      notifyError('', 'Tệp không hợp lệ, vui lòng chọn lại!');
      return;
    }

    try {
      setLoading(true);
      const fileToSend = await convertExcelFileToCsvFile(file);

      await importCsv(
        fileToSend,
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
            err?.data?.message || err?.message || 'Có lỗi xảy ra khi nhập dữ liệu từ Excel!';
          notifyError('', errorMsg);
        },
        setLoading
      );
    } catch (parseErr: any) {
      setLoading(false);
      notifyError(parseErr, 'Không thể đọc tệp Excel. Vui lòng kiểm tra định dạng tệp!');
    }
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
      title='Nhập dữ liệu khách hàng từ Excel'
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
          <Text type='secondary'>Hỗ trợ tệp định dạng .xlsx, .xls hoặc .csv (Ưu tiên Excel)</Text>
          <Button
            icon={<DownloadOutlined />}
            size='small'
            type='dashed'
            onClick={handleDownloadTemplate}
          >
            Tải tệp mẫu Excel (.xlsx)
          </Button>
        </div>

        {!result ? (
          <Dragger
            name='file'
            accept='.xlsx,.xls,.csv'
            maxCount={1}
            fileList={fileList}
            beforeUpload={(file) => {
              const fileName = file.name.toLowerCase();
              const isExcelOrCsv =
                fileName.endsWith('.xlsx') ||
                fileName.endsWith('.xls') ||
                fileName.endsWith('.csv') ||
                file.type === 'text/csv' ||
                file.type === 'application/vnd.ms-excel' ||
                file.type ===
                  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
              if (!isExcelOrCsv) {
                notifyError('', 'Chỉ chấp nhận tệp định dạng Excel (.xlsx, .xls) hoặc .csv!');
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
            <p className='ant-upload-text'>Kéo thả tệp Excel (.xlsx) vào đây hoặc bấm để chọn tệp</p>
            <p className='ant-upload-hint'>Hỗ trợ bảng tính Microsoft Excel (.xlsx, .xls) và .csv</p>
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
