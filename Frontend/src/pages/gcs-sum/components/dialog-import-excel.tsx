import { InboxOutlined, DownloadOutlined, CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { Modal, Upload, Button, Alert, Typography } from 'antd';
import type { UploadFile } from 'antd/es/upload/interface';
import { useState } from 'react';

import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useImportGcsTemporaryExcelHook, {
  ImportGcsTemporaryResult,
} from '@/infrastructure/repository/gcs/hooks/useImportGcsTemporaryExcel.hook';

const { Dragger } = Upload;
const { Text } = Typography;

interface Props {
  open: boolean;
  handleCancel: () => void;
  onSuccess: () => void;
  gcsParams: {
    gcsId: number;
    month: number;
    year: number;
    periodNumber: number;
  };
  onDownloadTemplate?: (onlySuspicious: boolean) => void;
}

const DialogImportExcel = ({
  open,
  handleCancel,
  onSuccess,
  gcsParams,
  onDownloadTemplate,
}: Props) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [result, setResult] = useState<ImportGcsTemporaryResult | null>(null);
  const [importExcel] = useImportGcsTemporaryExcelHook();

  const handleClose = () => {
    setFileList([]);
    setResult(null);
    handleCancel();
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
      await importExcel(
        file,
        gcsParams,
        (res) => {
          setResult(res);
          notifySuccess('', res?.message || 'Cập nhật chỉ số từ file Excel thành công!');
          onSuccess();
        },
        (err) => {
          notifyError(err, 'Lỗi khi nhập tệp Excel!');
        },
        setLoading
      );
    } catch (e: any) {
      notifyError(e, 'Có lỗi xảy ra trong quá trình xử lý tệp!');
    }
  };

  return (
    <Modal
      title='Nhập chỉ số & xác nhận từ Excel'
      open={open}
      onCancel={handleClose}
      width={680}
      footer={[
        <Button key='cancel' onClick={handleClose} disabled={loading}>
          {result ? 'Đóng' : 'Hủy'}
        </Button>,
        <Button
          key='submit'
          type='primary'
          onClick={handleUpload}
          loading={loading}
          disabled={fileList.length === 0 || loading}
          className='bg-blue-600'
        >
          Xác nhận nhập
        </Button>,
      ]}
    >
      <div className='space-y-4 py-2'>
        <Alert
          type='info'
          showIcon
          message='Quy trình hiệu chỉnh & xác nhận qua Excel:'
          description={
            <ol className='list-decimal pl-5 mt-1 space-y-1 text-xs text-gray-700'>
              <li>
                Bấm nút <b>"Tải bảng kê chỉ số Excel"</b> (hoặc nút <b>"Xuất file"</b> trên thanh công cụ) để tải toàn bộ dữ liệu kỳ chốt (mẫu chuẩn 5 cột tinh gọn).
              </li>
              <li>
                Mở tệp Excel để kiểm tra và hiệu chỉnh:
                <ul className='list-disc pl-4 mt-1 space-y-0.5 text-gray-600'>
                  <li>File gồm 5 cột: <b>Mã điểm đo</b>, <b>Tên khách hàng</b>, <b>Bộ chỉ số</b>, <b>Chỉ số cũ</b>, <b>Chỉ số mới</b>.</li>
                  <li>💡 <b>Mẹo lọc nhanh:</b> Dùng tính năng <b>Filter by Color (Lọc theo màu)</b> trên tiêu đề Excel để lọc các ô màu nền hồng nhạt (số liệu nghi ngờ / chưa cập nhật, giá trị vẫn là định dạng số chuẩn).</li>
                  <li>Để hiệu chỉnh/xác nhận: Sau khi sửa tay hoặc kiểm tra, người dùng chỉ cần <b>chuyển màu nền ô về không màu / màu trắng (Normal)</b> để xác nhận số liệu (hoặc nhập giá trị mới).</li>
                  <li>Hệ thống sẽ <b>tự động tính toán lại Sản lượng</b> và xác nhận số liệu sau khi hoàn tất nhập file.</li>
                </ul>
              </li>
              <li>Tải file đã chỉnh sửa lên đây và bấm <b>"Xác nhận nhập"</b> để cập nhật hàng loạt vào hệ thống.</li>
            </ol>
          }
        />

        {onDownloadTemplate && (
          <div className='flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200'>
            <div>
              <div className='font-semibold text-sm text-gray-800'>Tải bảng kê chỉ số:</div>
              <div className='text-xs text-gray-500'>Tải toàn bộ số liệu để xem, hiệu chỉnh chỉ số cũ/mới và xác nhận</div>
            </div>
            <div>
              <Button
                icon={<DownloadOutlined />}
                size='middle'
                type='primary'
                onClick={() => onDownloadTemplate(false)}
                className='bg-blue-600'
              >
                Tải bảng kê chỉ số Excel
              </Button>
            </div>
          </div>
        )}

        {!result && (
          <Dragger
            name='file'
            multiple={false}
            maxCount={1}
            fileList={fileList}
            showUploadList={false}
            accept='.xlsx,.xls'
            beforeUpload={(file) => {
              const isExcel =
                file.name.endsWith('.xlsx') ||
                file.name.endsWith('.xls') ||
                file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
                file.type === 'application/vnd.ms-excel';

              if (!isExcel) {
                notifyError('', 'Vui lòng chỉ chọn tệp Excel (.xlsx hoặc .xls)!');
                return Upload.LIST_IGNORE;
              }
              setFileList([file]);
              setResult(null);
              return false;
            }}
          >
            <p className='ant-upload-drag-icon text-blue-500 text-3xl mb-2'>
              <InboxOutlined />
            </p>
            <p className='ant-upload-text font-semibold text-gray-700'>
              Kéo thả hoặc nhấp chuột vào đây để chọn tệp Excel (.xlsx)
            </p>
            <p className='ant-upload-hint text-xs text-gray-500'>
              Hệ thống sẽ tự động đối soát theo ID bản ghi hoặc Mã điểm đo & Bộ chỉ số.
            </p>
          </Dragger>
        )}

        {fileList.length > 0 && !result && (
          <div className='flex items-center justify-between p-3 bg-blue-50 border border-blue-200 rounded-lg text-sm'>
            <div className='flex items-center gap-2 truncate text-blue-900 font-medium'>
              <span className='text-base'>📊</span>
              <span className='truncate'>{fileList[0].name}</span>
            </div>
            <Button
              type='link'
              size='small'
              danger
              onClick={() => {
                setFileList([]);
                setResult(null);
              }}
            >
              Xóa tệp
            </Button>
          </div>
        )}

        {result && (
          <div className='mt-4 p-4 rounded-lg bg-green-50 border border-green-200 space-y-2'>
            <div className='flex items-center gap-2 text-green-700 font-semibold'>
              <CheckCircleOutlined />
              <span>{result.message}</span>
            </div>
            <div className='text-xs text-gray-600 flex gap-4'>
              <span>Tổng dòng: <b>{result.totalRows}</b></span>
              <span className='text-green-600'>Cập nhật thành công: <b>{result.updatedCount}</b></span>
              <span className='text-gray-500'>Bỏ qua / Giữ nguyên: <b>{result.skippedCount}</b></span>
            </div>
            {result.errors && result.errors.length > 0 && (
              <div className='mt-2 p-2 bg-amber-50 rounded border border-amber-200 max-h-32 overflow-y-auto'>
                <div className='text-xs font-semibold text-amber-800 flex items-center gap-1 mb-1'>
                  <ExclamationCircleOutlined />
                  <span>Một số cảnh báo trong file:</span>
                </div>
                <ul className='list-disc pl-4 text-xs text-amber-700 space-y-0.5'>
                  {result.errors.map((err, idx) => (
                    <li key={idx}>{err}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};

export default DialogImportExcel;
