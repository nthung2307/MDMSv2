import { test, describe } from 'node:test';
import assert from 'node:assert/strict';

// 1. Logic tính toán Cos phi trung bình (đồng nhất với Backend & TemplateNotice trên Web)
export const calculateCosTB = (ap, aq, explicitCosPhi = null) => {
  if (explicitCosPhi !== null && explicitCosPhi !== undefined && Number(explicitCosPhi) > 0) {
    return Number(explicitCosPhi).toFixed(2);
  }
  const p = Number(ap ?? 0);
  const q = Math.max(0, Number(aq ?? 0));
  const s = Math.sqrt(p * p + q * q);
  return s <= 0 ? '0.00' : (p / s).toFixed(2);
};

// 2. Logic định dạng số hiển thị (formatNumberWithCommas)
export const formatNumberWithCommas = (val, fractionDigits = 2) => {
  if (val === null || val === undefined || val === '') return '0';
  const num = Number(val);
  if (isNaN(num)) return '0';
  return num.toLocaleString('en-US', {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  });
};

// 3. Logic phát hiện chuỗi lỗi mã hóa font tiếng Việt (CP1252 / ASCII Corruption)
export const isGarbledEncoding = (str) => {
  if (!str) return false;
  return str.includes('├') || str.includes('ß') || str.includes('─') || str.includes('║') || str.includes('¥');
};

// 4. Logic giải mã cấu hình layout và danh sách cột hiển thị (parseStatementLayoutConfig)
export const parseStatementLayoutConfig = (jsonStr) => {
  const fallback = {
    columns: ['BT', 'CD', 'TD', 'SG', 'VC', 'COSPHI', 'REMARKS', 'PMAX', 'MULTIPLIER'],
    documentCode: 'M&E-1/01 PR04',
    copyLabel: '(M&E Copy – Liên cho Phòng Cơ Điện)',
    partyAName: 'Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)',
    partyBName: 'Party B (Tenant)\nBên B (Khách hàng)',
  };

  if (!jsonStr) return fallback;
  try {
    const trimmed = jsonStr.trim();
    if (trimmed.startsWith('[')) {
      const arr = JSON.parse(trimmed);
      if (Array.isArray(arr) && arr.length > 0) {
        return { ...fallback, columns: arr };
      }
    } else if (trimmed.startsWith('{')) {
      const obj = JSON.parse(trimmed);
      return {
        columns: Array.isArray(obj.columns || obj.Columns) ? obj.columns || obj.Columns : fallback.columns,
        documentCode: obj.documentCode || obj.DocumentCode || fallback.documentCode,
        copyLabel: obj.copyLabel || obj.CopyLabel || fallback.copyLabel,
        partyAName: obj.partyAName || obj.PartyAName || fallback.partyAName,
        partyBName: obj.partyBName || obj.PartyBName || fallback.partyBName,
      };
    }
  } catch {
    // fallback
  }
  return fallback;
};

// =====================================================================================
// BỘ TEST CASES FRONTEND
// =====================================================================================

describe('Frontend Metering Calculation & Utility Tests', () => {
  test('calculateCosTB: Tính Cos phi chính xác từ công suất tác dụng AP và phản kháng AQ', () => {
    // Kỳ chốt thực tế KYOCERA 1: AP=67, AQ=28 => Cos phi ~ 0.92
    assert.equal(calculateCosTB(67, 28), '0.92');

    // Thuần trở: AP=100, AQ=0 => Cos phi = 1.00
    assert.equal(calculateCosTB(100, 0), '1.00');

    // Tam giác 3-4-5: AP=80, AQ=60 => Cos phi = 0.80
    assert.equal(calculateCosTB(80, 60), '0.80');

    // Không có tải: AP=0, AQ=0 => Cos phi = 0.00
    assert.equal(calculateCosTB(0, 0), '0.00');

    // Sử dụng giá trị chốt sẵn có trong DB
    assert.equal(calculateCosTB(67, 28, 0.924), '0.92');
  });

  test('formatNumberWithCommas: Định dạng số có dấu phẩy ngăn cách hàng nghìn chuẩn', () => {
    assert.equal(formatNumberWithCommas(1234567.89, 2), '1,234,567.89');
    assert.equal(formatNumberWithCommas(66, 2), '66.00');
    assert.equal(formatNumberWithCommas(0, 2), '0.00');
    assert.equal(formatNumberWithCommas(null, 2), '0');
  });

  test('isGarbledEncoding: Nhận diện chính xác chuỗi tiếng Việt bị hỏng font', () => {
    assert.equal(isGarbledEncoding('PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG'), false);
    assert.equal(isGarbledEncoding('TH├öNG B├üO CH─q Sß╗É ─ÉIß╗åN N─éNG'), true);
    assert.equal(isGarbledEncoding(null), false);
  });

  test('parseStatementLayoutConfig: Giải mã JSON layout cấu hình Mẫu phiếu báo', () => {
    const rawJson = JSON.stringify({
      columns: ['BT', 'CD', 'TD', 'SG', 'VC', 'COSPHI', 'REMARKS', 'PMAX', 'MULTIPLIER'],
      documentCode: 'M&E-1/01 PR04',
      copyLabel: '(M&E Copy – Liên cho Phòng Cơ Điện)',
      partyAName: 'Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)',
      partyBName: 'Party B (Tenant)\nBên B (Khách hàng)',
    });

    const parsed = parseStatementLayoutConfig(rawJson);
    assert.equal(parsed.documentCode, 'M&E-1/01 PR04');
    assert.equal(parsed.copyLabel, '(M&E Copy – Liên cho Phòng Cơ Điện)');
    assert.equal(parsed.columns.length, 9);
    assert.ok(parsed.columns.includes('COSPHI'));
    assert.ok(parsed.columns.includes('PMAX'));
    assert.ok(parsed.columns.includes('MULTIPLIER'));
  });
});

