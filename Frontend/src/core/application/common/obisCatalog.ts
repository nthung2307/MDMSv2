/**
 * Canonical OBIS Telemetry Catalog for MDMSv2 (IEC 62056-61)
 * Provides deterministic OBIS codes across Electrical, Energy, Alert, and Date/Time parameters.
 */

export interface CanonicalObisInfo {
  obisCode: string;
  parameterCode: string;
  category: 'ELECTRICAL' | 'ENERGY' | 'ALERT' | 'DATETIME';
  typeTag?: 'DATE' | 'TIME';
  displayName?: string;
}

export const CANONICAL_OBIS_CATALOG: Record<string, CanonicalObisInfo> = {
  // =========================================================================
  // 1. THÔNG SỐ VẬN HÀNH (ELECTRICAL) - 21 thông số
  // =========================================================================
  DONG_DIEN_PHA_A: {
    obisCode: '31.7.0',
    parameterCode: 'DONG_DIEN_PHA_A',
    category: 'ELECTRICAL',
    displayName: 'Dòng điện pha A',
  },
  DONG_DIEN_PHA_B: {
    obisCode: '51.7.0',
    parameterCode: 'DONG_DIEN_PHA_B',
    category: 'ELECTRICAL',
    displayName: 'Dòng điện pha B',
  },
  DONG_DIEN_PHA_C: {
    obisCode: '71.7.0',
    parameterCode: 'DONG_DIEN_PHA_C',
    category: 'ELECTRICAL',
    displayName: 'Dòng điện pha C',
  },
  DIEN_AP_PHA_A: {
    obisCode: '32.7.0',
    parameterCode: 'DIEN_AP_PHA_A',
    category: 'ELECTRICAL',
    displayName: 'Điện áp pha A',
  },
  DIEN_AP_PHA_B: {
    obisCode: '52.7.0',
    parameterCode: 'DIEN_AP_PHA_B',
    category: 'ELECTRICAL',
    displayName: 'Điện áp pha B',
  },
  DIEN_AP_PHA_C: {
    obisCode: '72.7.0',
    parameterCode: 'DIEN_AP_PHA_C',
    category: 'ELECTRICAL',
    displayName: 'Điện áp pha C',
  },
  CONG_SUAT_TAC_DUNG_PHA_A: {
    obisCode: '21.7.0',
    parameterCode: 'CONG_SUAT_TAC_DUNG_PHA_A',
    category: 'ELECTRICAL',
    displayName: 'Công suất tác dụng pha A',
  },
  CONG_SUAT_TAC_DUNG_PHA_B: {
    obisCode: '41.7.0',
    parameterCode: 'CONG_SUAT_TAC_DUNG_PHA_B',
    category: 'ELECTRICAL',
    displayName: 'Công suất tác dụng pha B',
  },
  CONG_SUAT_TAC_DUNG_PHA_C: {
    obisCode: '61.7.0',
    parameterCode: 'CONG_SUAT_TAC_DUNG_PHA_C',
    category: 'ELECTRICAL',
    displayName: 'Công suất tác dụng pha C',
  },
  CONG_SUAT_TAC_DUNG_TONG_3_PHA: {
    obisCode: '1.7.0',
    parameterCode: 'CONG_SUAT_TAC_DUNG_TONG_3_PHA',
    category: 'ELECTRICAL',
    displayName: 'Công suất tác dụng tổng 3 pha',
  },
  CONG_SUAT_PHAN_KHANG_PHA_A: {
    obisCode: '23.7.0',
    parameterCode: 'CONG_SUAT_PHAN_KHANG_PHA_A',
    category: 'ELECTRICAL',
    displayName: 'Công suất phản kháng pha A',
  },
  CONG_SUAT_PHAN_KHANG_PHA_B: {
    obisCode: '43.7.0',
    parameterCode: 'CONG_SUAT_PHAN_KHANG_PHA_B',
    category: 'ELECTRICAL',
    displayName: 'Công suất phản kháng pha B',
  },
  CONG_SUAT_PHAN_KHANG_PHA_C: {
    obisCode: '63.7.0',
    parameterCode: 'CONG_SUAT_PHAN_KHANG_PHA_C',
    category: 'ELECTRICAL',
    displayName: 'Công suất phản kháng pha C',
  },
  CONG_SUAT_PHAN_KHANG_TONG_3_PHA: {
    obisCode: '3.7.0',
    parameterCode: 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA',
    category: 'ELECTRICAL',
    displayName: 'Công suất phản kháng tổng 3 pha',
  },
  HE_SO_CONG_SUAT_PF_PHA_A: {
    obisCode: '33.7.0',
    parameterCode: 'HE_SO_CONG_SUAT_PF_PHA_A',
    category: 'ELECTRICAL',
    displayName: 'Hệ số công suất PF pha A',
  },
  HE_SO_CONG_SUAT_PF_PHA_B: {
    obisCode: '53.7.0',
    parameterCode: 'HE_SO_CONG_SUAT_PF_PHA_B',
    category: 'ELECTRICAL',
    displayName: 'Hệ số công suất PF pha B',
  },
  HE_SO_CONG_SUAT_PF_PHA_C: {
    obisCode: '73.7.0',
    parameterCode: 'HE_SO_CONG_SUAT_PF_PHA_C',
    category: 'ELECTRICAL',
    displayName: 'Hệ số công suất PF pha C',
  },
  HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA: {
    obisCode: '13.7.0',
    parameterCode: 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA',
    category: 'ELECTRICAL',
    displayName: 'Hệ số công suất PF trung bình 3 pha',
  },
  TAN_SO_HE_THONG: {
    obisCode: '14.7.0',
    parameterCode: 'TAN_SO_HE_THONG',
    category: 'ELECTRICAL',
    displayName: 'Tần số hệ thống',
  },
  PMAX_KY_HIEN_TAI: {
    obisCode: '1.6.0',
    parameterCode: 'PMAX_KY_HIEN_TAI',
    category: 'ELECTRICAL',
    displayName: 'Công suất cực đại kỳ hiện tại',
  },
  PMAX_KY_TRUOC: {
    obisCode: '1.6.0.1',
    parameterCode: 'PMAX_KY_TRUOC',
    category: 'ELECTRICAL',
    displayName: 'Công suất cực đại kỳ trước',
  },

  // =========================================================================
  // 2. THÔNG SỐ ĐIỆN NĂNG (ENERGY) - 20 thông số
  // =========================================================================
  // Chiều Giao:
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG: {
    obisCode: '1.8.0',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP tổng (Giao)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT: {
    obisCode: '1.8.1',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ BT (Giao)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD: {
    obisCode: '1.8.2',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ CD (Giao)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD: {
    obisCode: '1.8.3',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ TD (Giao)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG: {
    obisCode: '3.8.0',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AQ tổng (Giao)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_TONG: {
    obisCode: '1.8.0.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_TONG',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP tổng (Giao)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT: {
    obisCode: '1.8.1.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ BT (Giao)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD: {
    obisCode: '1.8.2.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ CD (Giao)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD: {
    obisCode: '1.8.3.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ TD (Giao)',
  },
  CHI_SO_CHOT_DIEN_NANG_AQ_TONG: {
    obisCode: '3.8.0.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AQ tổng (Giao)',
  },

  // Chiều Nhận:
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN: {
    obisCode: '2.8.0',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP tổng (Nhận)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN: {
    obisCode: '2.8.1',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ BT (Nhận)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN: {
    obisCode: '2.8.2',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ CD (Nhận)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN: {
    obisCode: '2.8.3',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AP biểu giờ TD (Nhận)',
  },
  CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN: {
    obisCode: '4.8.0',
    parameterCode: 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số tích lũy AQ tổng (Nhận)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN: {
    obisCode: '2.8.0.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP tổng (Nhận)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN: {
    obisCode: '2.8.1.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ BT (Nhận)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN: {
    obisCode: '2.8.2.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ CD (Nhận)',
  },
  CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN: {
    obisCode: '2.8.3.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AP giờ TD (Nhận)',
  },
  CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN: {
    obisCode: '4.8.0.1',
    parameterCode: 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN',
    category: 'ENERGY',
    displayName: 'Chỉ số chốt AQ tổng (Nhận)',
  },

  // =========================================================================
  // 3. CẢNH BÁO & KẾT NỐI (ALERT) - 11 thông số
  // =========================================================================
  CANH_BAO_01: {
    obisCode: '97.97.1',
    parameterCode: 'CANH_BAO_01',
    category: 'ALERT',
    displayName: 'Cảnh báo 01',
  },
  CANH_BAO_02: {
    obisCode: '97.97.2',
    parameterCode: 'CANH_BAO_02',
    category: 'ALERT',
    displayName: 'Cảnh báo 02',
  },
  CANH_BAO_03: {
    obisCode: '97.97.3',
    parameterCode: 'CANH_BAO_03',
    category: 'ALERT',
    displayName: 'Cảnh báo 03',
  },
  CANH_BAO_04: {
    obisCode: '97.97.4',
    parameterCode: 'CANH_BAO_04',
    category: 'ALERT',
    displayName: 'Cảnh báo 04',
  },
  CANH_BAO_05: {
    obisCode: '97.97.5',
    parameterCode: 'CANH_BAO_05',
    category: 'ALERT',
    displayName: 'Cảnh báo 05',
  },
  CANH_BAO_06: {
    obisCode: '97.97.6',
    parameterCode: 'CANH_BAO_06',
    category: 'ALERT',
    displayName: 'Cảnh báo 06',
  },
  CANH_BAO_07: {
    obisCode: '97.97.7',
    parameterCode: 'CANH_BAO_07',
    category: 'ALERT',
    displayName: 'Cảnh báo 07',
  },
  CANH_BAO_08: {
    obisCode: '97.97.8',
    parameterCode: 'CANH_BAO_08',
    category: 'ALERT',
    displayName: 'Cảnh báo 08',
  },
  CANH_BAO_09: {
    obisCode: '97.97.9',
    parameterCode: 'CANH_BAO_09',
    category: 'ALERT',
    displayName: 'Cảnh báo 09',
  },
  CANH_BAO_10: {
    obisCode: '97.97.10',
    parameterCode: 'CANH_BAO_10',
    category: 'ALERT',
    displayName: 'Cảnh báo 10',
  },
  MAT_KET_NOI: {
    obisCode: '96.5.0',
    parameterCode: 'MAT_KET_NOI',
    category: 'ALERT',
    displayName: 'Trạng thái kết nối',
  },

  // =========================================================================
  // 4. THỜI GIAN (DATETIME) - 24 thông số (Phân định rõ Date & Time theo IEC 62056-61)
  // =========================================================================
  // Nhóm 1: Thời gian hiện tại (Time: 0.9.1, Date: 0.9.2)
  NAM_HIEN_TAI: {
    obisCode: '0.9.2',
    parameterCode: 'NAM_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Năm hiện tại (Date)',
  },
  THANG_HIEN_TAI: {
    obisCode: '0.9.2',
    parameterCode: 'THANG_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Tháng hiện tại (Date)',
  },
  NGAY_HIEN_TAI: {
    obisCode: '0.9.2',
    parameterCode: 'NGAY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Ngày hiện tại (Date)',
  },
  GIO_HIEN_TAI: {
    obisCode: '0.9.1',
    parameterCode: 'GIO_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giờ hiện tại (Time)',
  },
  PHUT_HIEN_TAI: {
    obisCode: '0.9.1',
    parameterCode: 'PHUT_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Phút hiện tại (Time)',
  },
  GIAY_HIEN_TAI: {
    obisCode: '0.9.1',
    parameterCode: 'GIAY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giây hiện tại (Time)',
  },

  // Nhóm 2: Thời gian chốt gần nhất (Date: 0.9.3, Time: 0.9.4)
  NAM_CHOT_GAN_NHAT: {
    obisCode: '0.9.3',
    parameterCode: 'NAM_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Năm chốt gần nhất (Date)',
  },
  THANG_CHOT_GAN_NHAT: {
    obisCode: '0.9.3',
    parameterCode: 'THANG_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Tháng chốt gần nhất (Date)',
  },
  NGAY_CHOT_GAN_NHAT: {
    obisCode: '0.9.3',
    parameterCode: 'NGAY_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Ngày chốt gần nhất (Date)',
  },
  GIO_CHOT_GAN_NHAT: {
    obisCode: '0.9.4',
    parameterCode: 'GIO_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giờ chốt gần nhất (Time)',
  },
  PHUT_CHOT_GAN_NHAT: {
    obisCode: '0.9.4',
    parameterCode: 'PHUT_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Phút chốt gần nhất (Time)',
  },
  GIAY_CHOT_GAN_NHAT: {
    obisCode: '0.9.4',
    parameterCode: 'GIAY_CHOT_GAN_NHAT',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giây chốt gần nhất (Time)',
  },

  // Nhóm 3: Thời gian công suất cực đại kỳ hiện tại (Date: 0.9.5, Time: 0.9.6)
  NAM_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.5',
    parameterCode: 'NAM_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Năm Pmax kỳ hiện tại (Date)',
  },
  THANG_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.5',
    parameterCode: 'THANG_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Tháng Pmax kỳ hiện tại (Date)',
  },
  NGAY_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.5',
    parameterCode: 'NGAY_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Ngày Pmax kỳ hiện tại (Date)',
  },
  GIO_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.6',
    parameterCode: 'GIO_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giờ Pmax kỳ hiện tại (Time)',
  },
  PHUT_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.6',
    parameterCode: 'PHUT_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Phút Pmax kỳ hiện tại (Time)',
  },
  GIAY_PMAX_KY_HIEN_TAI: {
    obisCode: '0.9.6',
    parameterCode: 'GIAY_PMAX_KY_HIEN_TAI',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giây Pmax kỳ hiện tại (Time)',
  },

  // Nhóm 4: Thời gian công suất cực đại kỳ trước (Date: 0.9.7, Time: 0.9.8)
  NAM_PMAX_KY_TRUOC: {
    obisCode: '0.9.7',
    parameterCode: 'NAM_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Năm Pmax kỳ trước (Date)',
  },
  THANG_PMAX_KY_TRUOC: {
    obisCode: '0.9.7',
    parameterCode: 'THANG_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Tháng Pmax kỳ trước (Date)',
  },
  NGAY_PMAX_KY_TRUOC: {
    obisCode: '0.9.7',
    parameterCode: 'NGAY_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'DATE',
    displayName: 'Ngày Pmax kỳ trước (Date)',
  },
  GIO_PMAX_KY_TRUOC: {
    obisCode: '0.9.8',
    parameterCode: 'GIO_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giờ Pmax kỳ trước (Time)',
  },
  PHUT_PMAX_KY_TRUOC: {
    obisCode: '0.9.8',
    parameterCode: 'PHUT_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Phút Pmax kỳ trước (Time)',
  },
  GIAY_PMAX_KY_TRUOC: {
    obisCode: '0.9.8',
    parameterCode: 'GIAY_PMAX_KY_TRUOC',
    category: 'DATETIME',
    typeTag: 'TIME',
    displayName: 'Giây Pmax kỳ trước (Time)',
  },
};

/**
 * Lấy thông tin mã OBIS chuẩn hóa cho một mã tham số.
 * Ưu tiên mapping từ cấu hình MeterProfile (nếu có), nếu chưa tải hoặc không có thì fallback về catalog chuẩn.
 */
export const getCanonicalObisInfo = (
  paramCode: string,
  profileMappings?: Array<{ parameterCode: string; obisCode: string }>
): CanonicalObisInfo | undefined => {
  if (profileMappings && profileMappings.length > 0) {
    const prof = profileMappings.find((m) => m.parameterCode === paramCode);
    if (prof && prof.obisCode) {
      const canonical = CANONICAL_OBIS_CATALOG[paramCode];
      return {
        obisCode: prof.obisCode.replace(/_/g, '.'),
        parameterCode: paramCode,
        category: canonical?.category || 'ELECTRICAL',
        typeTag: canonical?.typeTag,
        displayName: canonical?.displayName,
      };
    }
  }
  return CANONICAL_OBIS_CATALOG[paramCode];
};

