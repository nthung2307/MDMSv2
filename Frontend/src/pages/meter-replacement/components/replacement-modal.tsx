import React, { useEffect, useState } from 'react';
import {
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  DatePicker,
  Row,
  Col,
  Card,
  Tag,
  Upload,
  Button,
  Spin,
  Alert,
} from 'antd';
import dayjs from 'dayjs';
import {
  SafetyCertificateOutlined,
  SaveOutlined,
  CheckCircleOutlined,
  UploadOutlined,
  FilePdfOutlined,
  FileImageOutlined,
  EyeOutlined,
  DeleteOutlined,
  ExportOutlined,
} from '@ant-design/icons';
import useMeterReplacementHook from '@/infrastructure/repository/meter-replacement/hooks/useMeterReplacementHook.tsx';
import useGetListMeasurementPointsHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPoints.hook.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import Constants from '@/core/application/common/constants.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const { Option } = Select;

interface ReplacementModalProps {
  visible: boolean;
  mode: 'create' | 'edit' | 'view';
  initialData?: any;
  defaultPointId?: number;
  onClose: () => void;
  onSuccess: () => void;
}

const COMMON_REASONS = [
  'Kiểm định định kỳ theo quy định',
  'Hư hỏng màn hình LCD công tơ',
  'Mất tín hiệu truyền thông RS485/Optical',
  'Thay đổi tỷ số biến dòng (TI/TU) - Nâng công suất',
  'Cháy nổ / Sự cố lưới điện tại trạm',
  'Khác',
];

export const ReplacementModal: React.FC<ReplacementModalProps> = ({
  visible,
  mode,
  initialData,
  defaultPointId,
  onClose,
  onSuccess,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState<boolean>(false);
  const [pointsList, setPointsList] = useState<any[]>([]);
  const [selectedPointInfo, setSelectedPointInfo] = useState<any>(null);
  const [documentUrl, setDocumentUrl] = useState<string>('');
  const [uploadingDoc, setUploadingDoc] = useState<boolean>(false);
  const [previewDocVisible, setPreviewDocVisible] = useState<boolean>(false);
  const [pdfBlobUrl, setPdfBlobUrl] = useState<string>('');
  const [loadingPdf, setLoadingPdf] = useState<boolean>(false);
  const [pdfError, setPdfError] = useState<string>('');

  const { createReplacement, updateReplacement, getPointMeterInfo, uploadDocument } = useMeterReplacementHook();
  const [getListPoints] = useGetListMeasurementPointsHook();

  // Load danh sách điểm đo
  useEffect(() => {
    if (visible) {
      getListPoints(
        {
          pageSize: 500,
          searchByFields: [{ searchFieldName: 'status', searchValue: Constants.AccountStatus.Active }],
          sortByFields: [],
        },
        (res: any) => {
          if (res?.data?.data) {
            setPointsList(res.data.data);
          }
        },
        () => {},
        setLoading
      );
    }
  }, [visible]);

  // Khởi tạo dữ liệu form
  useEffect(() => {
    if (visible) {
      form.resetFields();
      setSelectedPointInfo(null);
      if (mode === 'edit' || mode === 'view') {
        if (initialData) {
          form.setFieldsValue({
            ...initialData,
            oldBt: Math.round(Number(initialData.oldBt || 0)),
            oldCd: Math.round(Number(initialData.oldCd || 0)),
            oldTd: Math.round(Number(initialData.oldTd || 0)),
            oldSg: Math.round(Number(initialData.oldSg || 0)),
            oldVc: Math.round(Number(initialData.oldVc || 0)),
            oldPmax: Number((initialData.oldPmax || 0).toFixed(2)),
            newBt: Math.round(Number(initialData.newBt || 0)),
            newCd: Math.round(Number(initialData.newCd || 0)),
            newTd: Math.round(Number(initialData.newTd || 0)),
            newSg: Math.round(Number(initialData.newSg || 0)),
            newVc: Math.round(Number(initialData.newVc || 0)),
            newPmax: Number((initialData.newPmax || 0).toFixed(2)),
            replacementDate: initialData.replacementDate ? dayjs(initialData.replacementDate) : dayjs(),
          });
          setSelectedPointInfo({
            pointCode: initialData.measurementPointCode,
            pointName: initialData.measurementPointDescription,
            customerName: initialData.customerName,
            customerCode: initialData.customerCode,
          });
          setDocumentUrl(initialData.documentUrl || '');
        }
      } else {
        // Mode create
        const pointId = defaultPointId || (initialData?.measurementPointId);
        form.setFieldsValue({
          measurementPointId: pointId || undefined,
          replacementDate: dayjs(),
          reason: COMMON_REASONS[0],
          oldMeterSerial: '',
          oldMeterMultiplier: 1.0,
          newMeterMultiplier: 1.0,
          oldBt: 0,
          oldCd: 0,
          oldTd: 0,
          oldSg: 0,
          oldVc: 0,
          oldPmax: 0.00,
          newBt: 0,
          newCd: 0,
          newTd: 0,
          newSg: 0,
          newVc: 0,
          newPmax: 0.00,
          status: 'APPROVED',
          documentUrl: '',
        });
        setDocumentUrl('');

        if (pointId) {
          handleSelectPoint(pointId);
        } else {
          setSelectedPointInfo(null);
        }
      }
    } else {
      setSelectedPointInfo(null);
      setDocumentUrl('');
    }
  }, [visible, mode, initialData, defaultPointId]);

  const handleUploadFile = (file: File) => {
    const isAllowed = file.type === 'application/pdf' || file.type.startsWith('image/');
    if (!isAllowed) {
      notifyError('', 'Vui lòng chọn tệp định dạng PDF hoặc hình ảnh (JPG, PNG).');
      return false;
    }
    if (file.size > 20 * 1024 * 1024) {
      notifyError('', 'Dung lượng tệp không được vượt quá 20MB.');
      return false;
    }

    uploadDocument(
      file,
      (uploadedUrl: string) => {
        if (uploadedUrl) {
          setDocumentUrl(uploadedUrl);
          form.setFieldsValue({ documentUrl: uploadedUrl });
          notifySuccess('', 'Tải lên biên bản hiện trường thành công!');
        }
      },
      (errorMsg?: string) => {
        notifyError('', errorMsg || 'Tải lên biên bản hiện trường thất bại.');
      },
      setUploadingDoc
    );

    return false;
  };

  const getFullDocumentUrl = (url: any) => {
    if (!url || typeof url !== 'string') return '';
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    const baseUrl = import.meta.env.VITE_APP_API_URL || '';
    return `${baseUrl.replace(/\/+$/, '')}/${url.replace(/^\/+/, '')}`;
  };

  const isPdfDocument = (url: any) => {
    if (!url || typeof url !== 'string') return false;
    return url.toLowerCase().includes('.pdf');
  };

  useEffect(() => {
    let active = true;
    let currentBlobUrl = '';

    if (previewDocVisible && documentUrl && isPdfDocument(documentUrl)) {
      const fullUrl = getFullDocumentUrl(documentUrl);
      setLoadingPdf(true);
      setPdfError('');
      setPdfBlobUrl('');

      fetch(fullUrl, {
        method: 'GET',
        headers: {
          Accept: 'application/pdf, application/octet-stream, */*',
        },
      })
        .then(async (res) => {
          if (!res.ok) {
            throw new Error(`Không thể tải tệp PDF (HTTP ${res.status})`);
          }
          const contentType = res.headers.get('content-type') || '';
          if (contentType.includes('text/html')) {
            throw new Error('Máy chủ trả về trang HTML thay vì tệp PDF. Vui lòng cập nhật lại gói cài đặt hoặc kiểm tra tệp trên máy chủ.');
          }
          const buf = await res.arrayBuffer();
          const pdfBlob = new Blob([buf], { type: 'application/pdf' });
          currentBlobUrl = URL.createObjectURL(pdfBlob);
          if (active) {
            setPdfBlobUrl(currentBlobUrl);
          }
        })
        .catch((err: any) => {
          if (active) {
            setPdfError(err?.message || 'Lỗi tải tài liệu PDF.');
          }
        })
        .finally(() => {
          if (active) setLoadingPdf(false);
        });
    } else {
      setPdfBlobUrl('');
      setPdfError('');
      setLoadingPdf(false);
    }

    return () => {
      active = false;
      if (currentBlobUrl) {
        URL.revokeObjectURL(currentBlobUrl);
      }
    };
  }, [previewDocVisible, documentUrl]);

  const handleSelectPoint = (pointId: number) => {
    if (!pointId) {
      setSelectedPointInfo(null);
      form.setFieldsValue({
        oldMeterSerial: '',
        oldMeterMultiplier: 1.0,
        newMeterMultiplier: 1.0,
        oldBt: 0,
        oldCd: 0,
        oldTd: 0,
        oldSg: 0,
        oldVc: 0,
        oldPmax: 0.00,
      });
      return;
    }

    getPointMeterInfo(
      pointId,
      (res: any) => {
        const info = res?.data;
        if (info) {
          setSelectedPointInfo(info);
          form.setFieldsValue({
            oldMeterSerial: info.currentSerial || '',
            oldMeterMultiplier: info.currentMultiplier || 1.0,
            newMeterMultiplier: info.currentMultiplier || 1.0,
            oldBt: Math.round(Number(info.latestBt || 0)),
            oldCd: Math.round(Number(info.latestCd || 0)),
            oldTd: Math.round(Number(info.latestTd || 0)),
            oldSg: Math.round(Number(info.latestSg || 0)),
            oldVc: Math.round(Number(info.latestVc || 0)),
            oldPmax: Number((info.latestPmax || 0).toFixed(2)),
          });
        }
      },
      () => {},
      setLoading
    );
  };

  const handleSubmit = async (targetStatus?: 'DRAFT' | 'APPROVED') => {
    try {
      const values = await form.validateFields();
      const statusToSave = targetStatus || (mode === 'create' ? 'APPROVED' : 'DRAFT');

      const payload = {
        ...values,
        replacementDate: values.replacementDate ? values.replacementDate.toISOString() : new Date().toISOString(),
        oldMeterMultiplier: Number(values.oldMeterMultiplier || 1),
        newMeterMultiplier: Number(values.newMeterMultiplier || 1),
        oldBt: Math.round(Number(values.oldBt || 0)),
        oldCd: Math.round(Number(values.oldCd || 0)),
        oldTd: Math.round(Number(values.oldTd || 0)),
        oldSg: Math.round(Number(values.oldSg || 0)),
        oldVc: Math.round(Number(values.oldVc || 0)),
        oldPmax: Number(Number(values.oldPmax || 0).toFixed(2)),
        newBt: Math.round(Number(values.newBt || 0)),
        newCd: Math.round(Number(values.newCd || 0)),
        newTd: Math.round(Number(values.newTd || 0)),
        newSg: Math.round(Number(values.newSg || 0)),
        newVc: Math.round(Number(values.newVc || 0)),
        newPmax: Number(Number(values.newPmax || 0).toFixed(2)),
        status: statusToSave,
      };

      if (mode === 'create') {
        createReplacement(
          payload,
          () => {
            notifySuccess('', statusToSave === 'APPROVED' ? 'Đã lập và phê duyệt biên bản treo tháo thành công!' : 'Đã lưu bản nháp biên bản treo tháo!');
            onSuccess();
            onClose();
          },
          () => {
            notifyError('', 'Không thể lưu biên bản treo tháo.');
          },
          setLoading
        );
      } else if (mode === 'edit') {
        updateReplacement(
          initialData.id,
          payload,
          () => {
            notifySuccess('', statusToSave === 'APPROVED' ? 'Đã phê duyệt biên bản treo tháo thành công!' : 'Đã lưu thay đổi biên bản treo tháo!');
            onSuccess();
            onClose();
          },
          () => {
            notifyError('', 'Không thể cập nhật biên bản.');
          },
          setLoading
        );
      }
    } catch (err) {
      // Validate failed
    }
  };

  const isView = mode === 'view';

  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <SafetyCertificateOutlined style={{ color: '#b22222', fontSize: 20 }} />
          <span style={{ fontSize: 17, fontWeight: 700, color: '#0f172a' }}>
            {mode === 'create'
              ? 'Lập biên bản treo tháo'
              : mode === 'edit'
              ? `Chỉnh sửa biên bản treo tháo #${initialData?.id || ''}`
              : `Chi tiết biên bản treo tháo #${initialData?.id || ''}`}
          </span>
          {initialData?.status && (
            <Tag
              color={
                initialData.status === 'APPROVED' ? 'success' : initialData.status === 'DRAFT' ? 'warning' : 'default'
              }
              className="ml-2 font-semibold"
            >
              {initialData.status === 'APPROVED' ? 'Đã phê duyệt' : initialData.status === 'DRAFT' ? 'Bản nháp' : 'Đã hủy'}
            </Tag>
          )}
        </div>
      }
      open={visible}
      onCancel={onClose}
      width={940}
      destroyOnClose
      footer={
        isView ? null : (
          <div className="flex justify-end gap-3 pt-2">
            <ButtonCommon
              title="Lưu"
              colorClass="firebrick-outline-bg"
              icon={<SaveOutlined />}
              onClick={() => handleSubmit('DRAFT')}
              loading={loading}
            />
            <ButtonCommon
              title="Phê duyệt"
              colorClass="firebrick-bg"
              icon={<CheckCircleOutlined />}
              onClick={() => handleSubmit('APPROVED')}
              loading={loading}
            />
          </div>
        )
      }
    >
      <Form form={form} layout="vertical" disabled={isView} initialValues={{ oldMeterMultiplier: 1, newMeterMultiplier: 1 }}>
        {/* 1. THÔNG TIN CHUNG */}
        <Card size="small" className="mb-4 bg-slate-50 border-slate-200">
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label={<span className="font-semibold text-slate-700">Mã điểm đo *</span>}
                name="measurementPointId"
                rules={[{ required: true, message: 'Vui lòng chọn mã điểm đo' }]}
              >
                <Select
                  placeholder="-- Chọn mã điểm đo --"
                  showSearch
                  allowClear
                  optionFilterProp="children"
                  onChange={handleSelectPoint}
                  disabled={mode !== 'create'}
                >
                  {pointsList.map((p) => (
                    <Option key={p.id} value={p.id}>
                      {p.code}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>

            <Col span={12}>
              <Form.Item
                label={<span className="font-semibold text-slate-700">Thời điểm treo tháo *</span>}
                name="replacementDate"
                rules={[{ required: true, message: 'Vui lòng chọn thời điểm treo tháo' }]}
              >
                <DatePicker
                  showTime
                  format="DD/MM/YYYY HH:mm"
                  className="w-full"
                  placeholder="Chọn thời điểm treo tháo"
                />
              </Form.Item>
            </Col>
          </Row>

          {selectedPointInfo && (
            <div className="bg-white p-3 rounded border border-slate-200 text-xs text-slate-700 space-y-2 mb-3">
              <div className="flex items-center">
                <span className="w-32 flex-shrink-0 font-semibold text-slate-700 text-left">Mã điểm đo:</span>
                <span className="text-blue-700 font-bold text-left">{selectedPointInfo.pointCode}</span>
              </div>
              <div className="flex items-center">
                <span className="w-32 flex-shrink-0 font-semibold text-slate-700 text-left">Tên khách hàng:</span>
                <span className="font-medium text-slate-800 text-left">{selectedPointInfo.customerName || '---'}</span>
              </div>
            </div>
          )}

          <Row gutter={16}>
            <Col span={24}>
              <Form.Item
                label={<span className="font-semibold text-slate-700">Lý do treo tháo *</span>}
                name="reason"
                rules={[{ required: true, message: 'Vui lòng chọn hoặc nhập lý do treo tháo' }]}
              >
                <Select
                  placeholder="Chọn hoặc nhập lý do treo tháo"
                  showSearch
                  optionFilterProp="children"
                  allowClear
                >
                  {COMMON_REASONS.map((r, i) => (
                    <Option key={i} value={r}>
                      {r}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* 2. BỘ CHỈ SỐ CÔNG TƠ THÁO & CÔNG TƠ TREO (MỖI CHỈ SỐ TRÊN 1 HÀNG GỒM NHÃN VÀ TEXTBOX) */}
        <Row gutter={16}>
          {/* CỘT TRÁI: CÔNG TƠ THÁO */}
          <Col span={12}>
            <Card
              size="small"
              title={<span className="text-amber-800 font-bold text-sm">Công tơ tháo</span>}
              className="border-amber-200 bg-amber-50/30"
            >
              <div className="space-y-2.5">
                {/* 1. Số chế tạo */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-slate-700">
                    Số chế tạo <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldMeterSerial" rules={[{ required: true, message: 'Nhập số chế tạo' }]} noStyle>
                      <Input placeholder="VD: 23098172" className="font-mono font-semibold" />
                    </Form.Item>
                  </div>
                </div>

                {/* 2. Hệ số nhân (HSN) */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-slate-700">
                    Hệ số nhân (HSN) <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldMeterMultiplier" rules={[{ required: true, message: 'Nhập HSN' }]} noStyle>
                      <InputNumber min={0.0001} step={1} className="w-full font-bold" />
                    </Form.Item>
                  </div>
                </div>

                {/* 3. Bình thường (BT) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Bình thường (BT)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldBt" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 4. Cao điểm (CD) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Cao điểm (CD)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldCd" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 5. Thấp điểm (TD) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Thấp điểm (TD)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldTd" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 6. Tổng điện năng hữu công (SG) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-amber-900">
                    Tổng điện năng hữu công (SG) <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldSg" rules={[{ required: true, message: 'Nhập tổng SG' }]} noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full font-bold text-amber-900" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 7. Tổng điện năng phản kháng (VC) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Tổng điện năng phản kháng (VC)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldVc" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 8. Công suất cực đại (Pmax) - cấp chính xác đến % (2 chữ số thập phân) */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Công suất cực đại (Pmax)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="oldPmax" noStyle>
                      <InputNumber min={0} step={0.01} precision={2} className="w-full" placeholder="0.00" />
                    </Form.Item>
                  </div>
                </div>
              </div>
            </Card>
          </Col>

          {/* CỘT PHẢI: CÔNG TƠ TREO */}
          <Col span={12}>
            <Card
              size="small"
              title={<span className="text-emerald-800 font-bold text-sm">Công tơ treo</span>}
              className="border-emerald-200 bg-emerald-50/30"
            >
              <div className="space-y-2.5">
                {/* 1. Số chế tạo */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-slate-700">
                    Số chế tạo <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newMeterSerial" rules={[{ required: true, message: 'Nhập số chế tạo mới' }]} noStyle>
                      <Input placeholder="VD: 24110099" className="font-mono font-semibold text-emerald-800" />
                    </Form.Item>
                  </div>
                </div>

                {/* 2. Hệ số nhân (HSN) */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-slate-700">
                    Hệ số nhân (HSN) <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newMeterMultiplier" rules={[{ required: true, message: 'Nhập HSN mới' }]} noStyle>
                      <InputNumber min={0.0001} step={1} className="w-full font-bold text-emerald-800" />
                    </Form.Item>
                  </div>
                </div>

                {/* 3. Bình thường (BT) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Bình thường (BT)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newBt" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 4. Cao điểm (CD) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Cao điểm (CD)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newCd" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 5. Thấp điểm (TD) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Thấp điểm (TD)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newTd" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 6. Tổng điện năng hữu công (SG) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-semibold text-emerald-900">
                    Tổng điện năng hữu công (SG) <span className="text-red-500">*</span>
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newSg" rules={[{ required: true, message: 'Nhập tổng SG mới' }]} noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full font-bold text-emerald-900" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 7. Tổng điện năng phản kháng (VC) - cấp chính xác hàng đơn vị */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Tổng điện năng phản kháng (VC)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newVc" noStyle>
                      <InputNumber min={0} step={1} precision={0} className="w-full" placeholder="0" />
                    </Form.Item>
                  </div>
                </div>

                {/* 8. Công suất cực đại (Pmax) - cấp chính xác đến % (2 chữ số thập phân) */}
                <div className="grid grid-cols-12 items-center gap-2">
                  <label className="col-span-5 text-xs font-medium text-slate-700">
                    Công suất cực đại (Pmax)
                  </label>
                  <div className="col-span-7">
                    <Form.Item name="newPmax" noStyle>
                      <InputNumber min={0} step={0.01} precision={2} className="w-full" placeholder="0.00" />
                    </Form.Item>
                  </div>
                </div>
              </div>
            </Card>
          </Col>
        </Row>

        {/* 3. BIÊN BẢN HIỆN TRƯỜNG */}
        <div className="mt-4 p-3 bg-slate-50 rounded-lg border border-slate-200">
          <div className="flex items-center justify-between mb-2">
            <span className="font-semibold text-slate-700 text-xs flex items-center gap-1.5">
              <FilePdfOutlined className="text-red-600" />
              Biên bản hiện trường
            </span>
          </div>

          <Form.Item name="documentUrl" hidden>
            <Input />
          </Form.Item>

          {documentUrl ? (
            <div className="flex items-center justify-between p-2.5 bg-white rounded border border-slate-200 shadow-xs">
              <div className="flex items-center gap-2.5 overflow-hidden">
                {isPdfDocument(documentUrl) ? (
                  <FilePdfOutlined className="text-red-500 text-2xl flex-shrink-0" />
                ) : (
                  <FileImageOutlined className="text-blue-500 text-2xl flex-shrink-0" />
                )}
                <div className="truncate">
                  <div className="text-xs font-semibold text-slate-800 truncate">
                    {typeof documentUrl === 'string' ? (documentUrl.split('/').pop() || 'bien-ban-hien-truong') : 'bien-ban-hien-truong'}
                  </div>
                  <div className="text-[11px] text-slate-500">
                    {isPdfDocument(documentUrl) ? 'Tài liệu PDF' : 'Tệp hình ảnh scan'}
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2 flex-shrink-0">
                <Button
                  size="small"
                  type="primary"
                  ghost
                  icon={<EyeOutlined />}
                  onClick={() => setPreviewDocVisible(true)}
                >
                  Xem biên bản
                </Button>

                {!isView && (
                  <Button
                    size="small"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => {
                      setDocumentUrl('');
                      form.setFieldsValue({ documentUrl: '' });
                    }}
                  >
                    Gỡ bỏ
                  </Button>
                )}
              </div>
            </div>
          ) : isView ? (
            <div className="text-xs text-slate-400 italic py-2">
              Chưa có tệp scan hoặc ảnh biên bản hiện trường đính kèm.
            </div>
          ) : (
            <div>
              <Upload.Dragger
                name="file"
                multiple={false}
                showUploadList={false}
                beforeUpload={handleUploadFile}
                accept=".pdf,image/png,image/jpeg,image/jpg"
                disabled={uploadingDoc}
                className="bg-white"
              >
                <p className="ant-upload-drag-icon text-slate-400 mb-1">
                  <UploadOutlined style={{ fontSize: 24, color: '#3b82f6' }} />
                </p>
                <p className="text-xs font-semibold text-slate-700 m-0">
                  {uploadingDoc ? 'Đang tải tệp lên máy chủ...' : 'Nhấp hoặc kéo thả tệp PDF, ảnh chụp / scan biên bản vào đây'}
                </p>
                <p className="text-[11px] text-slate-400 m-0 mt-0.5">
                  Hỗ trợ định dạng PDF, JPG, PNG (tối đa 20MB)
                </p>
              </Upload.Dragger>
            </div>
          )}
        </div>

        {/* Modal xem trước biên bản hiện trường (Hỗ trợ cả PDF và Hình ảnh) */}
        {previewDocVisible && (
          <Modal
            open={previewDocVisible}
            footer={
              <div className="flex justify-between items-center w-full">
                <span className="text-xs text-slate-500 font-mono truncate max-w-[400px]">
                  {typeof documentUrl === 'string' ? documentUrl.split('/').pop() : ''}
                </span>
                <div className="flex gap-2">
                  <Button
                    icon={<ExportOutlined />}
                    onClick={() => window.open(isPdfDocument(documentUrl) && pdfBlobUrl ? pdfBlobUrl : getFullDocumentUrl(documentUrl), '_blank')}
                  >
                    Mở tab mới
                  </Button>
                  <Button type="primary" onClick={() => setPreviewDocVisible(false)}>
                    Đóng
                  </Button>
                </div>
              </div>
            }
            onCancel={() => setPreviewDocVisible(false)}
            width={920}
            title="Xem biên bản hiện trường"
            centered
          >
            <div className="flex justify-center items-center p-2 bg-slate-100 rounded min-h-[450px]">
              {isPdfDocument(documentUrl) ? (
                loadingPdf ? (
                  <div className="text-center py-12">
                    <Spin size="large" />
                    <div className="mt-3 text-xs text-slate-500 font-medium">Đang tải dữ liệu tệp PDF...</div>
                  </div>
                ) : pdfError ? (
                  <Alert
                    type="error"
                    message="Không thể hiển thị tài liệu PDF"
                    description={pdfError}
                    className="max-w-lg"
                    showIcon
                  />
                ) : pdfBlobUrl ? (
                  <iframe
                    src={pdfBlobUrl}
                    title="Biên bản hiện trường PDF"
                    className="w-full h-[75vh] border-0 rounded bg-white shadow-sm"
                  />
                ) : null
              ) : (
                <img
                  src={getFullDocumentUrl(documentUrl)}
                  alt="Biên bản hiện trường"
                  className="max-h-[75vh] max-w-full object-contain rounded shadow-sm"
                />
              )}
            </div>
          </Modal>
        )}
      </Form>
    </Modal>
  );
};
