import { Col, Row } from 'antd';
import { useEffect, useState } from 'react';

import exportIc from '@/assets/export-pdf-ic.svg';
import exportItem from '@/assets/export-pdf-item.svg';
import { StatementPdfTemplateDto } from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import TablePDFCommon from '@/infrastructure/common/components/table/table-pdf-common';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service';
import { convertDateTime, formatNumberWithCommas, renderTextNumber } from '@/infrastructure/utils/helpers';

export interface StatementLayoutConfig {
  columns: string[];
  taxCode: string;
  showTaxCode: boolean;
  documentCode: string;
  copyLabel: string;
  partyAName: string;
  partyBName: string;
  headerLeftMode: 'LOGO' | 'NONE' | string;
  headerCenterMode?: 'LOGO' | 'NONE' | string;
  headerRightMode: 'DECOR' | 'LOGO' | 'NONE' | string;
  headerLeftText?: string;
  headerCenterText?: string;
  headerRightText?: string;
  footerLeftMode?: 'LOGO' | 'NONE' | string;
  footerCenterMode?: 'LOGO' | 'NONE' | string;
  footerRightMode?: 'LOGO' | 'NONE' | string;
  footerLeftText: string;
  footerCenterText: string;
  footerRightText: string;
}

export interface StatementLogosConfig {
  headerLeftLogo?: string;
  headerCenterLogo?: string;
  headerRightLogo?: string;
  footerLeftLogo?: string;
  footerCenterLogo?: string;
  footerRightLogo?: string;
}

export const DEFAULT_LAYOUT_CONFIG: StatementLayoutConfig = {
  columns: ['BT', 'CD', 'TD', 'SG', 'VC', 'COSPHI', 'REMARKS', 'PMAX', 'MULTIPLIER'],
  taxCode: '',
  showTaxCode: false,
  documentCode: 'M&E-1/01 PR04',
  copyLabel: '(M&E Copy – Liên cho Phòng Cơ Điện)',
  partyAName: 'Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)',
  partyBName: 'Party B (Tenant)\nBên B (Khách hàng)',
  headerLeftMode: 'LOGO',
  headerCenterMode: 'LOGO',
  headerRightMode: 'DECOR',
  headerLeftText: '',
  headerCenterText: '',
  headerRightText: '',
  footerLeftMode: 'LOGO',
  footerCenterMode: 'LOGO',
  footerRightMode: 'LOGO',
  footerLeftText: '',
  footerCenterText: '',
  footerRightText: '',
};

const STORAGE_CACHE_KEY = 'mdms_active_statement_template_v2';
let memoryCachedTemplate: StatementPdfTemplateDto | null = null;

export const setCachedStatementTemplate = (tpl: StatementPdfTemplateDto | null) => {
  memoryCachedTemplate = tpl;
  try {
    if (tpl) {
      localStorage.setItem(STORAGE_CACHE_KEY, JSON.stringify(tpl));
    } else {
      localStorage.removeItem(STORAGE_CACHE_KEY);
    }
  } catch {
    // Ignore storage quota errors
  }
};

export const getCachedStatementTemplate = (): StatementPdfTemplateDto | null => {
  if (memoryCachedTemplate) return memoryCachedTemplate;
  try {
    const raw = localStorage.getItem(STORAGE_CACHE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      memoryCachedTemplate = parsed;
      return parsed;
    }
  } catch {
    // Ignore parse error
  }
  return null;
};

export const parseStatementLayoutConfig = (displayColumnsJson?: string): StatementLayoutConfig => {
  const fallback: StatementLayoutConfig = {
    ...DEFAULT_LAYOUT_CONFIG,
    columns: [...DEFAULT_LAYOUT_CONFIG.columns],
  };
  if (!displayColumnsJson) return fallback;
  const trimmed = displayColumnsJson.trim();
  try {
    if (trimmed.startsWith('[')) {
      const arr = JSON.parse(trimmed);
      if (Array.isArray(arr) && arr.length > 0) {
        const set = new Set(arr.map((x: string) => String(x).toUpperCase()));
        if (!set.has('REMARKS')) arr.push('REMARKS');
        if (!set.has('MULTIPLIER')) arr.push('MULTIPLIER');
        return { ...fallback, columns: arr };
      }
    } else if (trimmed.startsWith('{')) {
      const obj = JSON.parse(trimmed);
      return {
        columns:
          Array.isArray(obj.columns || obj.Columns) && (obj.columns || obj.Columns).length > 0
            ? obj.columns || obj.Columns
            : fallback.columns,
        taxCode: obj.taxCode ?? obj.TaxCode ?? fallback.taxCode,
        showTaxCode: obj.showTaxCode ?? obj.ShowTaxCode ?? fallback.showTaxCode,
        documentCode: obj.documentCode ?? obj.DocumentCode ?? fallback.documentCode,
        copyLabel: obj.copyLabel ?? obj.CopyLabel ?? fallback.copyLabel,
        partyAName: obj.partyAName ?? obj.PartyAName ?? fallback.partyAName,
        partyBName: obj.partyBName ?? obj.PartyBName ?? fallback.partyBName,
        headerLeftMode: obj.headerLeftMode ?? obj.HeaderLeftMode ?? fallback.headerLeftMode,
        headerCenterMode: obj.headerCenterMode ?? obj.HeaderCenterMode ?? fallback.headerCenterMode,
        headerRightMode: obj.headerRightMode ?? obj.HeaderRightMode ?? fallback.headerRightMode,
        headerLeftText: obj.headerLeftText ?? obj.HeaderLeftText ?? fallback.headerLeftText,
        headerCenterText: obj.headerCenterText ?? obj.HeaderCenterText ?? fallback.headerCenterText,
        headerRightText: obj.headerRightText ?? obj.HeaderRightText ?? fallback.headerRightText,
        footerLeftMode: obj.footerLeftMode ?? obj.FooterLeftMode ?? fallback.footerLeftMode,
        footerCenterMode: obj.footerCenterMode ?? obj.FooterCenterMode ?? fallback.footerCenterMode,
        footerRightMode: obj.footerRightMode ?? obj.FooterRightMode ?? fallback.footerRightMode,
        footerLeftText: obj.footerLeftText ?? obj.FooterLeftText ?? fallback.footerLeftText,
        footerCenterText: obj.footerCenterText ?? obj.FooterCenterText ?? fallback.footerCenterText,
        footerRightText: obj.footerRightText ?? obj.FooterRightText ?? fallback.footerRightText,
      };
    }
  } catch {
    // Fallback
  }
  return fallback;
};

export const parseStatementLogosConfig = (
  excelTemplateBase64?: string,
  logoBase64?: string
): StatementLogosConfig => {
  const fallback: StatementLogosConfig = {
    headerLeftLogo: logoBase64 || '',
  };
  if (excelTemplateBase64 && excelTemplateBase64.trim().startsWith('{')) {
    try {
      const obj = JSON.parse(excelTemplateBase64.trim());
      return {
        headerLeftLogo: obj.headerLeftLogo ?? obj.HeaderLeftLogo ?? logoBase64 ?? '',
        headerCenterLogo: obj.headerCenterLogo ?? obj.HeaderCenterLogo ?? '',
        headerRightLogo: obj.headerRightLogo ?? obj.HeaderRightLogo ?? '',
        footerLeftLogo: obj.footerLeftLogo ?? obj.FooterLeftLogo ?? '',
        footerCenterLogo: obj.footerCenterLogo ?? obj.FooterCenterLogo ?? '',
        footerRightLogo: obj.footerRightLogo ?? obj.FooterRightLogo ?? '',
      };
    } catch {
      // Ignore parse error
    }
  }
  return fallback;
};

export const formatBase64Src = (b64?: string): string => {
  if (!b64) return '';
  const trimmed = b64.trim();
  if (!trimmed) return '';
  if (trimmed.startsWith('data:image') || trimmed.startsWith('http') || trimmed.startsWith('/')) {
    return trimmed;
  }
  return `data:image/png;base64,${trimmed}`;
};

type Props = {
  record: any;
  template?: Partial<StatementPdfTemplateDto> | null;
  companyName?: string;
  companyAddress?: string;
  contactPhone?: string;
};

const TemplateNotice = (props: Props) => {
  const { record, template: propTemplate, companyName, companyAddress, contactPhone } = props;
  const [fetchedTemplate, setFetchedTemplate] = useState<StatementPdfTemplateDto | null>(() =>
    getCachedStatementTemplate()
  );

  useEffect(() => {
    if (propTemplate) return;
    let mounted = true;
    const service = new TemplateManagementService();
    service
      .getStatementPdfTemplates()
      .then((res: any) => {
        if (!mounted) return;
        const rawList = res?.data?.data !== undefined ? res.data.data : res?.data;
        if (Array.isArray(rawList) && rawList.length > 0) {
          const chosen =
            rawList.find((x: StatementPdfTemplateDto) => x.isDefault && x.isActive) ||
            rawList.find((x: StatementPdfTemplateDto) => x.isActive) ||
            rawList.find((x: StatementPdfTemplateDto) => x.isDefault) ||
            rawList[0];
          if (chosen) {
            setCachedStatementTemplate(chosen);
            setFetchedTemplate(chosen);
          }
        }
      })
      .catch(() => {
        // Ignore if offline or unauthenticated
      });
    return () => {
      mounted = false;
    };
  }, [propTemplate]);

  const activeTemplate: Partial<StatementPdfTemplateDto> =
    propTemplate || record?.statementPdfTemplate || fetchedTemplate || {};

  const layout = parseStatementLayoutConfig(activeTemplate.displayColumnsJson);
  const logos = parseStatementLogosConfig(
    activeTemplate.excelTemplateBase64,
    activeTemplate.logoBase64
  );
  const enabledCols = new Set((layout.columns || []).map((c) => String(c).toUpperCase()));

  const displayNumber = (value: any) => {
    return value === null || value === undefined ? '0' : formatNumberWithCommas(value);
  };

  const formatTimestampToLocal = (ts: any, format: 'date' | 'time' | 'datetime') => {
    if (!ts) return null;
    try {
      const d = new Date(ts);
      if (isNaN(d.getTime())) return null;
      const dd = String(d.getDate()).padStart(2, '0');
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const yyyy = d.getFullYear();
      const hh = String(d.getHours()).padStart(2, '0');
      const mi = String(d.getMinutes()).padStart(2, '0');
      const ss = String(d.getSeconds()).padStart(2, '0');
      if (format === 'date') return `${dd}/${mm}/${yyyy}`;
      if (format === 'time') return `${hh}:${mi}:${ss}`;
      return `${dd}/${mm}/${yyyy} ${hh}:${mi}:${ss}`;
    } catch {
      return null;
    }
  };

  // 1. Reading Date & Reading Time của chỉ số mới (Thời điểm chốt gửi về từ công tơ lưu trong DB nếu có)
  const meterLatchTimestamp = record?.billingTimestamp || record?.readingTime;
  const dbDay = record?.day && Number(record.day) > 0 ? renderTextNumber(record.day) : null;
  const dbMonth = record?.month && Number(record.month) > 0 ? renderTextNumber(record.month) : null;
  const dbYear = record?.year && Number(record.year) > 0 ? record.year : null;
  const defaultReadingDate = dbDay && dbMonth && dbYear ? `${dbDay}/${dbMonth}/${dbYear}` : '.........';

  const readingDate = formatTimestampToLocal(meterLatchTimestamp, 'date') || defaultReadingDate;
  const readingTimeVal = formatTimestampToLocal(meterLatchTimestamp, 'time') || '00:00:00';

  // 2. Reading Date (Ngày ghi) của chỉ số cũ (lấy trong DB nếu có)
  const hasDbPreviousDate =
    Boolean(record?.previousDay) &&
    Boolean(record?.previousMonth) &&
    Boolean(record?.previousYear) &&
    Number(record.previousDay) > 0;

  const previousReadingDate = hasDbPreviousDate
    ? `${renderTextNumber(record.previousDay)}/${renderTextNumber(record.previousMonth)}/${record.previousYear}`
    : formatTimestampToLocal(record?.previousBillingTimestamp || record?.previousReadingDate || record?.previousDate, 'date') ||
      '.........';

  const forMonthVal = `${renderTextNumber(record?.month)}/${record?.year}`;

  const localPmaxTime = formatTimestampToLocal(record?.pmaxTime, 'datetime') || '---';

  const isGarbled = (s?: string) =>
    !s ? false : s.includes('├') || s.includes('ß') || s.includes('─') || s.includes('║') || s.includes('¥');

  const compNameRaw = companyName || activeTemplate.companyName;
  const compName =
    compNameRaw && !isGarbled(compNameRaw) ? compNameRaw : 'VSIP HAI PHONG CO., LTD';

  const compAddrRaw = companyAddress !== undefined ? companyAddress : activeTemplate.companyAddress;
  const compAddr =
    compAddrRaw !== undefined && !isGarbled(compAddrRaw)
      ? compAddrRaw
      : 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam';

  const compPhoneRaw = contactPhone !== undefined ? contactPhone : activeTemplate.contactPhone;
  const compPhone =
    compPhoneRaw !== undefined && !isGarbled(compPhoneRaw)
      ? compPhoneRaw
      : 'Tel: 84-225-3959868   Fax: 84-225-3959886';

  const headerTitleRaw = activeTemplate.headerTitle;
  const headerTitle =
    headerTitleRaw && !isGarbled(headerTitleRaw)
      ? headerTitleRaw
      : 'MONTHLY POWER METER READING';

  const subHeaderTitleRaw = activeTemplate.subHeaderTitle;
  const subHeaderTitle =
    subHeaderTitleRaw !== undefined && !isGarbled(subHeaderTitleRaw)
      ? subHeaderTitleRaw
      : 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG';
  const copyLabel =
    layout.copyLabel !== undefined ? layout.copyLabel : '(M&E Copy – Liên cho Phòng Cơ Điện)';
  const documentCode = layout.documentCode !== undefined ? layout.documentCode : 'M&E-1/01 PR04';
  const requireDualSignatures =
    activeTemplate.requireDualSignatures !== undefined
      ? activeTemplate.requireDualSignatures
      : true;

  const resultCosTB = (AP: any, AQ: any) => {
    if (record?.cosPhi !== null && record?.cosPhi !== undefined) {
      return Number(record.cosPhi).toFixed(2);
    }
    const p = Number(AP ?? 0);
    const q = Math.max(0, Number(AQ ?? 0));
    const s = Math.sqrt(p * p + q * q);
    return s <= 0 ? '0.00' : (p / s).toFixed(2);
  };

  const allRows = [
    {
      key: '1',
      description: (
        <div>
          <p>Previous meter reading</p>
          <p>(Chỉ số cũ)</p>
          <p className='text-nowrap tracking-[-0.03em]'>{`Reading date: ${previousReadingDate}`}</p>
          <p>(Ngày ghi)</p>
        </div>
      ),
      normalHours: displayNumber(record?.previousBt),
      peakHours: displayNumber(record?.previousCd),
      offPeakHours: displayNumber(record?.previousTd),
      totalActivePower: displayNumber(record?.previousSg),
      totalReactivePower: displayNumber(record?.previousVc),
      cosPhi: '',
      remarks: '',
    },
    {
      key: '2',
      description: (
        <div>
          <p>Current meter reading</p>
          <p>(Chỉ số mới)</p>
        </div>
      ),
      normalHours: displayNumber(record?.bt),
      peakHours: displayNumber(record?.cd),
      offPeakHours: displayNumber(record?.td),
      totalActivePower: displayNumber(record?.sg),
      totalReactivePower: displayNumber(record?.vc),
      cosPhi: '',
      remarks: '',
    },
    ...(enabledCols.has('MULTIPLIER')
      ? [
          {
            key: '3',
            description: (
              <div>
                <p>Multiplier</p>
                <p>(hệ số)</p>
              </div>
            ),
            normalHours: displayNumber(
              record?.measurementPoint?.multiplierFactor ?? record?.multiplier ?? 1
            ),
            peakHours: displayNumber(
              record?.measurementPoint?.multiplierFactor ?? record?.multiplier ?? 1
            ),
            offPeakHours: displayNumber(
              record?.measurementPoint?.multiplierFactor ?? record?.multiplier ?? 1
            ),
            totalActivePower: displayNumber(
              record?.measurementPoint?.multiplierFactor ?? record?.multiplier ?? 1
            ),
            totalReactivePower: displayNumber(
              record?.measurementPoint?.multiplierFactor ?? record?.multiplier ?? 1
            ),
            cosPhi: '',
            remarks: '',
          },
        ]
      : []),
    {
      key: '4',
      description: (
        <div>
          <p>Actual consumption</p>
          <p>(Số tiêu thụ thực tế)</p>
        </div>
      ),
      normalHours: displayNumber(record?.consumtionBt),
      peakHours: displayNumber(record?.consumtionCd),
      offPeakHours: displayNumber(record?.consumtionTd),
      totalActivePower: displayNumber(record?.consumtionSg),
      totalReactivePower: displayNumber(record?.consumtionVc),
      cosPhi: (() => {
        const val = resultCosTB(record?.consumtionSg, record?.consumtionVc);
        return !val || val === '0.00' || val === '0' ? '' : displayNumber(val);
      })(),
      remarks: '',
    },
  ];

  const titleCustome = (title: string, description: string, param?: string) => {
    return (
      <div className='flex flex-col items-center text-center'>
        <p className='title-custome-pdf'>{title}</p>
        <p className='desc-custome-pdf '>{description}</p>
        {param && <span>{param}</span>}
      </div>
    );
  };

  const metricColumnsDef = [
    {
      code: 'BT',
      col: {
        title: titleCustome('Normal hours kWh meter 1', '(Giờ bình thường)'),
        dataIndex: 'normalHours',
        key: 'normalHours',
        width: 96,
      },
    },
    {
      code: 'CD',
      col: {
        title: titleCustome('Peak hours kWh meter 2', '(Giờ cao điểm)'),
        dataIndex: 'peakHours',
        key: 'peakHours',
        width: 96,
      },
    },
    {
      code: 'TD',
      col: {
        title: titleCustome('Off-peak hours kWh meter 3', '(Giờ thấp điểm)'),
        dataIndex: 'offPeakHours',
        key: 'offPeakHours',
        width: 96,
      },
    },
    {
      code: 'SG',
      col: {
        title: titleCustome('Total active power', '(Tổng công suất tác dụng)', 'AP'),
        dataIndex: 'totalActivePower',
        key: 'totalActivePower',
        width: 112,
      },
    },
    {
      code: 'VC',
      col: {
        title: titleCustome('Total reactive power', '(Tổng công suất phản kháng)', 'AQ'),
        dataIndex: 'totalReactivePower',
        key: 'totalReactivePower',
        width: 112,
      },
    },
    {
      code: 'COSPHI',
      col: {
        title: titleCustome('Cos φ TB', '(≥0.9)'),
        dataIndex: 'cosPhi',
        key: 'cosPhi',
        width: 50,
      },
    },
    {
      code: 'REMARKS',
      col: {
        title: titleCustome('Remarks', '(Ghi chú)'),
        dataIndex: 'remarks',
        key: 'remarks',
        width: 50,
      },
    },
  ];

  let activeMetricCols = metricColumnsDef.filter((m) => enabledCols.has(m.code)).map((m) => m.col);
  if (activeMetricCols.length === 0) {
    activeMetricCols = metricColumnsDef
      .filter((m) => m.code === 'BT' || m.code === 'SG')
      .map((m) => m.col);
  }

  const columns = [
    {
      title: titleCustome('Description', '(Mô tả)'),
      dataIndex: 'description',
      key: 'description',
      width: 125,
    },
    ...activeMetricCols,
  ];

  const leftLogoSrc =
    layout.headerLeftMode === 'NONE'
      ? ''
      : logos.headerLeftLogo
      ? formatBase64Src(logos.headerLeftLogo)
      : exportIc;

  const centerLogoSrc = layout.headerCenterMode === 'NONE' ? '' : formatBase64Src(logos.headerCenterLogo);
  const rightLogoSrc = formatBase64Src(logos.headerRightLogo);

  const partyALines = (
    layout.partyAName || 'Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)'
  ).split('\n');
  const partyBLines = (
    layout.partyBName || 'Party B (Tenant)\nBên B (Khách hàng)'
  ).split('\n');

  const showFooterLeft = layout.footerLeftMode !== 'NONE';
  const showFooterCenter = layout.footerCenterMode !== 'NONE';
  const showFooterRight = layout.footerRightMode !== 'NONE';

  const hasFooter =
    (showFooterLeft && (Boolean(logos.footerLeftLogo) || Boolean(layout.footerLeftText))) ||
    (showFooterCenter && (Boolean(logos.footerCenterLogo) || Boolean(layout.footerCenterText))) ||
    (showFooterRight && (Boolean(logos.footerRightLogo) || Boolean(layout.footerRightText)));

  return (
    <div
      className='relative form-pdf bg-white'
      style={{
        width: '794px',
        maxWidth: '794px',
        boxSizing: 'border-box',
        overflow: 'hidden',
      }}
    >
      <div className='w-[500px] mx-auto pl-10 pt-2 mb-1 text-center'>
        {centerLogoSrc && (
          <div className='flex justify-center mb-1'>
            <img
              src={centerLogoSrc}
              alt='centerLogo'
              style={{ maxHeight: 26, maxWidth: 130, objectFit: 'contain' }}
            />
          </div>
        )}
        <p className='text-[14px] leading-[15px] font-bold tracking-[-0.08em]'>{compName}</p>
        <div className='text-[12.5px] leading-[14px] font-normal tracking-[-0.08em]'>
          {compAddr &&
            (typeof compAddr === 'string' ? (
              (compAddr.includes('\n')
                ? compAddr
                : compAddr.replace(/(,?\s*)(Hoa Binh Ward|Xã An Lư)/i, '\n$2')
              )
                .split('\n')
                .map((line: string, idx: number) => <p key={idx}>{line.trim()}</p>)
            ) : (
              <p>{compAddr}</p>
            ))}
          {compPhone && <p>{compPhone}</p>}
          {layout.showTaxCode && layout.taxCode && (
            <p className='font-semibold'>Tax Code (MST): {layout.taxCode}</p>
          )}
        </div>
      </div>
      <div className='w-[425px] pl-10 mx-auto mb-1 text-center'>
        <p className='text-[14px] leading-[16px] font-bold tracking-[-0.08em]'>{headerTitle}</p>
        <div className='tracking-[-0.08em]'>
          <div>
            {subHeaderTitle && (
              <p className='text-[13.5px] leading-[15px] font-bold'>{subHeaderTitle}</p>
            )}
            {copyLabel && <p className='text-[12.5px] leading-[14px] font-medium'>{copyLabel}</p>}
          </div>
          <div className='text-12-18 text-code text-start font-medium absolute top-[80px] right-[75px] tracking-[0.0025em]'>
            <p>{documentCode}</p>
            <p>
              Serial No (số):{' '}
              <span className='text-red-500 text-16-16 tracking-[2px] font-bold'>
                {record?.serialNumber}
              </span>
            </p>
          </div>
        </div>
      </div>

      {/* Header Right: Decorative Stripes OR Custom Right Logo */}
      {layout.headerRightMode === 'LOGO' && rightLogoSrc ? (
        <img
          src={rightLogoSrc}
          alt='headerRightLogo'
          className='absolute top-3 right-8 max-w-[115px] max-h-[46px] object-contain'
        />
      ) : layout.headerRightMode !== 'NONE' ? (
        <img src={exportItem} alt='exportItem' className='absolute top-0 right-0 w-[180px]' />
      ) : null}

      {/* Header Left Logo */}
      {leftLogoSrc && (
        <img
          src={leftLogoSrc}
          alt='exportIc'
          className='absolute top-4 left-[44px] w-[113px] max-h-[46px] object-contain'
        />
      )}

      <div className='mx-4'>
        <div className='ml-4 mb-1'>
          <div className='flex text-[11px] leading-[18px] pr-1'>
            <p className='w-[150px]'>Tenant Name (Tên khách hàng):</p>
            <span className='flex-1 result-pdf relative pl-2 font-semibold'>
              {record?.measurementPoint?.customer?.fullName}
            </span>
          </div>
          <div className='flex text-[11px] leading-[18px] pr-1'>
            <p className='w-[150px]'>Meter ID (Mã điểm đo):</p>
            <span className='flex-1 result-pdf relative pl-2 font-semibold'>
              {record?.measurementPoint?.code}
            </span>
          </div>
          <Row>
            <Col span={8} className='flex text-[11px] leading-[18px] pr-4'>
              <p>Reading Time (Giờ ghi):</p>
              <span className='result-pdf relative flex-1 text-left pl-2 font-semibold'>
                {readingTimeVal}
              </span>
            </Col>
            <Col span={8} className='flex text-[11px] leading-[18px] pr-4'>
              <p>Reading Date (Ngày ghi):</p>
              <span className='result-pdf relative flex-1 text-left pl-2 font-semibold'>
                {readingDate}
              </span>
            </Col>
            <Col span={8} className='flex text-[11px] leading-[18px] pr-1'>
              <p>For month (Tháng):</p>
              <span className='result-pdf relative flex-1 text-left pl-2 font-semibold'>
                {forMonthVal}
              </span>
            </Col>
          </Row>
        </div>

        <div>
          <TablePDFCommon
            className='table-pdf'
            columns={columns}
            dataSource={allRows}
            bordered
            pagination={false}
            scroll={{ x: '100%' }}
          />
        </div>
        <div>
          {enabledCols.has('PMAX') && (
            <div className='my-0.5 ml-[10px] text-[11px] leading-[15px] font-bold text-gray-800'>
              * Công suất cực đại Pmax: {formatNumberWithCommas(record?.pmax, 2)} kW, ghi nhận hồi:{' '}
              {localPmaxTime}
            </div>
          )}
          <div className='my-0.5 ml-[10px] text-[11px] leading-[12px] tracking-[0.0025em]'>
            <p>The above meter reading has been jointly read by the following Parties:</p>
            <p> (Các chỉ số trên đã được các Bên dưới đây xác nhận)</p>
          </div>
          <div className='flex gap-14 w-full mx-auto'>
            {requireDualSignatures ? (
              <>
                <div
                  className='py-[4px] px-[6px] flex-1 text-[11px] leading-[12px]'
                  style={{ border: '1px solid #000000' }}
                >
                  <div className='mb-1.5'>
                    {partyALines.map((line, idx) => (
                      <p key={idx} className={idx === 0 ? 'font-semibold' : ''}>
                        {line}
                      </p>
                    ))}
                  </div>
                  <div>
                    <p>Name and Signature</p>
                    <p>(Ký và ghi rõ họ tên)</p>
                  </div>
                </div>
                <div
                  className='py-[4px] px-[6px] flex-1 text-[11px] leading-[12px]'
                  style={{ border: '1px solid #000000' }}
                >
                  <div className='mb-1.5'>
                    {partyBLines.map((line, idx) => (
                      <p key={idx} className={idx === 0 ? 'font-semibold' : ''}>
                        {line}
                      </p>
                    ))}
                  </div>
                  <div>
                    <p>Name and Signature</p>
                    <p>(Ký và ghi rõ họ tên)</p>
                  </div>
                </div>
              </>
            ) : (
              <div className='flex justify-end w-full'>
                <div
                  className='py-[4px] px-[6px] w-[48%] text-[11px] leading-[12px]'
                  style={{ border: '1px solid #000000' }}
                >
                  <div className='mb-1.5'>
                    {partyALines.map((line, idx) => (
                      <p key={idx} className={idx === 0 ? 'font-semibold' : ''}>
                        {line}
                      </p>
                    ))}
                  </div>
                  <div>
                    <p>Name and Signature</p>
                    <p>(Ký và ghi rõ họ tên)</p>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Custom Footer Left / Center / Right */}
          {hasFooter && (
            <div className='mt-1 pt-0.5 border-t border-gray-200 flex justify-between items-center text-[10px] text-gray-600 px-1'>
              <div className='flex items-center gap-2 text-left flex-1'>
                {showFooterLeft && logos.footerLeftLogo && (
                  <img
                    src={formatBase64Src(logos.footerLeftLogo)}
                    alt='footerLeft'
                    style={{ maxHeight: 20, maxWidth: 80, objectFit: 'contain' }}
                  />
                )}
                {showFooterLeft && layout.footerLeftText && <span>{layout.footerLeftText}</span>}
              </div>
              <div className='flex flex-col items-center justify-center text-center flex-1'>
                {showFooterCenter && logos.footerCenterLogo && (
                  <img
                    src={formatBase64Src(logos.footerCenterLogo)}
                    alt='footerCenter'
                    style={{ maxHeight: 20, maxWidth: 80, objectFit: 'contain' }}
                  />
                )}
                {showFooterCenter && layout.footerCenterText && <span>{layout.footerCenterText}</span>}
              </div>
              <div className='flex items-center justify-end gap-2 text-right flex-1'>
                {showFooterRight && layout.footerRightText && <span>{layout.footerRightText}</span>}
                {showFooterRight && logos.footerRightLogo && (
                  <img
                    src={formatBase64Src(logos.footerRightLogo)}
                    alt='footerRight'
                    style={{ maxHeight: 20, maxWidth: 80, objectFit: 'contain' }}
                  />
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TemplateNotice;
