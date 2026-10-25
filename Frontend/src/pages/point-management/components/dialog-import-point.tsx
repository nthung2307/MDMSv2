import { DownloadOutlined, InboxOutlined } from '@ant-design/icons';
import { Alert, Button, Modal, Table, Tag, Typography, Upload } from 'antd';
import type { UploadFile } from 'antd/es/upload/interface';
import { useState } from 'react';

import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useImportMeasurementPointCsvHook from '@/infrastructure/repository/measurement-point/hooks/useImportMeasurementPointCsv.hook';
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
    pointCode?: string;
    errorMessage: string;
  }>;
}

const DialogImportPoint = ({ open, handleCancel, onSuccess }: Props) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [importCsv] = useImportMeasurementPointCsvHook();

  const handleClose = () => {
    setFileList([]);
    setResult(null);
    handleCancel();
  };

  const handleDownloadTemplate = async () => {
    try {
      await downloadExcelTemplate(
        'Template_DiemDo.xlsx',
        'DiemDo',
        [
          { header: 'STT', key: 'stt', width: 8 },
          { header: 'Mã điểm đo', key: 'maDiemDo', width: 16 },
          { header: 'Loại điểm đo', key: 'loaiDiemDo', width: 16 },
          { header: 'Mô tả', key: 'moTa', width: 32 },
          { header: 'Tên khách hàng', key: 'tenKh', width: 32 },
          { header: 'Email nhận phiếu báo (tối đa 3 email)', key: 'email', width: 36 },
          { header: 'Trạng thái', key: 'trangThai', width: 16 },
        ],
        [
          {
            stt: 1,
            maDiemDo: 'ĐĐ001',
            loaiDiemDo: '3 Pha',
            moTa: 'Điểm đo trạm biến áp A',
            tenKh: 'Công ty Điện lực Mẫu A',
            email: 'email1@example.com; email2@example.com',
            trangThai: 'Đang sử dụng',
          },
          {
            stt: 2,
            maDiemDo: 'ĐĐ002',
            loaiDiemDo: '1 Pha',
            moTa: 'Điểm đo phân xưởng 1',
            tenKh: 'Nhà máy Sản xuất Kyocera',
            email: 'kyocera_amr@example.com',
            trangThai: 'Đang sử dụng',
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
      title: 'Mã điểm đo',
      dataIndex: 'pointCode',
      key: 'pointCode',
      width: 140,
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
      title='Nhập dữ liệu điểm đo từ Excel'
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
          description='Hệ thống đối chiếu theo Mã điểm đo. Điểm đo đã tồn tại sẽ được cập nhật thông tin và email nhận phiếu báo (bảo toàn 100% ID và dữ liệu đo đếm lịch sử). Điểm đo mới sẽ được thêm vào hệ thống và liên kết với Khách hàng tương ứng.'
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

export default DialogImportPoint;
