const OCR_PROMPT = `Bạn là trợ lý AI chuyên gia bóc tách số liệu biên bản treo tháo và bảng ghi chỉ số công tơ điện lực Việt Nam (AMR/MDMS).
Hãy đọc kỹ hình ảnh hoặc tài liệu đính kèm và trích xuất chính xác các thông tin đo đếm sang định dạng JSON sau:
{
  "oldMeterSerial": "Số chế tạo công tơ tháo (hoặc Serial No nếu là bảng ghi 1 công tơ)",
  "oldMeterMultiplier": 1,
  "oldBt": 0,
  "oldCd": 0,
  "oldTd": 0,
  "oldSg": 0,
  "oldVc": 0,
  "oldPmax": 0.00,
  "newMeterSerial": "Số chế tạo công tơ treo (nếu có)",
  "newMeterMultiplier": 1,
  "newBt": 0,
  "newCd": 0,
  "newTd": 0,
  "newSg": 0,
  "newVc": 0,
  "newPmax": 0.00,
  "reason": "Lý do treo tháo hoặc ghi chú nếu có trong biên bản"
}
Quy tắc chuẩn hóa:
- Nếu là biên bản treo tháo có 2 công tơ (Công tơ tháo & Công tơ treo): điền đầy đủ vào cả nhóm old* và new*.
- Nếu tài liệu chỉ ghi nhận 1 công tơ (ví dụ Phiếu ghi chỉ số / Biên bản xác nhận chỉ số có cột Giờ bình thường / Normal hours kWh meter 1, Giờ cao điểm / Peak hours, Giờ thấp điểm / Off-peak hours, Tổng tác dụng AP, Tổng phản kháng AQ, Hệ số / Multiplier, Pmax): điền Serial No và bộ chỉ số mới nhất (Current meter reading) vào nhóm old* (Công tơ tháo), để newMeterSerial = "".
- oldBt / newBt = Bình thường (Normal hours / BT).
- oldCd / newCd = Cao điểm (Peak hours / CD).
- oldTd / newTd = Thấp điểm (Off-peak hours / TD).
- oldSg / newSg = Tổng hữu công / Tổng công suất tác dụng (SG / AP).
- oldVc / newVc = Tổng phản kháng (VC / AQ).
- oldMeterMultiplier / newMeterMultiplier: Hệ số nhân HSN (Multiplier, mặc định 1 nếu không ghi).
- Các chỉ số điện năng BT, CD, TD, SG, VC làm tròn đến hàng đơn vị (số nguyên).
- Pmax: Công suất cực đại lấy số thực 2 chữ số thập phân (VD: 7.52 hoặc 18.23).
- Chỉ trả về duy nhất chuỗi JSON hợp lệ, tuyệt đối không chèn thêm văn bản hay markdown.`;

function cleanBase64DataUrl(dataUrl: string): { mimeType: string; data: string } {
  if (!dataUrl.includes(',')) {
    return { mimeType: 'image/jpeg', data: dataUrl };
  }
  const [header, data] = dataUrl.split(',', 2);
  let mimeType = 'image/jpeg';
  if (header.includes('image/png')) mimeType = 'image/png';
  else if (header.includes('image/webp')) mimeType = 'image/webp';
  else if (header.includes('application/pdf')) mimeType = 'application/pdf';
  return { mimeType, data };
}

async function fetchDocumentAsDataUrl(fullUrl: string, isPdf: boolean): Promise<string> {
  const res = await fetch(fullUrl, {
    headers: {
      Accept: isPdf
        ? 'application/pdf, application/octet-stream, */*'
        : 'image/*, application/octet-stream, */*',
    },
  });
  if (!res.ok) {
    throw new Error(`Không thể đọc tệp từ máy chủ (HTTP ${res.status})`);
  }
  const blob = await res.blob();
  const mimeType = isPdf ? 'application/pdf' : blob.type || 'image/jpeg';
  const typedBlob = new Blob([blob], { type: mimeType });

  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(new Error('Không thể chuyển đổi tệp sang Base64'));
    reader.readAsDataURL(typedBlob);
  });
}

/**
 * Fallback gọi trực tiếp Gemini REST API từ trình duyệt trong trường hợp máy chủ nội bộ (VM) không có Internet ra Google.
 */
export async function callGeminiDirectlyFromBrowser(params: {
  apiKey: string;
  imageBase64?: string;
  documentFullUrl?: string;
  isPdf?: boolean;
}): Promise<any> {
  const apiKey = params.apiKey.trim();
  if (!apiKey) {
    throw new Error('Vui lòng nhập Gemini API Key để quét trực tiếp từ trình duyệt.');
  }

  let dataUrl = params.imageBase64 || '';
  if (!dataUrl && params.documentFullUrl) {
    dataUrl = await fetchDocumentAsDataUrl(params.documentFullUrl, Boolean(params.isPdf));
  }
  if (!dataUrl) {
    throw new Error('Không có dữ liệu ảnh hoặc PDF để gửi lên Gemini.');
  }

  const { mimeType, data } = cleanBase64DataUrl(dataUrl);
  const requestBody = JSON.stringify({
    contents: [
      {
        parts: [
          { text: OCR_PROMPT },
          { inline_data: { mime_type: mimeType, data } },
        ],
      },
    ],
    generationConfig: {
      response_mime_type: 'application/json',
      temperature: 0.1,
    },
  });

  const candidates = ['gemini-2.5-flash', 'gemini-3.8-flash', 'gemini-3.5-flash-lite', 'gemini-2.0-flash'];
  let finalResponse: Response | null = null;
  let finalJson: any = null;
  let all404 = true;

  for (const model of candidates) {
    const url = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent`;
    const res = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-goog-api-key': apiKey,
      },
      body: requestBody,
    });
    finalResponse = res;
    finalJson = await res.json().catch(() => null);

    if (res.ok) {
      all404 = false;
      break;
    }
    if (res.status === 404) {
      continue;
    }
    all404 = false;
    break;
  }

  // Nếu các model cố định đều 404, truy vấn /v1beta/models lấy model Flash mới nhất khả dụng
  if (all404) {
    const listRes = await fetch('https://generativelanguage.googleapis.com/v1beta/models', {
      headers: { 'x-goog-api-key': apiKey },
    });
    if (listRes.ok) {
      const listJson = await listRes.json().catch(() => null);
      const flashModels: string[] = (listJson?.models || [])
        .filter(
          (m: any) =>
            typeof m?.name === 'string' &&
            m.name.toLowerCase().includes('flash') &&
            Array.isArray(m.supportedGenerationMethods) &&
            m.supportedGenerationMethods.includes('generateContent')
        )
        .map((m: any) => m.name.replace(/^models\//i, ''));

      for (const dynamicModel of flashModels) {
        const res = await fetch(
          `https://generativelanguage.googleapis.com/v1beta/models/${dynamicModel}:generateContent`,
          {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'x-goog-api-key': apiKey,
            },
            body: requestBody,
          }
        );
        finalResponse = res;
        finalJson = await res.json().catch(() => null);
        if (res.ok) break;
      }
    }
  }

  if (!finalResponse || !finalResponse.ok) {
    const msg = finalJson?.error?.message || `Lỗi gọi Gemini API (HTTP ${finalResponse?.status || 'Network'})`;
    throw new Error(msg);
  }

  const rawText: string = finalJson?.candidates?.[0]?.content?.parts?.[0]?.text || '';
  if (!rawText.trim()) {
    throw new Error('Gemini trả về nội dung rỗng.');
  }

  const cleanText = rawText
    .trim()
    .replace(/^```json\s*/i, '')
    .replace(/^```\s*/, '')
    .replace(/\s*```$/, '')
    .trim();

  return JSON.parse(cleanText);
}

