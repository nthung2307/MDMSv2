import { Modal } from 'antd';

import { CheckboxCommon } from '@/infrastructure/common/components/Checkbox/checkbox-common';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import { notifyError, notifySuccess, notifyWarning } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useExportFileCsvHook from '@/infrastructure/repository/export/hooks/useExportFileCsv.hook';
import RequestService from '@/infrastructure/services/request.service';
import { ExcelColumn, ExcelHeaderGroup, ChartSheetItem, exportToExcelWithChart } from '@/infrastructure/utils/excelHelper';
import { _t } from '@/infrastructure/utils/translation';

interface itemHeader {
  label: string;
  attr: string;
  activeMsg?: string;
  inactiveMsg?: string;
  width?: number;
  align?: 'left' | 'center' | 'right';
  numFmt?: string;
}

type Props = {
  open: boolean;
  title?: string;
  handleCancel: () => void;
  dataSource?: any[];
  listHeader: itemHeader[];
  fileNameCsv: string;
  chartRef?: HTMLElement | null;
  chartSheets?: ChartSheetItem[];
  format?: any;
  onlyPDF?: boolean;
  isHtml2Pdf?: boolean;
  typeExportAll: string;
  subTypeExportAll?: string;
  paramsId?: any;
  requestBody?: any;
  serverExcelEndpoint?: string;
  onlyExportAll?: boolean;
  totalRow?: any;
  summaryRows?: Array<Record<string, any>>;
  extraInfo?: Array<{ label: string; value: string }>;
  headerGroups?: ExcelHeaderGroup[];
};

const DialogExportForm = (props: Props) => {
  const {
    title,
    handleCancel,
    open,
    dataSource = [],
    listHeader = [],
    fileNameCsv,
    chartRef,
    chartSheets,
    typeExportAll,
    subTypeExportAll,
    paramsId,
    requestBody,
    serverExcelEndpoint,
    onlyExportAll = false,
    totalRow,
    summaryRows,
    extraInfo = [],
    headerGroups: customHeaderGroups,
  } = props;

  const [data, setData] = useStateDataForm({
    type: 'EXCEL',
    quanlity: 0,
  });
  const [exportAllCsv] = useExportFileCsvHook();

  /**
   * Tự động phát hiện các nhóm Header 2 tầng (ví dụ Kênh giao, Kênh nhận)
   */
  const processColumnsAndGroups = (
    inputCols: ExcelColumn[]
  ): { columns: ExcelColumn[]; groups: ExcelHeaderGroup[] } => {
    if (customHeaderGroups && customHeaderGroups.length > 0) {
      return { columns: inputCols, groups: customHeaderGroups };
    }

    const groups: ExcelHeaderGroup[] = [];
    const cleanedCols: ExcelColumn[] = [];

    let currentGroupType: 'giao' | 'nhan' | null = null;
    let groupStart = -1;

    inputCols.forEach((col, idx) => {
      const colNum = idx + 1;
      const hLower = col.header.toLowerCase();

      const isGiao = hLower.includes('kênh giao') || hLower.includes('chiều giao');
      const isNhan = hLower.includes('kênh nhận') || hLower.includes('chiều nhận');

      if (isGiao) {
        if (currentGroupType !== 'giao') {
          if (currentGroupType && groupStart !== -1) {
            groups.push({
              title: currentGroupType === 'giao' ? 'KÊNH GIAO (kWh)' : 'KÊNH NHẬN (kWh)',
              startCol: groupStart,
              endCol: colNum - 1,
            });
          }
          currentGroupType = 'giao';
          groupStart = colNum;
        }
        const cleanedHeader = col.header
          .replace(/^((sản\s*lượng\s*)?(kênh|chiều)\s*giao(\s*\(kwh\))?\s*[-–:]\s*)/i, '')
          .trim();
        cleanedCols.push({ ...col, header: cleanedHeader || col.header });
      } else if (isNhan) {
        if (currentGroupType !== 'nhan') {
          if (currentGroupType && groupStart !== -1) {
            groups.push({
              title: currentGroupType === 'giao' ? 'KÊNH GIAO (kWh)' : 'KÊNH NHẬN (kWh)',
              startCol: groupStart,
              endCol: colNum - 1,
            });
          }
          currentGroupType = 'nhan';
          groupStart = colNum;
        }
        const cleanedHeader = col.header
          .replace(/^((sản\s*lượng\s*)?(kênh|chiều)\s*nhận(\s*\(kwh\))?\s*[-–:]\s*)/i, '')
          .trim();
        cleanedCols.push({ ...col, header: cleanedHeader || col.header });
      } else {
        if (currentGroupType && groupStart !== -1) {
          groups.push({
            title: currentGroupType === 'giao' ? 'KÊNH GIAO (kWh)' : 'KÊNH NHẬN (kWh)',
            startCol: groupStart,
            endCol: colNum - 1,
          });
          currentGroupType = null;
          groupStart = -1;
        }
        cleanedCols.push(col);
      }
    });

    if (currentGroupType && groupStart !== -1) {
      groups.push({
        title: currentGroupType === 'giao' ? 'KÊNH GIAO (kWh)' : 'KÊNH NHẬN (kWh)',
        startCol: groupStart,
        endCol: inputCols.length,
      });
    }

    return { columns: cleanedCols, groups };
  };

  /**
   * Chuyển đổi listHeader sang ExcelColumn
   */
  const buildExcelColumns = (headers: itemHeader[]): ExcelColumn[] => {
    return headers.map((h) => ({
      header: h.label,
      key: h.attr,
      width: h.width || Math.max(h.label.length * 1.5, 14),
      align:
        h.align ||
        (h.attr.toLowerCase() === 'stt' || h.attr.toLowerCase() === 'index'
          ? 'center'
          : h.attr.toLowerCase().includes('time') || h.attr.toLowerCase().includes('date')
          ? 'center'
          : 'left'),
      numFmt: h.numFmt,
      activeMsg: h.activeMsg,
      inactiveMsg: h.inactiveMsg,
    }));
  };

  /**
   * Parse CSV trả về từ API Backend thành mảng đối tượng
   */
  const parseCsvToTableData = (csvText: string, headersConfig: itemHeader[] = []) => {
    const lines = csvText.split(/\r?\n/).filter((l) => l.trim() !== '');
    if (lines.length === 0) return { columns: [], rows: [] };

    const cleanVal = (val: string) => {
      let v = val.trim();
      if (v.startsWith('"') && v.endsWith('"') && v.length >= 2) {
        v = v.slice(1, -1).replace(/""/g, '"').trim();
      }
      return v;
    };

    const parseLine = (line: string): string[] => {
      const result: string[] = [];
      let cur = '';
      let insideQuote = false;
      for (let i = 0; i < line.length; i++) {
        const c = line[i];
        if (c === '"') {
          if (insideQuote && line[i + 1] === '"') {
            cur += '"';
            i++;
          } else {
            insideQuote = !insideQuote;
          }
        } else if (c === ',' && !insideQuote) {
          result.push(cleanVal(cur));
          cur = '';
        } else {
          cur += c;
        }
      }
      result.push(cleanVal(cur));
      return result;
    };

    const headerLine = lines[0].replace(/^\uFEFF/, '');
    const headerLabels = parseLine(headerLine);
    const columns: ExcelColumn[] = headerLabels.map((lbl, idx) => ({
      header: lbl || `Cột ${idx + 1}`,
      key: headersConfig[idx]?.attr || `col_${idx}`,
      width: Math.max(lbl.length * 1.5, 15),
    }));

    const rows = lines.slice(1).map((line, rIdx) => {
      const vals = parseLine(line);
      const rowObj: Record<string, any> = {};
      vals.forEach((v, idx) => {
        rowObj[`col_${idx}`] = v;
        if (headerLabels[idx]) {
          rowObj[headerLabels[idx]] = v;
        }
        if (headersConfig[idx]?.attr) {
          rowObj[headersConfig[idx].attr] = v;
        }
        const headerLbl = (headerLabels[idx] || '').trim().toLowerCase();
        const matched = headersConfig.find(
          (h) => h.label.trim().toLowerCase() === headerLbl
        );
        if (matched) {
          rowObj[matched.attr] = v;
        }
      });
      if (rowObj['index'] === undefined || rowObj['index'] === '') {
        rowObj['index'] = rIdx + 1;
      }
      if (rowObj['stt'] === undefined || rowObj['stt'] === '') {
        rowObj['stt'] = rIdx + 1;
      }
      return rowObj;
    });

    return { columns, rows };
  };

  /**
   * Xuất Excel trang hiện tại (kèm biểu đồ nếu có)
   */
  const exportCurrentPageExcel = async () => {
    try {
      const rawColumns = buildExcelColumns(listHeader);
      const { columns, groups } = processColumnsAndGroups(rawColumns);

      await exportToExcelWithChart({
        title: title || 'Báo cáo dữ liệu',
        fileName: fileNameCsv,
        columns,
        data: dataSource,
        chartElement: chartRef,
        chartSheets,
        totalRow,
        summaryRows,
        subTitle: `Thời gian xuất báo cáo: ${new Date().toLocaleString('vi-VN')}`,
        extraInfo,
        headerGroups: groups,
      });
      notifySuccess('Tải tệp Excel thành công!');
    } catch (err: any) {
      notifyError(err, 'Lỗi khi xuất tệp Excel!');
    }
  };

  /**
   * Xuất Excel cho tất cả dữ liệu (qua API Backend và kèm biểu đồ nếu có)
   */
  const exportAllExcelAsync = async () => {
    // Nếu trang hiện tại đã có đầy đủ dữ liệu (ví dụ báo cáo đã load hết item)
    if (
      dataSource &&
      dataSource.length > 0 &&
      listHeader.length > 0 &&
      (!typeExportAll || typeExportAll === '')
    ) {
      await exportCurrentPageExcel();
      return;
    }

    await exportAllCsv(
      typeExportAll,
      async (res: any) => {
        try {
          const text = typeof res === 'string' ? res : await (res as Blob).text();
          if (!text || text.trim() === '') {
            notifyWarning('Không có dữ liệu để xuất!');
            return;
          }
          const { columns: parsedCols, rows } = parseCsvToTableData(text, listHeader);

          const inputCols =
            listHeader.length > 0
              ? buildExcelColumns(listHeader)
              : parsedCols.length > 0
              ? parsedCols
              : buildExcelColumns(listHeader);

          const finalRows = rows.length > 0 ? rows : dataSource;

          const { columns, groups } = processColumnsAndGroups(inputCols);

          await exportToExcelWithChart({
            title: title || 'Báo cáo dữ liệu',
            fileName: fileNameCsv,
            columns,
            data: finalRows,
            chartElement: chartRef,
            chartSheets,
            totalRow,
            summaryRows,
            subTitle: `Thời gian xuất báo cáo: ${new Date().toLocaleString('vi-VN')}`,
            extraInfo,
            headerGroups: groups,
          });
          notifySuccess('Tải tệp Excel thành công!');
        } catch (err: any) {
          notifyError(err, 'Có lỗi xảy ra khi tạo tệp Excel từ dữ liệu máy chủ!');
        }
      },
      async () => {
        // Fallback to export current page if server error
        if (dataSource && dataSource.length > 0) {
          notifyWarning('Không thể tải toàn bộ dữ liệu từ máy chủ, xuất dữ liệu trang hiện tại.');
          await exportCurrentPageExcel();
        }
      },
      () => {},
      subTypeExportAll,
      paramsId,
      requestBody
    );
  };

  const exportServerExcelAsync = async () => {
    try {
      const res: any = await new RequestService().makePostExportRequestAsync(
        serverExcelEndpoint!,
        requestBody || {}
      );
      const rawData = res?.data || res;
      const blob =
        rawData instanceof Blob
          ? rawData
          : new Blob([rawData], {
              type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });

      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.setAttribute('download', `${fileNameCsv || 'Export'}.xlsx`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(downloadUrl);
      notifySuccess('Tải tệp Excel Native Charts thành công!');
    } catch (err: any) {
      notifyError(err, 'Lỗi khi tải tệp từ máy chủ, chuyển sang tạo tệp từ trình duyệt...');
      await exportCurrentPageExcel();
    }
  };

  const onSubmitAction = async (_form: any) => {
    // Nếu có endpoint xuất tệp từ máy chủ (chứa Native Charts), tự động ưu tiên tải từ máy chủ
    if (serverExcelEndpoint) {
      await exportServerExcelAsync();
      return;
    }

    // Nếu có typeExportAll thì ưu tiên xuất toàn bộ kết quả từ máy chủ
    if (typeExportAll && typeExportAll.trim() !== '') {
      await exportAllExcelAsync();
    } else {
      await exportCurrentPageExcel();
    }
  };

  const fileTypeOptions = [{ label: 'Excel (.xlsx)', value: 'EXCEL' }];

  return (
    <Modal
      open={open}
      key={'f-export'}
      centered
      className='min-w-[380px]'
      closable={true}
      footer={false}
      onCancel={handleCancel}
    >
      <div className='modal space-y-4'>
        <div className='flex flex-col justify-between gap-4'>
          <p className='title text-center font-extrabold text-xl text-gray-800'>
            Xuất dữ liệu Excel
          </p>

          <InputSelectCommon
            label={_t('Định dạng tệp')}
            attribute={'type'}
            dataAttribute={data?.type}
            setData={setData}
            isRequired={false}
            listDataItem={fileTypeOptions}
            placeholder={_t('Định dạng tệp')}
          />

          <ButtonCommon
            title={_t('Tải xuống Excel')}
            colorClass={'firebrick-bg'}
            onClick={() => {
              onSubmitAction(data || null);
              handleCancel();
            }}
            classButton={'w-full'}
          />
        </div>
      </div>
    </Modal>
  );
};

export default DialogExportForm;
