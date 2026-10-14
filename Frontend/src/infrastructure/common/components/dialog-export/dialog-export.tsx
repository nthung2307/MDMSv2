import { pdf } from '@react-pdf/renderer';
import { Modal } from 'antd';
import html2canvas from 'html2canvas';

import { CheckboxCommon } from '@/infrastructure/common/components/Checkbox/checkbox-common';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import TemplateExport from '@/infrastructure/common/components/dialog-export/template-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import { notifyError, notifyWarning } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useExportFileCsvHook from '@/infrastructure/repository/export/hooks/useExportFileCsv.hook';
import useExportFilePdfHook from '@/infrastructure/repository/export/hooks/useExportFilePdf.hook';
import { _t } from '@/infrastructure/utils/translation';

interface itemHeader {
  label: string;
  attr: string;
  activeMsg?: string;
  inactiveMsg?: string;
}

type Props = {
  open: any;
  title?: string;
  handleCancel: () => void;
  dataSource?: any;
  listHeader: any;
  fileNameCsv: string;
  chartRef?: any;
  format?: any;
  onlyPDF?: boolean;
  isHtml2Pdf?: boolean;
  typeExportAll: string;
  subTypeExportAll?: string;
  paramsId?: any;
  requestBody?: any;
  onlyExportAll?: boolean;
};

const DialogExportForm = (props: Props) => {
  const {
    title,
    handleCancel,
    open,
    dataSource,
    listHeader,
    fileNameCsv,
    chartRef,
    onlyPDF = false,
    typeExportAll,
    subTypeExportAll,
    paramsId,
    requestBody,
    onlyExportAll = false,
  } = props;
  const [data, setData] = useStateDataForm({ type: 'PDF', quanlity: 0 });
  const [exportAllPdf] = useExportFilePdfHook();
  const [exportAllCsv] = useExportFileCsvHook();

  // Export All
  const exportAllPdfAsync = async () => {
    await exportAllPdf(
      typeExportAll,
      (res: any) => {
        const blob = new Blob([res], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `${fileNameCsv}_All`);
        document.body.appendChild(link);
        link.click();
        window.URL.revokeObjectURL(url);
      },
      () => {},
      () => {},
      subTypeExportAll,
      paramsId,
      requestBody
    );
  };

  const exportAllCsvAsync = async () => {
    await exportAllCsv(
      typeExportAll,
      (res: any) => {
        const blob = new Blob([res], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        const downloadName = fileNameCsv.toLowerCase().endsWith('.csv')
          ? `${fileNameCsv.replace(/\.csv$/i, '')}_All.csv`
          : `${fileNameCsv}_All.csv`;
        link.setAttribute('download', downloadName);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      },
      () => {},
      () => {},
      subTypeExportAll,
      paramsId,
      requestBody
    );
  };

  // Export Only page
  const captureDiv = async (chartRef: HTMLElement, fixedWidth: number) => {
    if (chartRef) {
      try {
        const originalStyle = chartRef.getAttribute('style') || '';

        chartRef.style.width = `${fixedWidth}px`;
        chartRef.style.height = 'auto';
        chartRef.style.overflow = 'visible';

        const canvas = await html2canvas(chartRef, {
          scale: 2,
          useCORS: true,
        });

        const originalWidth = canvas.width;
        const originalHeight = canvas.height;

        const aspectRatio = originalHeight / originalWidth;
        const fixedHeight = fixedWidth * aspectRatio;

        const resizedCanvas = document.createElement('canvas');
        resizedCanvas.width = fixedWidth;
        resizedCanvas.height = fixedHeight;

        const ctx = resizedCanvas.getContext('2d');
        if (ctx) {
          ctx.drawImage(canvas, 0, 0, fixedWidth, fixedHeight);
        }

        chartRef.setAttribute('style', originalStyle);
        const image = resizedCanvas.toDataURL('image/png');
        return { image, width: fixedWidth, height: fixedHeight < 900 ? fixedHeight : 900 };
      } catch (error) {
        notifyError(error, 'Không tải được ảnh');
      }
    }
  };

  const downloadPDF = async (fileNameCsv: string) => {
    const chartUrl = await captureDiv(chartRef, 1600);
    const blob = await pdf(
      chartUrl ? (
        <TemplateExport
          dataSource={dataSource}
          listHeader={listHeader}
          title={title || 'Hệ thống AMR'}
          chartUrl={chartUrl}
        />
      ) : (
        <TemplateExport
          dataSource={dataSource}
          listHeader={listHeader}
          title={title || 'Hệ thống AMR'}
        />
      )
    ).toBlob();

    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = `${fileNameCsv}.pdf`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const exportDataToCSV = (listHeader: Array<itemHeader>, data: any[], filename: string) => {
    const headers = listHeader.map((header) => header.label);
    const csvRows = [headers.join(',')];

    data.forEach((row) => {
      const values = listHeader.map(({ attr, activeMsg, inactiveMsg }) => {
        const value = row[attr];
        if (attr == 'status') {
          return row[attr] == 'ACTIVE' ? activeMsg : inactiveMsg;
        }
        const escaped = value !== null && value !== undefined ? `${value}`.replace(/"/g, '""') : '';

        return `"${escaped}"`;
      });
      csvRows.push(values.join(','));
    });

    const csvContent = '\uFEFF' + csvRows.join('\n');
    const encodedUri = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csvContent);

    const link = document.createElement('a');
    link.href = encodedUri;
    link.setAttribute('download', `${filename}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const onSubmitAction = (file: any) => {
    if (!data) {
      return notifyWarning('', 'Hệ thống lỗi, vui lòng liên hệ quản trị viên');
    }

    if (onlyExportAll) {
      file.quanlity = 1;
    }

    const key = `${file.type}_${file.quanlity}`;
    switch (key) {
      case 'PDF_0':
        downloadPDF(fileNameCsv);
        break;

      case 'CSV_0':
        exportDataToCSV(listHeader, dataSource, fileNameCsv);
        break;

      case 'PDF_1':
        exportAllPdfAsync();
        break;

      case 'CSV_1':
        exportAllCsvAsync();
        break;

      default:
        notifyWarning('', 'Hệ thống lỗi, vui lòng liên hệ quản trị viên');
        break;
    }
  };
  return (
    <Modal
      open={open}
      key={'f-0'}
      centered
      className='min-w-300px]'
      closable={true}
      footer={false}
      onCancel={handleCancel}
    >
      <div className='modal space-y-4'>
        <div className='flex flex-col justify-between gap-4'>
          <p className='title text-center font-extrabold text-xl'>Tải xuống</p>
          <InputSelectCommon
            label={_t('Loại tệp')}
            attribute={'type'}
            dataAttribute={data?.type}
            setData={setData}
            isRequired={false}
            listDataItem={
              onlyPDF
                ? [{ label: 'PDF', value: 'PDF' }]
                : [
                    { label: 'PDF', value: 'PDF' },
                    { label: 'CSV', value: 'CSV' },
                  ]
            }
            placeholder={_t('Loại tệp')}
          />
          {typeExportAll !== '' && !onlyExportAll && (
            <div className='flex gap-4'>
              <CheckboxCommon
                dataAttribute={data?.quanlity === 0}
                setData={() => {
                  setData({ quanlity: 0 });
                }}
                disabled={false}
                label={'Trang hiện tại'}
              />
              <CheckboxCommon
                dataAttribute={data?.quanlity === 1}
                setData={() => {
                  setData({ quanlity: 1 });
                }}
                disabled={false}
                label={'Tất cả'}
              />
            </div>
          )}
          <ButtonCommon
            title={_t('Tải xuống')}
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
