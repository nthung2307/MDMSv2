import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import html2canvas from 'html2canvas';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  LineController,
  BarElement,
  BarController,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  LineController,
  BarElement,
  BarController,
  Title,
  Tooltip,
  Legend
);

export interface ExcelColumn {
  header: string;
  key: string;
  width?: number;
  align?: 'left' | 'center' | 'right';
  numFmt?: string;
  activeMsg?: string;
  inactiveMsg?: string;
}

export interface ExcelHeaderGroup {
  title: string;
  startCol: number; // 1-indexed column index
  endCol: number; // 1-indexed column index
}

export interface ChartSheetItem {
  sheetName: string;
  title: string;
  chartConfig?: {
    type: 'line' | 'bar';
    data: any;
    options?: any;
  };
  element?: HTMLElement | null;
}

export interface ExportExcelOptions {
  title: string;
  fileName: string;
  columns: ExcelColumn[];
  data: any[];
  chartElement?: HTMLElement | null;
  chartSheets?: ChartSheetItem[];
  totalRow?: any;
  summaryRows?: Array<Record<string, any>>;
  subTitle?: string;
  extraInfo?: Array<{ label: string; value: string }>;
  headerGroups?: ExcelHeaderGroup[];
}

/**
 * Chụp ảnh Canvas hoặc DOM element của biểu đồ (tự động bung phần tử nếu đang bị ẩn)
 */
async function captureChartImage(chartElement: HTMLElement): Promise<string | null> {
  const restoreStyles: Array<{
    el: HTMLElement;
    display: string;
    position: string;
    visibility: string;
    width: string;
    height: string;
    top: string;
    left: string;
    opacity: string;
  }> = [];

  try {
    let curr: HTMLElement | null = chartElement;
    while (curr && curr !== document.body) {
      const style = window.getComputedStyle(curr);
      if (style.display === 'none') {
        restoreStyles.push({
          el: curr,
          display: curr.style.display,
          position: curr.style.position,
          visibility: curr.style.visibility,
          width: curr.style.width,
          height: curr.style.height,
          top: curr.style.top,
          left: curr.style.left,
          opacity: curr.style.opacity,
        });
        curr.style.display = 'block';
        curr.style.position = 'fixed';
        curr.style.left = '-99999px';
        curr.style.top = '0px';
        curr.style.width = '1200px';
        curr.style.height = '600px';
        curr.style.visibility = 'visible';
        curr.style.opacity = '1';
      }
      curr = curr.parentElement;
    }

    if (restoreStyles.length > 0) {
      await new Promise((resolve) => setTimeout(resolve, 150));
    }

    const canvas =
      chartElement instanceof HTMLCanvasElement
        ? chartElement
        : (chartElement.querySelector('canvas') as HTMLCanvasElement | null);

    if (canvas) {
      const chartInst = ChartJS.getChart(canvas);
      if (chartInst) {
        try {
          const offCanvas = document.createElement('canvas');
          offCanvas.width = 1200;
          offCanvas.height = 600;
          const ctx = offCanvas.getContext('2d');
          if (ctx) {
            const clonedOptions = {
              ...(chartInst.config.options || {}),
              animation: false,
              responsive: false,
              maintainAspectRatio: false,
            };
            const tempChart = new ChartJS(ctx, {
              type: chartInst.config.type as any,
              data: chartInst.config.data,
              options: clonedOptions as any,
            });
            tempChart.update('none');
            const dataUrl = offCanvas.toDataURL('image/png');
            tempChart.destroy();
            if (dataUrl && dataUrl.length > 500) {
              return dataUrl;
            }
          }
        } catch (e) {
          console.warn('Offscreen chart render fallback:', e);
        }

        try {
          chartInst.resize(1200, 600);
          chartInst.update('none');
          const dataUrl = chartInst.toBase64Image();
          if (dataUrl && dataUrl.length > 500) {
            return dataUrl;
          }
        } catch (e) {
          console.warn('chartInst.toBase64Image fallback:', e);
        }
      }

      if (canvas.width > 20 && canvas.height > 20) {
        const dataUrl = canvas.toDataURL('image/png');
        if (dataUrl && dataUrl.length > 500) {
          return dataUrl;
        }
      }
    }

    try {
      const renderedCanvas = await html2canvas(chartElement, {
        scale: 2,
        useCORS: true,
        backgroundColor: '#ffffff',
      });
      const dataUrl = renderedCanvas.toDataURL('image/png');
      if (dataUrl && dataUrl.length > 500) {
        return dataUrl;
      }
    } catch (e) {
      console.warn('html2canvas error:', e);
    }

    return null;
  } catch (err) {
    console.error('Error capturing chart image:', err);
    return null;
  } finally {
    restoreStyles.forEach(({ el: rEl, display, position, visibility, width, height, top, left, opacity }) => {
      rEl.style.display = display;
      rEl.style.position = position;
      rEl.style.visibility = visibility;
      rEl.style.width = width;
      rEl.style.height = height;
      rEl.style.top = top;
      rEl.style.left = left;
      rEl.style.opacity = opacity;
    });
  }
}

/**
 * Render trực tiếp cấu hình ChartJS (data + options) lên canvas trong RAM không cần DOM
 */
function renderChartConfigToBase64(chartConfig: { type: string; data: any; options?: any }): string | null {
  try {
    const offCanvas = document.createElement('canvas');
    offCanvas.width = 1200;
    offCanvas.height = 600;
    const ctx = offCanvas.getContext('2d');
    if (!ctx) return null;

    const clonedOptions = {
      ...(chartConfig.options || {}),
      animation: false,
      responsive: false,
      maintainAspectRatio: false,
    };

    const tempChart = new ChartJS(ctx, {
      type: chartConfig.type as any,
      data: chartConfig.data,
      options: clonedOptions as any,
    });
    tempChart.update('none');
    const dataUrl = offCanvas.toDataURL('image/png');
    tempChart.destroy();
    return dataUrl;
  } catch (err) {
    console.warn('Lỗi khi render chartConfig sang base64:', err);
    return null;
  }
}

/**
 * Chèn một Worksheet Biểu đồ chuẩn vào Workbook Excel
 */
function addChartSheetToWorkbook(
  workbook: ExcelJS.Workbook,
  sheetName: string,
  chartTitle: string,
  imageBase64: string,
  extraInfo: Array<{ label: string; value: string }> = [],
  subTitle?: string
) {
  const cleanSheetName = sheetName.replace(/[\\/?*:[\]]/g, '').trim().substring(0, 31);
  const chartWorksheet = workbook.addWorksheet(cleanSheetName, {
    views: [{ showGridLines: true }],
  });

  // 1. Tiêu đề hệ thống trên Tab biểu đồ
  const cBrandRow = chartWorksheet.addRow([
    'HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)',
  ]);
  cBrandRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: 'FF555555' } };
  cBrandRow.alignment = { horizontal: 'center', vertical: 'middle' };
  chartWorksheet.mergeCells(1, 1, 1, 14);
  cBrandRow.height = 20;

  // 2. Tiêu đề chính biểu đồ
  const cleanTitle = chartTitle
    .replace(/^biểu\s*đồ(\s*trực\s*quan\s*hóa)?[:\s-]*/i, '')
    .trim();
  const cTitleRow = chartWorksheet.addRow([
    cleanTitle ? `BIỂU ĐỒ: ${cleanTitle.toUpperCase()}` : 'BIỂU ĐỒ',
  ]);
  cTitleRow.font = { name: 'Arial', size: 13, bold: true, color: { argb: 'FF0F4C81' } };
  cTitleRow.alignment = { horizontal: 'center', vertical: 'middle' };
  chartWorksheet.mergeCells(2, 1, 2, 14);
  cTitleRow.height = 26;

  let chartCurRow = 3;
  if (subTitle) {
    const cSubRow = chartWorksheet.addRow([subTitle]);
    cSubRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: 'FF333333' } };
    cSubRow.alignment = { horizontal: 'center', vertical: 'middle' };
    chartWorksheet.mergeCells(chartCurRow, 1, chartCurRow, 14);
    cSubRow.height = 18;
    chartCurRow++;
  }

  if (extraInfo.length > 0) {
    extraInfo.forEach((item) => {
      const cInfoRow = chartWorksheet.addRow([`${item.label}: ${item.value}`]);
      cInfoRow.font = { name: 'Arial', size: 10, bold: true, color: { argb: 'FF1F2937' } };
      chartWorksheet.mergeCells(chartCurRow, 1, chartCurRow, 14);
      cInfoRow.height = 18;
      chartCurRow++;
    });
  }

  // Chèn ảnh biểu đồ
  const imageId = workbook.addImage({
    base64: imageBase64,
    extension: 'png',
  });

  chartWorksheet.addImage(imageId, {
    tl: { col: 1, row: chartCurRow + 1 },
    ext: { width: 980, height: 480 },
  });
}

/**
 * Xuất dữ liệu bảng và hình ảnh biểu đồ tương ứng ra tệp Excel (.xlsx) chuẩn OpenXML
 */
export const exportToExcelWithChart = async (options: ExportExcelOptions) => {
  const {
    title,
    fileName,
    columns,
    data,
    chartElement,
    chartSheets = [],
    totalRow,
    summaryRows = [],
    subTitle,
    extraInfo = [],
    headerGroups = [],
  } = options;

  const workbook = new ExcelJS.Workbook();
  workbook.creator = 'MDMS AMR System';
  workbook.created = new Date();

  const worksheet = workbook.addWorksheet('Báo cáo', {
    views: [{ showGridLines: true }],
  });

  const totalCols = Math.max(columns.length, 6);

  // 1. Tiêu đề hệ thống
  const brandRow = worksheet.addRow(['HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)']);
  brandRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: 'FF555555' } };
  brandRow.alignment = { horizontal: 'center', vertical: 'middle' };
  worksheet.mergeCells(1, 1, 1, totalCols);
  brandRow.height = 20;

  // 2. Tiêu đề chính báo cáo
  const titleRow = worksheet.addRow([title.toUpperCase()]);
  titleRow.font = { name: 'Arial', size: 14, bold: true, color: { argb: 'FF0F4C81' } };
  titleRow.alignment = { horizontal: 'center', vertical: 'middle' };
  worksheet.mergeCells(2, 1, 2, totalCols);
  titleRow.height = 28;

  // 3. Phụ đề hoặc thời gian xuất
  let currentRowIndex = 3;
  if (subTitle) {
    const subRow = worksheet.addRow([subTitle]);
    subRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: 'FF333333' } };
    subRow.alignment = { horizontal: 'center', vertical: 'middle' };
    worksheet.mergeCells(currentRowIndex, 1, currentRowIndex, totalCols);
    subRow.height = 20;
    currentRowIndex++;
  }

  // 4. Các thông tin bổ sung (Khách hàng, Điểm đo, Kỳ chốt...)
  if (extraInfo.length > 0) {
    extraInfo.forEach((item) => {
      const infoRow = worksheet.addRow([`${item.label}: ${item.value}`]);
      infoRow.font = { name: 'Arial', size: 10, bold: true, color: { argb: 'FF1F2937' } };
      worksheet.mergeCells(currentRowIndex, 1, currentRowIndex, totalCols);
      infoRow.height = 18;
      currentRowIndex++;
    });
  }

  // Cách 1 dòng trống trước bảng
  worksheet.addRow([]);
  currentRowIndex++;

  // 5. Header dòng bảng dữ liệu (Hỗ trợ 2 tầng nếu có headerGroups)
  const hasGroupHeaders = headerGroups && headerGroups.length > 0;

  if (hasGroupHeaders) {
    const headerRowStart = currentRowIndex;
    const row1Values = new Array(columns.length).fill('');
    const row2Values = new Array(columns.length).fill('');

    columns.forEach((col, idx) => {
      const colNum = idx + 1;
      const group = headerGroups.find((g) => colNum >= g.startCol && colNum <= g.endCol);
      if (group) {
        if (colNum === group.startCol) {
          row1Values[idx] = group.title;
        }
        row2Values[idx] = col.header;
      } else {
        row1Values[idx] = col.header;
        row2Values[idx] = '';
      }
    });

    const headerRow1 = worksheet.addRow(row1Values);
    headerRow1.height = 26;
    const headerRow2 = worksheet.addRow(row2Values);
    headerRow2.height = 26;

    // Merge các ô theo nhóm và theo cột độc lập
    headerGroups.forEach((g) => {
      worksheet.mergeCells(headerRowStart, g.startCol, headerRowStart, g.endCol);
    });

    columns.forEach((_, idx) => {
      const colNum = idx + 1;
      const isGrouped = headerGroups.some((g) => colNum >= g.startCol && colNum <= g.endCol);
      if (!isGrouped) {
        worksheet.mergeCells(headerRowStart, colNum, headerRowStart + 1, colNum);
      }
    });

    // Định dạng style cho cả 2 hàng Header
    [headerRow1, headerRow2].forEach((hr) => {
      hr.eachCell((cell) => {
        cell.fill = {
          type: 'pattern',
          pattern: 'solid',
          fgColor: { argb: 'FF1F4E79' }, // Màu xanh Navy chuẩn EVN
        };
        cell.font = {
          name: 'Arial',
          size: 10,
          bold: true,
          color: { argb: 'FFFFFFFF' },
        };
        cell.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        cell.border = {
          top: { style: 'thin', color: { argb: 'FF000000' } },
          left: { style: 'thin', color: { argb: 'FF000000' } },
          bottom: { style: 'thin', color: { argb: 'FF000000' } },
          right: { style: 'thin', color: { argb: 'FF000000' } },
        };
      });
    });

    currentRowIndex += 2;
  } else {
    const headerValues = columns.map((col) => col.header);
    const headerRow = worksheet.addRow(headerValues);
    headerRow.height = 26;
    headerRow.eachCell((cell) => {
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FF1F4E79' }, // Màu xanh Navy chuẩn EVN
      };
      cell.font = {
        name: 'Arial',
        size: 10,
        bold: true,
        color: { argb: 'FFFFFFFF' },
      };
      cell.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
      cell.border = {
        top: { style: 'thin', color: { argb: 'FF000000' } },
        left: { style: 'thin', color: { argb: 'FF000000' } },
        bottom: { style: 'thin', color: { argb: 'FF000000' } },
        right: { style: 'thin', color: { argb: 'FF000000' } },
      };
    });
    currentRowIndex++;
  }

  // 6. Các dòng dữ liệu
  data.forEach((item, rIdx) => {
    const rowValues = columns.map((col) => {
      const val = item[col.key];
      if (val === undefined || val === null) return '';

      const isStt =
        col.key.toLowerCase() === 'stt' ||
        col.key.toLowerCase() === 'index' ||
        col.header.trim().toUpperCase() === 'STT';

      const isIntegerCol =
        isStt ||
        col.key.toLowerCase() === 'periodnumber' ||
        col.header.trim().toLowerCase() === 'số kì' ||
        col.header.trim().toLowerCase() === 'số kỳ';

      if (isIntegerCol) {
        if (typeof val === 'number') return Math.round(val);
        const parsedInt = parseInt(String(val), 10);
        return isNaN(parsedInt) ? val : parsedInt;
      }

      const isValueCol =
        col.key.toLowerCase() === 'value' ||
        col.header.trim().toLowerCase() === 'giá trị';

      if (isValueCol) {
        if (val === true || val === 'True' || val === 'true' || val === 1 || val === '1') return 1;
        return 0;
      }

      if (col.activeMsg && col.inactiveMsg) {
        const isAct =
          val === true ||
          val === 'ACTIVE' ||
          val === 'active' ||
          val === 1 ||
          val === '1' ||
          val === col.activeMsg;
        return isAct ? col.activeMsg : col.inactiveMsg;
      }

      // Nếu là chuỗi có chứa '*' (số liệu nghi ngờ) -> giữ nguyên chuỗi
      if (typeof val === 'string' && val.includes('*')) {
        return val;
      }

      // Nếu là số hợp lệ
      if (typeof val === 'number') return val;
      const parsed = Number(val);
      if (
        !isNaN(parsed) &&
        String(val).trim() !== '' &&
        col.key !== 'timeLabel' &&
        col.key !== 'code' &&
        col.key !== 'customerCode' &&
        col.key !== 'measurementPointCode' &&
        col.key !== 'phoneNumber' &&
        !col.key.toLowerCase().includes('phone') &&
        !col.key.toLowerCase().includes('sdt')
      ) {
        return parsed;
      }
      return val;
    });

    const dataRow = worksheet.addRow(rowValues);
    dataRow.height = 20;

    const isEven = rIdx % 2 === 1;
    dataRow.eachCell((cell, colNumber) => {
      const colDef = columns[colNumber - 1];
      const isStt =
        colDef?.key.toLowerCase() === 'stt' ||
        colDef?.key.toLowerCase() === 'index' ||
        colDef?.header.trim().toUpperCase() === 'STT';

      const isIntegerCol =
        isStt ||
        colDef?.key.toLowerCase() === 'periodnumber' ||
        colDef?.header.trim().toLowerCase() === 'số kì' ||
        colDef?.header.trim().toLowerCase() === 'số kỳ';

      const isValueCol =
        colDef?.key.toLowerCase() === 'value' ||
        colDef?.header.trim().toLowerCase() === 'giá trị';

      let align = colDef?.align;
      if (!align) {
        if (isIntegerCol || isValueCol) align = 'center';
        else if (typeof cell.value === 'number') align = 'right';
        else if (typeof cell.value === 'string' && cell.value.includes('*')) align = 'right';
        else align = 'left';
      }

      cell.font = { name: 'Arial', size: 10 };
      cell.alignment = { horizontal: align, vertical: 'middle' };
      cell.border = {
        top: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        left: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        bottom: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        right: { style: 'thin', color: { argb: 'FFD3D3D3' } },
      };

      if (isEven) {
        cell.fill = {
          type: 'pattern',
          pattern: 'solid',
          fgColor: { argb: 'FFF9FAFB' },
        };
      }

      // Format số
      if (typeof cell.value === 'number') {
        if (isIntegerCol) {
          cell.numFmt = '0'; // Số nguyên, không có số thập phân
        } else if (colDef?.numFmt) {
          cell.numFmt = colDef.numFmt;
        } else if (
          colDef?.key === 'value' ||
          colDef?.header.trim().toLowerCase() === 'giá trị' ||
          colDef?.key === 'periodNumber' ||
          colDef?.header.trim().toLowerCase() === 'số kì' ||
          colDef?.header.trim().toLowerCase() === 'số kỳ'
        ) {
          cell.numFmt = '0'; // Cột Số kì / Giá trị: chỉ hiển thị số nguyên, không có hàng thập phân
        } else {
          cell.numFmt = '#,##0.00';
        }
      }
    });
    currentRowIndex++;
  });

  // 7. Dòng tổng kết (Summary: Tổng, Min, Max hoặc dòng Tổng cộng)
  const rowsToRender: Array<{ row: Record<string, any>; type: 'sum' | 'min' | 'max' | 'other' }> = [];

  if (summaryRows && summaryRows.length > 0) {
    summaryRows.forEach((sr) => {
      const label = String(sr.stt || sr.title || sr.label || '').toLowerCase();
      let type: 'sum' | 'min' | 'max' | 'other' = 'other';
      if (label.includes('tổng') || label.includes('sum')) type = 'sum';
      else if (label.includes('min')) type = 'min';
      else if (label.includes('max')) type = 'max';
      rowsToRender.push({ row: sr, type });
    });
  } else if (totalRow) {
    rowsToRender.push({ row: totalRow, type: 'sum' });
  }

  rowsToRender.forEach(({ row: sRow, type }, sIdx) => {
    const isLastSummary = sIdx === rowsToRender.length - 1;
    const summaryLabel =
      sRow.stt ||
      sRow.title ||
      sRow.label ||
      (type === 'sum' ? 'Tổng' : type === 'min' ? 'Min' : type === 'max' ? 'Max' : 'Tổng cộng');

    const rowValues = columns.map((col, cIdx) => {
      // Cột đầu tiên (STT): Chỉ hiển thị nhãn "Tổng", "Min", "Max"
      if (cIdx === 0) return summaryLabel;

      // Cột thời gian hoặc ngày tháng: Tuyệt đối để trống "" để không bị in đè nhãn
      const colKeyLower = col.key.toLowerCase();
      if (
        colKeyLower === 'timelabel' ||
        colKeyLower === 'createdtime' ||
        colKeyLower.includes('time') ||
        colKeyLower.includes('date')
      ) {
        return '';
      }

      const val = sRow[col.key];
      if (val === undefined || val === null || val === '') return '';
      if (typeof val === 'number') return val;
      const parsed = Number(val);
      if (!isNaN(parsed)) return parsed;
      return val;
    });

    const summaryDataRow = worksheet.addRow(rowValues);
    summaryDataRow.height = 22;

    // Chọn định dạng ĐƠN SẮC (Monochrome) chuẩn báo cáo tài chính/kỹ thuật
    let fgColor = 'FFF2F4F7'; // Xám nhạt thanh lịch cho Tổng
    let fontColor = 'FF101828'; // Đen đậm
    let borderColor = 'FF98A2B3'; // Xám viền

    if (type === 'min' || type === 'max') {
      fgColor = 'FFFFFFFF'; // Nền trắng đơn sắc
      fontColor = 'FF344054'; // Xám đậm
      borderColor = 'FFD0D5DD'; // Viền mỏng
    }

    summaryDataRow.eachCell((cell, colNumber) => {
      cell.font = { name: 'Arial', size: 10, bold: true, color: { argb: fontColor } };
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: fgColor },
      };
      const colDef = columns[colNumber - 1];
      const isDash = cell.value === '-';
      const align =
        colNumber === 1 || colNumber === 2 || isDash ? 'center' : colDef?.align || 'right';
      cell.alignment = { horizontal: align, vertical: 'middle' };
      cell.border = {
        top: { style: 'thin', color: { argb: borderColor } },
        left: { style: 'thin', color: { argb: borderColor } },
        bottom: { style: isLastSummary ? 'double' : 'thin', color: { argb: isLastSummary ? 'FF101828' : borderColor } },
        right: { style: 'thin', color: { argb: borderColor } },
      };
      if (typeof cell.value === 'number') {
        cell.numFmt = colDef?.numFmt || '#,##0';
      }
    });
    currentRowIndex++;
  });

  // 8. Tự động căn chỉnh độ rộng cột khoa học, phù hợp với từng loại dữ liệu
  columns.forEach((col, idx) => {
    const isStt =
      col.key.toLowerCase() === 'stt' ||
      col.key.toLowerCase() === 'index' ||
      col.header.trim().toUpperCase() === 'STT';

    if (isStt) {
      worksheet.getColumn(idx + 1).width = 7;
      return;
    }

    if (col.key.toLowerCase().includes('time') || col.key.toLowerCase().includes('date')) {
      worksheet.getColumn(idx + 1).width = 18;
      return;
    }

    if (
      col.key.toLowerCase().includes('pointcode') ||
      col.header.toLowerCase().includes('điểm đo') ||
      col.key.toLowerCase().includes('customercode')
    ) {
      worksheet.getColumn(idx + 1).width = 22;
      return;
    }

    // Các cột số liệu đo đếm điện năng
    if (
      col.key.toLowerCase().includes('chiso') ||
      col.key.toLowerCase().includes('sanluong') ||
      col.header.toLowerCase().includes('tổng') ||
      col.header.toLowerCase().includes('biểu') ||
      col.header.toLowerCase().includes('vc')
    ) {
      worksheet.getColumn(idx + 1).width = 15;
      return;
    }

    if (col.width) {
      worksheet.getColumn(idx + 1).width = col.width;
      return;
    }

    let maxLen = col.header ? col.header.length * 1.3 : 10;
    data.slice(0, 50).forEach((item) => {
      const val = item[col.key];
      if (val !== undefined && val !== null) {
        const len = String(val).length;
        if (len > maxLen) maxLen = len;
      }
    });
    worksheet.getColumn(idx + 1).width = Math.max(14, Math.min(Math.ceil(maxLen) + 3, 35));
  });

  // 9. Chèn hình ảnh biểu đồ vào các Sheet riêng biệt
  if (chartSheets && chartSheets.length > 0) {
    for (const sheetItem of chartSheets) {
      try {
        let base64: string | null = null;
        if (sheetItem.chartConfig) {
          base64 = renderChartConfigToBase64(sheetItem.chartConfig);
        }
        if (!base64 && sheetItem.element) {
          base64 = await captureChartImage(sheetItem.element);
        }
        if (base64 && base64.length > 500) {
          addChartSheetToWorkbook(
            workbook,
            sheetItem.sheetName,
            sheetItem.title,
            base64,
            extraInfo,
            subTitle
          );
        }
      } catch (cErr) {
        console.warn(`Lỗi khi tạo sheet biểu đồ ${sheetItem.sheetName}:`, cErr);
      }
    }
  } else if (chartElement) {
    // Fallback: nếu chỉ truyền 1 chartElement duy nhất
    try {
      const chartImageBase64 = await captureChartImage(chartElement);
      if (chartImageBase64 && chartImageBase64.length > 500) {
        addChartSheetToWorkbook(
          workbook,
          'Biểu đồ',
          title,
          chartImageBase64,
          extraInfo,
          subTitle
        );
      }
    } catch (chartErr) {
      console.warn('Không thể chụp hình ảnh biểu đồ:', chartErr);
    }
  }

  // 10. Ghi và tải file Excel
  const buffer = await workbook.xlsx.writeBuffer();
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
  const finalFileName = fileName.toLowerCase().endsWith('.xlsx')
    ? fileName
    : `${fileName.replace(/\.csv$/i, '')}.xlsx`;

  saveAs(blob, finalFileName);
};

/**
 * Tải file mẫu Excel (.xlsx) với định dạng bảng và màu sắc chuẩn hóa
 */
export const downloadExcelTemplate = async (
  templateFileName: string,
  sheetName: string,
  columns: Array<{ header: string; key: string; width?: number }>,
  sampleRows: any[]
) => {
  const workbook = new ExcelJS.Workbook();
  const worksheet = workbook.addWorksheet(sheetName, {
    views: [{ showGridLines: true }],
  });

  // Hàng Header
  const headerRow = worksheet.addRow(columns.map((col) => col.header));
  headerRow.height = 25;
  headerRow.eachCell((cell) => {
    cell.fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FF1F4E79' },
    };
    cell.font = { name: 'Arial', size: 10, bold: true, color: { argb: 'FFFFFFFF' } };
    cell.alignment = { horizontal: 'center', vertical: 'middle' };
    cell.border = {
      top: { style: 'thin' },
      left: { style: 'thin' },
      bottom: { style: 'thin' },
      right: { style: 'thin' },
    };
  });

  // Hàng dữ liệu mẫu
  sampleRows.forEach((row, idx) => {
    const rowValues = columns.map((col) => row[col.key] ?? '');
    const dataRow = worksheet.addRow(rowValues);
    dataRow.height = 20;
    dataRow.eachCell((cell, colNum) => {
      cell.font = { name: 'Arial', size: 10 };
      cell.alignment = {
        horizontal: colNum === 1 ? 'center' : 'left',
        vertical: 'middle',
      };
      cell.border = {
        top: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        left: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        bottom: { style: 'thin', color: { argb: 'FFD3D3D3' } },
        right: { style: 'thin', color: { argb: 'FFD3D3D3' } },
      };
      if (idx % 2 === 1) {
        cell.fill = {
          type: 'pattern',
          pattern: 'solid',
          fgColor: { argb: 'FFF9FAFB' },
        };
      }
    });
  });

  // Tự động căn chiều rộng
  columns.forEach((col, idx) => {
    worksheet.getColumn(idx + 1).width = col.width || 20;
  });

  const buffer = await workbook.xlsx.writeBuffer();
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
  saveAs(blob, templateFileName.endsWith('.xlsx') ? templateFileName : `${templateFileName}.xlsx`);
};

/**
 * Đọc file Excel (.xlsx / .xls) và chuyển đổi thành đối tượng File CSV UTF-8 an toàn để gửi lên API Backend
 */
export const convertExcelFileToCsvFile = async (file: File): Promise<File> => {
  const fileName = file.name.toLowerCase();
  if (fileName.endsWith('.csv')) {
    return file;
  }

  const workbook = new ExcelJS.Workbook();
  const buffer = await file.arrayBuffer();
  await workbook.xlsx.load(buffer);

  const worksheet = workbook.worksheets[0];
  if (!worksheet) {
    throw new Error('Tệp Excel không có trang tính nào (Sheet rỗng)!');
  }

  const csvRows: string[] = [];

  worksheet.eachRow((row) => {
    const values = (row.values as any[]).slice(1); // ExcelJS values 1-indexed
    const line = values
      .map((val) => {
        if (val === null || val === undefined) return '""';
        if (typeof val === 'object') {
          if (val.text !== undefined) return `"${String(val.text).replace(/"/g, '""')}"`;
          if (val.result !== undefined) return `"${String(val.result).replace(/"/g, '""')}"`;
        }
        return `"${String(val).replace(/"/g, '""')}"`;
      })
      .join(',');
    csvRows.push(line);
  });

  const csvContent = '\uFEFF' + csvRows.join('\r\n');
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  const csvName = file.name.replace(/\.[^/.]+$/, '') + '.csv';

  return new File([blob], csvName, { type: 'text/csv' });
};
