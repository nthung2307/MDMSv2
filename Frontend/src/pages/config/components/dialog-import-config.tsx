import { DownloadOutlined, InboxOutlined } from '@ant-design/icons';
import { Alert, Button, Modal, Tag, Typography, Upload } from 'antd';
import type { UploadFile } from 'antd/es/upload/interface';
import { useState } from 'react';

import Constants from '@/core/application/common/constants';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { convertExcelFileToCsvFile, downloadExcelTemplate } from '@/infrastructure/utils/excelHelper';

const { Dragger } = Upload;
const { Text } = Typography;

interface Props {
  open: boolean;
  handleCancel: () => void;
  onSuccess: (updatedList: any[]) => void;
  currentListTagName: any[];
  pointCode: string;
}

interface ImportResult {
  totalRows: number;
  updatedCount: number;
  skippedCount: number;
}

const DialogImportConfig = ({
  open,
  handleCancel,
  onSuccess,
  currentListTagName,
  pointCode,
}: Props) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [result, setResult] = useState<ImportResult | null>(null);

  const handleClose = () => {
    setFileList([]);
    setResult(null);
    handleCancel();
  };

  const getGroupName = (type: string) => {
    switch (type) {
      case 'ELECTRICAL_PARAMETERS':
        return 'Thông số vận hành';
      case 'OPERATING_PARAMETERS':
        return 'Thông số điện năng';
      case 'WARNING':
        return 'Cảnh báo';
      case 'TIME':
      case 'DATETIME':
        return 'Thời gian';
      default:
        return 'Khác';
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      const sampleRows = Constants.dataMeasurementPoint.allDataParams.map((param: any, idx: number) => {
        const item = currentListTagName?.find((t: any) => t.description === param.value) || {};
        const groupName = getGroupName(param.type);
        const label = param.label || '';
        const code = param.value || '';
        const tagId = item.tagName || '';
        const unit = item.unit || (param.unit && param.unit.length > 0 ? param.unit[0].value : '');
        const customName = item.customName || '';

        return {
          stt: idx + 1,
          groupName,
          label,
          code,
          tagId,
          unit,
          customName,
        };
      });

      await downloadExcelTemplate(
        `Template_LienKetDuLieu_${pointCode || 'DiemDo'}.xlsx`,
        'LienKetDuLieu',
        [
          { header: 'STT', key: 'stt', width: 8 },
          { header: 'Nhóm thông số', key: 'groupName', width: 22 },
          { header: 'Tên thông số', key: 'label', width: 26 },
          { header: 'Mã tham số', key: 'code', width: 20 },
          { header: 'Tag ID', key: 'tagId', width: 26 },
          { header: 'Đơn vị', key: 'unit', width: 14 },
          { header: 'Tên cảnh báo', key: 'customName', width: 26 },
        ],
        sampleRows
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

    setLoading(true);
    try {
      const fileToSend = await convertExcelFileToCsvFile(file);
      const reader = new FileReader();
      reader.onload = (evt) => {
        try {
          const text = evt.target?.result as string;
          if (!text) {
            notifyError('', 'File rỗng!');
            setLoading(false);
            return;
          }

        const rawLines = text.split(/\r?\n/).filter((l) => l.trim().length > 0);
        if (rawLines.length <= 1) {
          notifyError('', 'File không có dữ liệu!');
          setLoading(false);
          return;
        }

        const delimiter = rawLines[0].includes(';') && !rawLines[0].includes(',') ? ';' : ',';

        const parseLine = (line: string) => {
          const resultArr: string[] = [];
          let current = '';
          let inQuotes = false;
          for (let i = 0; i < line.length; i++) {
            const char = line[i];
            if (char === '"') {
              if (inQuotes && line[i + 1] === '"') {
                current += '"';
                i++;
              } else {
                inQuotes = !inQuotes;
              }
            } else if (char === delimiter && !inQuotes) {
              resultArr.push(current.trim());
              current = '';
            } else {
              current += char;
            }
          }
          resultArr.push(current.trim());
          return resultArr;
        };

        const headers = parseLine(rawLines[0]).map((h) =>
          h
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]/g, '')
        );

        const codeIdx = headers.findIndex(
          (h) => h.includes('mathamso') || h.includes('code') || h.includes('description')
        );
        const tagIdx = headers.findIndex((h) => h.includes('tagid') || h.includes('tag'));
        const unitIdx = headers.findIndex((h) => h.includes('donvi') || h.includes('unit'));
        const customNameIdx = headers.findIndex(
          (h) =>
            h.includes('tencanhbao') || h.includes('customname') || h.includes('canhbao')
        );
        const labelIdx = headers.findIndex(
          (h) => h.includes('tenthamso') || h.includes('label') || h.includes('name')
        );

        let updatedCount = 0;
        let skippedCount = 0;
        const currentList = currentListTagName ? [...currentListTagName] : [];

        for (let i = 1; i < rawLines.length; i++) {
          const cols = parseLine(rawLines[i]);
          if (cols.length === 0) continue;

          const rowCode = codeIdx !== -1 ? cols[codeIdx]?.replace(/^"|"$/g, '').trim() : '';
          const rowLabel = labelIdx !== -1 ? cols[labelIdx]?.replace(/^"|"$/g, '').trim() : '';
          const rowTag = tagIdx !== -1 ? cols[tagIdx]?.replace(/^"|"$/g, '').trim() : '';
          const rowUnit = unitIdx !== -1 ? cols[unitIdx]?.replace(/^"|"$/g, '').trim() : '';
          const rowCustomName =
            customNameIdx !== -1 ? cols[customNameIdx]?.replace(/^"|"$/g, '').trim() : '';

          const itemIndex = currentList.findIndex((item) => {
            if (rowCode && item.description.toLowerCase() === rowCode.toLowerCase()) return true;
            if (rowLabel) {
              const param = Constants.dataMeasurementPoint.allDataParams.find(
                (p: any) => p.value === item.description
              );
              if (param && param.label.toLowerCase() === rowLabel.toLowerCase()) return true;
            }
            return false;
          });

          if (itemIndex !== -1) {
            const currentItem = currentList[itemIndex];
            const paramDef = Constants.dataMeasurementPoint.allDataParams.find(
              (p: any) => p.value === currentItem.description
            );

            let newUnit = currentItem.unit;
            if (paramDef && paramDef.unit && paramDef.unit.length === 1) {
              newUnit = paramDef.unit[0].value;
            } else if (paramDef && paramDef.unit && paramDef.unit.length > 1 && rowUnit) {
              const matched = paramDef.unit.find(
                (u: any) => u.value.toLowerCase() === rowUnit.toLowerCase()
              );
              if (matched) newUnit = matched.value;
            }

            currentList[itemIndex] = {
              ...currentItem,
              tagName: rowTag !== undefined ? rowTag : currentItem.tagName,
              unit: newUnit,
              customName: rowCustomName !== undefined ? rowCustomName : currentItem.customName,
            };
            updatedCount++;
          } else {
            skippedCount++;
          }
        }

        const resData: ImportResult = {
          totalRows: rawLines.length - 1,
          updatedCount,
          skippedCount,
        };
        setResult(resData);
        onSuccess(currentList);
        notifySuccess(
          '',
          `Đã nhập cấu hình từ file Excel (${updatedCount} tham số). Vui lòng nhấn "Lưu" để hoàn tất.`
        );
      } catch (err: any) {
        notifyError('', 'Lỗi khi đọc file: ' + (err.message || ''));
      } finally {
        setLoading(false);
      }
    };
    reader.onerror = () => {
      notifyError('', 'Lỗi khi đọc file!');
      setLoading(false);
    };
    reader.readAsText(fileToSend, 'utf-8');
    } catch (parseErr: any) {
      setLoading(false);
      notifyError(parseErr, 'Không thể đọc tệp Excel. Vui lòng kiểm tra định dạng tệp!');
    }
  };

  return (
    <Modal
      title='Nhập cấu hình liên kết dữ liệu từ Excel'
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
          message='Quy tắc Nhập cấu hình liên kết dữ liệu'
          description='Hệ thống đối chiếu theo Mã tham số hoặc Tên tham số để cập nhật Tag ID, Đơn vị và Tên cảnh báo cho điểm đo. Sau khi nhập thành công, vui lòng nhấn "Lưu" trên trang để lưu cấu hình.'
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
              <Tag color='blue'>Tổng số dòng: {result.totalRows}</Tag>
              <Tag color='green'>Đã cập nhật: {result.updatedCount}</Tag>
              {result.skippedCount > 0 && (
                <Tag color='orange'>Không khớp/Bỏ qua: {result.skippedCount}</Tag>
              )}
            </div>
            <Alert
              message='Dữ liệu đã được nạp lên bảng cài đặt'
              description='Vui lòng kiểm tra lại các giá trị trên bảng và nhấn nút "Lưu" để lưu vào cơ sở dữ liệu.'
              type='success'
              showIcon
            />
          </div>
        )}
      </div>
    </Modal>
  );
};

export default DialogImportConfig;
