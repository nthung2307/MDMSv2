import React, { useEffect, useRef, useState } from 'react';
import {
  Modal,
  Button,
  Input,
  Spin,
  Alert,
  Card,
  Row,
  Col,
  Tag,
  Tooltip,
  Upload,
} from 'antd';
import {
  RobotOutlined,
  ScissorOutlined,
  ReloadOutlined,
  KeyOutlined,
  CheckCircleOutlined,
  UploadOutlined,
  EyeOutlined,
  InfoCircleOutlined,
  FullscreenOutlined,
} from '@ant-design/icons';
import useMeterReplacementHook from '@/infrastructure/repository/meter-replacement/hooks/useMeterReplacementHook.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { callGeminiDirectlyFromBrowser } from '@/pages/meter-replacement/components/gemini-ocr-helper.ts';

interface OcrCanvasModalProps {
  visible: boolean;
  documentUrl?: string;
  onClose: () => void;
  onApply: (data: any) => void;
}

export const OcrCanvasModal: React.FC<OcrCanvasModalProps> = ({
  visible,
  documentUrl,
  onClose,
  onApply,
}) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);

  const [loading, setLoading] = useState<boolean>(false);
  const [apiKey, setApiKey] = useState<string>(() => localStorage.getItem('MDMS_GEMINI_API_KEY') || '');
  const [showApiKeyInput, setShowApiKeyInput] = useState<boolean>(false);

  const [imageSrc, setImageSrc] = useState<string>('');
  const [isPdf, setIsPdf] = useState<boolean>(false);
  const [rotation, setRotation] = useState<number>(0);

  // Bounding box tỷ lệ (0 đến 1)
  const [cropBox, setCropBox] = useState<{ x: number; y: number; width: number; height: number }>({
    x: 0.05,
    y: 0.35, // Mặc định bỏ 35% trên (phần thông tin khách hàng / tiêu đề)
    width: 0.9,
    height: 0.6,
  });

  const [isDragging, setIsDragging] = useState<boolean>(false);
  const [dragMode, setDragMode] = useState<'move' | 'nw' | 'ne' | 'se' | 'sw' | null>(null);
  const [dragStart, setDragStart] = useState<{ x: number; y: number; box: any }>({ x: 0, y: 0, box: cropBox });

  const [extractedData, setExtractedData] = useState<any>(null);

  const { extractOcr } = useMeterReplacementHook();

  const getFullUrl = (url: string) => {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    const baseUrl = import.meta.env.VITE_APP_API_URL || '';
    return `${baseUrl.replace(/\/+$/, '')}/${url.replace(/^\/+/, '')}`;
  };

  useEffect(() => {
    if (visible) {
      setExtractedData(null);
      setRotation(0);
      if (documentUrl) {
        const isPdfFile = documentUrl.toLowerCase().includes('.pdf');
        setIsPdf(isPdfFile);
        if (!isPdfFile) {
          setImageSrc(getFullUrl(documentUrl));
        } else {
          setImageSrc('');
        }
      } else {
        setIsPdf(false);
        setImageSrc('');
      }
    }
  }, [visible, documentUrl]);

  // Vẽ hình ảnh và khung Crop lên Canvas
  useEffect(() => {
    if (!imageSrc || isPdf || !visible) return;

    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => {
      const containerWidth = containerRef.current?.clientWidth || 700;
      const scale = Math.min(1, containerWidth / img.width);

      canvas.width = img.width * scale;
      canvas.height = img.height * scale;

      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.save();

      // Vẽ hình ảnh đã scale
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

      // Lớp phủ mờ bảo vệ PII bên ngoài khung chọn
      ctx.fillStyle = 'rgba(15, 23, 42, 0.45)';
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      // Đục lỗ vùng chọn (vùng giữ lại)
      const bx = cropBox.x * canvas.width;
      const by = cropBox.y * canvas.height;
      const bw = cropBox.width * canvas.width;
      const bh = cropBox.height * canvas.height;

      ctx.clearRect(bx, by, bw, bh);
      ctx.drawImage(img, cropBox.x * img.width, cropBox.y * img.height, cropBox.width * img.width, cropBox.height * img.height, bx, by, bw, bh);

      // Khung viền vùng chọn
      ctx.strokeStyle = '#3b82f6';
      ctx.lineWidth = 2.5;
      ctx.setLineDash([6, 4]);
      ctx.strokeRect(bx, by, bw, bh);

      // Điểm mút điều chỉnh ở 4 góc
      ctx.setLineDash([]);
      ctx.fillStyle = '#ffffff';
      ctx.strokeStyle = '#2563eb';
      ctx.lineWidth = 2;
      const handleSize = 8;
      const corners = [
        [bx, by],
        [bx + bw, by],
        [bx + bw, by + bh],
        [bx, by + bh],
      ];
      corners.forEach(([cx, cy]) => {
        ctx.fillRect(cx - handleSize / 2, cy - handleSize / 2, handleSize, handleSize);
        ctx.strokeRect(cx - handleSize / 2, cy - handleSize / 2, handleSize, handleSize);
      });

      // Nhãn chỉ dẫn
      ctx.font = '11px sans-serif';
      ctx.fillStyle = '#2563eb';
      ctx.fillRect(bx, Math.max(0, by - 20), 180, 20);
      ctx.fillStyle = '#ffffff';
      ctx.fillText('Vùng bảng gửi đến Gemini AI', bx + 6, Math.max(14, by - 6));

      ctx.restore();
    };
    img.src = imageSrc;
  }, [imageSrc, isPdf, visible, cropBox, rotation]);

  // Xử lý kéo thả khung chọn trên Canvas
  const handleMouseDown = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const mx = (e.clientX - rect.left) / canvas.width;
    const my = (e.clientY - rect.top) / canvas.height;

    const bx = cropBox.x;
    const by = cropBox.y;
    const bw = cropBox.width;
    const bh = cropBox.height;

    const threshold = 0.05;

    // Kiểm tra góc
    if (Math.hypot(mx - bx, my - by) < threshold) setDragMode('nw');
    else if (Math.hypot(mx - (bx + bw), my - by) < threshold) setDragMode('ne');
    else if (Math.hypot(mx - (bx + bw), my - (by + bh)) < threshold) setDragMode('se');
    else if (Math.hypot(mx - bx, my - (by + bh)) < threshold) setDragMode('sw');
    else if (mx >= bx && mx <= bx + bw && my >= by && my <= by + bh) setDragMode('move');
    else setDragMode(null);

    setIsDragging(true);
    setDragStart({ x: mx, y: my, box: { ...cropBox } });
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLCanvasElement>) => {
    if (!isDragging || !dragMode) return;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const mx = (e.clientX - rect.left) / canvas.width;
    const my = (e.clientY - rect.top) / canvas.height;

    const dx = mx - dragStart.x;
    const dy = my - dragStart.y;
    const orig = dragStart.box;

    if (dragMode === 'move') {
      const nx = Math.max(0, Math.min(1 - orig.width, orig.x + dx));
      const ny = Math.max(0, Math.min(1 - orig.height, orig.y + dy));
      setCropBox({ ...orig, x: nx, y: ny });
    } else if (dragMode === 'se') {
      const nw = Math.max(0.1, Math.min(1 - orig.x, orig.width + dx));
      const nh = Math.max(0.1, Math.min(1 - orig.y, orig.height + dy));
      setCropBox({ ...orig, width: nw, height: nh });
    } else if (dragMode === 'nw') {
      const nx = Math.max(0, Math.min(orig.x + orig.width - 0.1, orig.x + dx));
      const ny = Math.max(0, Math.min(orig.y + orig.height - 0.1, orig.y + dy));
      const nw = orig.width - (nx - orig.x);
      const nh = orig.height - (ny - orig.y);
      setCropBox({ x: nx, y: ny, width: nw, height: nh });
    } else if (dragMode === 'ne') {
      const ny = Math.max(0, Math.min(orig.y + orig.height - 0.1, orig.y + dy));
      const nw = Math.max(0.1, Math.min(1 - orig.x, orig.width + dx));
      const nh = orig.height - (ny - orig.y);
      setCropBox({ ...orig, y: ny, width: nw, height: nh });
    } else if (dragMode === 'sw') {
      const nx = Math.max(0, Math.min(orig.x + orig.width - 0.1, orig.x + dx));
      const nw = orig.width - (nx - orig.x);
      const nh = Math.max(0.1, Math.min(1 - orig.y, orig.height + dy));
      setCropBox({ ...orig, x: nx, width: nw, height: nh });
    }
  };

  const handleMouseUp = () => {
    setIsDragging(false);
    setDragMode(null);
  };

  // Cắt ảnh từ vùng chọn Canvas thành Base64 (tối ưu kích thước max 1600px để OCR nét và truyền nhanh)
  const getCroppedBase64 = (): Promise<string> => {
    return new Promise((resolve) => {
      if (isPdf || !imageSrc) {
        resolve('');
        return;
      }
      const img = new Image();
      img.crossOrigin = 'anonymous';
      img.onload = () => {
        const offCanvas = document.createElement('canvas');
        const sx = cropBox.x * img.width;
        const sy = cropBox.y * img.height;
        const sw = cropBox.width * img.width;
        const sh = cropBox.height * img.height;

        const maxDim = 1600;
        const ratio = Math.min(1, maxDim / Math.max(sw, sh));
        const destW = Math.max(1, Math.round(sw * ratio));
        const destH = Math.max(1, Math.round(sh * ratio));

        offCanvas.width = destW;
        offCanvas.height = destH;
        const offCtx = offCanvas.getContext('2d');
        if (offCtx) {
          offCtx.drawImage(img, sx, sy, sw, sh, 0, 0, destW, destH);
          resolve(offCanvas.toDataURL('image/jpeg', 0.9));
        } else {
          resolve('');
        }
      };
      img.onerror = () => resolve('');
      img.src = imageSrc;
    });
  };

  // Kích hoạt bóc tách bằng Gemini Flash AI (Tự động fallback sang trình duyệt nếu Backend VM không có Internet)
  const handleStartOcr = async () => {
    try {
      let imageBase64 = '';
      if (!isPdf && imageSrc) {
        imageBase64 = await getCroppedBase64();
      } else if (isPdf && imageSrc && imageSrc.startsWith('data:')) {
        imageBase64 = imageSrc;
      }

      const trimmedKey = apiKey?.trim() || '';
      const payload: any = {
        apiKey: trimmedKey || undefined,
        documentUrl: documentUrl || undefined,
        imageBase64: imageBase64 || undefined,
      };

      extractOcr(
        payload,
        (data: any) => {
          setExtractedData(data);
          notifySuccess('', 'Gemini Flash AI đã bóc tách dữ liệu thành công!');
        },
        async (errorMsg?: string) => {
          const isNetError =
            errorMsg &&
            (errorMsg.includes('NET_CONNECT_FAILED') ||
              errorMsg.includes('Timeout') ||
              errorMsg.includes('elapsing') ||
              errorMsg.includes('kết nối') ||
              errorMsg.includes('SocketException'));

          if (isNetError && trimmedKey) {
            try {
              setLoading(true);
              const directData = await callGeminiDirectlyFromBrowser({
                apiKey: trimmedKey,
                imageBase64: imageBase64 || undefined,
                documentFullUrl: documentUrl ? getFullUrl(documentUrl) : undefined,
                isPdf,
              });
              setExtractedData(directData);
              notifySuccess('', 'Đã tự động quét thành công trực tiếp từ trình duyệt qua Gemini API!');
              return;
            } catch (directErr: any) {
              notifyError('', `Lỗi quét trực tiếp từ trình duyệt: ${directErr?.message || 'Không xác định'}`);
              return;
            } finally {
              setLoading(false);
            }
          }

          notifyError('', errorMsg?.replace('[NET_CONNECT_FAILED] ', '') || 'Bóc tách dữ liệu AI thất bại.');
        },
        setLoading
      );
    } catch (e: any) {
      notifyError('', e?.message || 'Có lỗi xảy ra khi xử lý ảnh.');
    }
  };

  const handleSaveApiKey = () => {
    localStorage.setItem('MDMS_GEMINI_API_KEY', apiKey.trim());
    notifySuccess('', 'Đã lưu Gemini API Key vào trình duyệt!');
    setShowApiKeyInput(false);
  };

  const handleApplyData = () => {
    if (!extractedData) return;
    onApply(extractedData);
    notifySuccess('', 'Đã áp dụng các thông số đo đếm vào biên bản!');
    onClose();
  };

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      width={980}
      title={
        <div className="flex items-center justify-between pr-8">
          <div className="flex items-center gap-2">
            <RobotOutlined style={{ color: '#7c3aed', fontSize: 22 }} />
            <div>
              <span className="font-bold text-slate-800 text-base">
                Trích xuất thông số bằng Gemini Flash AI & Canvas
              </span>
              <Tag color="purple" className="ml-2">Multimodal AI</Tag>
            </div>
          </div>

          <Button
            size="small"
            icon={<KeyOutlined />}
            onClick={() => setShowApiKeyInput(!showApiKeyInput)}
          >
            {apiKey ? 'API Key: Đã cài đặt' : 'Cài đặt Gemini API Key'}
          </Button>
        </div>
      }
      footer={
        <div className="flex justify-between items-center w-full pt-2">
          <div className="text-xs text-slate-500 flex items-center gap-1.5">
            <InfoCircleOutlined className="text-blue-500" />
            <span>Canvas tự động loại trừ thông tin khách hàng để bảo mật PII trước khi gửi sang Google Cloud.</span>
          </div>
          <div className="flex gap-2">
            <Button onClick={onClose}>Hủy</Button>
            {extractedData ? (
              <Button
                type="primary"
                style={{ backgroundColor: '#10b981', borderColor: '#10b981' }}
                icon={<CheckCircleOutlined />}
                onClick={handleApplyData}
              >
                Áp dụng vào biên bản
              </Button>
            ) : (
              <Button
                type="primary"
                style={{ backgroundColor: '#7c3aed', borderColor: '#7c3aed' }}
                icon={<ScissorOutlined />}
                onClick={handleStartOcr}
                loading={loading}
              >
                Cắt vùng chọn & Quét AI
              </Button>
            )}
          </div>
        </div>
      }
    >
      {/* Box nhập Gemini API Key */}
      {showApiKeyInput && (
        <Alert
          type="info"
          className="mb-4"
          message="Cấu hình Google Gemini API Key"
          description={
            <div className="mt-2 space-y-2">
              <div className="text-xs text-slate-600">
                Nhập Google Gemini API Key của bạn (hoàn toàn miễn phí 1.500 lượt/ngày tại aistudio.google.com).
                Khóa được lưu cục bộ an toàn trên trình duyệt của bạn.
              </div>
              <div className="flex gap-2">
                <Input.Password
                  placeholder="AIzaSy..."
                  value={apiKey}
                  onChange={(e) => setApiKey(e.target.value)}
                  className="font-mono"
                />
                <Button type="primary" onClick={handleSaveApiKey}>
                  Lưu khóa
                </Button>
              </div>
            </div>
          }
        />
      )}

      <div className="space-y-4">
        {/* KHÔNG GIAN CANVAS CROP */}
        <div className="bg-slate-900 rounded-lg p-3 overflow-hidden shadow-inner flex flex-col items-center justify-center min-h-[350px]">
          {isPdf ? (
            <div className="text-center p-8 bg-slate-800 rounded-lg text-white max-w-lg border border-slate-700">
              <div className="text-3xl mb-2">📄</div>
              <div className="font-semibold text-sm mb-1">Tài liệu định dạng PDF</div>
              <div className="text-xs text-slate-400 mb-4">
                Gemini Flash AI hỗ trợ đọc trực tiếp tệp PDF đa trang nguyên bản. Nhấn nút "Quét AI" để phân tích toàn bộ tài liệu.
              </div>
              <Button
                type="primary"
                style={{ backgroundColor: '#7c3aed', borderColor: '#7c3aed' }}
                icon={<RobotOutlined />}
                onClick={handleStartOcr}
                loading={loading}
              >
                Quét dữ liệu từ PDF
              </Button>
            </div>
          ) : imageSrc ? (
            <div ref={containerRef} className="w-full flex flex-col items-center">
              <div className="w-full flex justify-between items-center text-xs text-slate-300 pb-2 border-b border-slate-800 mb-2">
                <span>Kéo thả vùng viền nét đứt màu xanh để chọn bảng chỉ số công tơ</span>
                <div className="flex gap-2">
                  <Button
                    size="small"
                    ghost
                    icon={<FullscreenOutlined />}
                    onClick={() => setCropBox({ x: 0, y: 0, width: 1, height: 1 })}
                  >
                    Toàn bộ ảnh
                  </Button>
                  <Button
                    size="small"
                    ghost
                    onClick={() => setCropBox({ x: 0.05, y: 0.35, width: 0.9, height: 0.6 })}
                  >
                    Bỏ tiêu đề (60% dưới)
                  </Button>
                </div>
              </div>

              <canvas
                ref={canvasRef}
                onMouseDown={handleMouseDown}
                onMouseMove={handleMouseMove}
                onMouseUp={handleMouseUp}
                className="cursor-crosshair rounded border border-slate-700 shadow-md max-h-[480px] object-contain"
              />
            </div>
          ) : (
            <div className="text-center p-8 text-slate-400">
              <UploadOutlined className="text-3xl mb-2 text-slate-500" />
              <div className="text-xs">Chưa có ảnh biên bản hiện trường. Vui lòng tải ảnh lên trước hoặc chọn ảnh scan từ máy tính:</div>
              <div className="mt-3">
                <Upload
                  accept="image/*,.pdf"
                  showUploadList={false}
                  beforeUpload={(file) => {
                    const reader = new FileReader();
                    reader.onload = (e) => {
                      setImageSrc(e.target?.result as string);
                      setIsPdf(file.type === 'application/pdf');
                    };
                    reader.readAsDataURL(file);
                    return false;
                  }}
                >
                  <Button icon={<UploadOutlined />} size="small">Chọn ảnh kiểm thử ngay</Button>
                </Upload>
              </div>
            </div>
          )}
        </div>

        {/* LOADING INDICATOR */}
        {loading && (
          <div className="p-6 bg-purple-50 rounded-lg border border-purple-200 text-center">
            <Spin size="large" />
            <div className="mt-3 font-semibold text-purple-900 text-sm">
              Gemini Flash AI đang đọc và bóc tách bảng số liệu...
            </div>
            <div className="text-xs text-purple-600 mt-1">
              Đang phân tích số chế tạo, hệ số nhân và 6 bộ chỉ số đo đếm của công tơ tháo & công tơ treo
            </div>
          </div>
        )}

        {/* KẾT QUẢ BÓC TÁCH TỪ AI */}
        {extractedData && (
          <div className="border border-emerald-300 bg-emerald-50/40 rounded-lg p-4">
            <div className="flex items-center justify-between mb-3 border-b border-emerald-200 pb-2">
              <span className="font-bold text-emerald-900 text-sm flex items-center gap-1.5">
                <CheckCircleOutlined className="text-emerald-600" />
                Kết quả nhận diện từ Gemini Flash AI
              </span>
              <Tag color="success">Độ chính xác cao</Tag>
            </div>

            <Row gutter={16}>
              {/* CÔNG TƠ THÁO */}
              <Col span={12}>
                <Card size="small" title={<span className="text-amber-800 font-bold text-xs">Công tơ tháo</span>} className="border-amber-200">
                  <div className="text-xs space-y-1">
                    <div><b>Số chế tạo:</b> <span className="font-mono text-blue-700 font-bold">{extractedData.oldMeterSerial || '---'}</span></div>
                    <div><b>Hệ số nhân (HSN):</b> <b>{extractedData.oldMeterMultiplier ?? 1}</b></div>
                    <div className="grid grid-cols-2 gap-1 pt-1 border-t border-slate-100 text-[11px]">
                      <div>BT: <b>{Number(extractedData.oldBt || 0).toLocaleString()}</b> kWh</div>
                      <div>CD: <b>{Number(extractedData.oldCd || 0).toLocaleString()}</b> kWh</div>
                      <div>TD: <b>{Number(extractedData.oldTd || 0).toLocaleString()}</b> kWh</div>
                      <div>SG: <b className="text-amber-700">{Number(extractedData.oldSg || 0).toLocaleString()}</b> kWh</div>
                      <div>VC: <b>{Number(extractedData.oldVc || 0).toLocaleString()}</b> kvarh</div>
                      <div>Pmax: <b>{Number(extractedData.oldPmax || 0).toFixed(2)}</b> kW</div>
                    </div>
                  </div>
                </Card>
              </Col>

              {/* CÔNG TƠ TREO */}
              <Col span={12}>
                <Card size="small" title={<span className="text-emerald-800 font-bold text-xs">Công tơ treo</span>} className="border-emerald-200">
                  <div className="text-xs space-y-1">
                    <div><b>Số chế tạo:</b> <span className="font-mono text-emerald-700 font-bold">{extractedData.newMeterSerial || '---'}</span></div>
                    <div><b>Hệ số nhân (HSN):</b> <b>{extractedData.newMeterMultiplier ?? 1}</b></div>
                    <div className="grid grid-cols-2 gap-1 pt-1 border-t border-slate-100 text-[11px]">
                      <div>BT: <b>{Number(extractedData.newBt || 0).toLocaleString()}</b> kWh</div>
                      <div>CD: <b>{Number(extractedData.newCd || 0).toLocaleString()}</b> kWh</div>
                      <div>TD: <b>{Number(extractedData.newTd || 0).toLocaleString()}</b> kWh</div>
                      <div>SG: <b className="text-emerald-700">{Number(extractedData.newSg || 0).toLocaleString()}</b> kWh</div>
                      <div>VC: <b>{Number(extractedData.newVc || 0).toLocaleString()}</b> kvarh</div>
                      <div>Pmax: <b>{Number(extractedData.newPmax || 0).toFixed(2)}</b> kW</div>
                    </div>
                  </div>
                </Card>
              </Col>
            </Row>

            {extractedData.reason && (
              <div className="mt-2 text-xs text-slate-600 bg-white p-2 rounded border border-emerald-100">
                <b>Lý do ghi nhận:</b> {extractedData.reason}
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};

