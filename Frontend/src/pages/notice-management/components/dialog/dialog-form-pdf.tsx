import { Modal } from 'antd';
import html2pdf from 'html2pdf.js';
import { createRoot } from 'react-dom/client';
import { BiSolidDownload } from 'react-icons/bi';
import { FaPrint } from 'react-icons/fa6';

import TemplateNotice from '@/pages/notice-management/components/form-pdf';

type Props = {
  open: any;
  handleCancel: () => void;
};

const DialogFormPDF = (props: Props) => {
  const { handleCancel, open } = props;
  const printTemplate = (action: string) => {
    if (!open?.record) return;

    let targetElement = document.getElementById('template-notice-print-area');
    let clone: HTMLElement | null = null;
    let tempRoot: any = null;

    if (targetElement) {
      clone = targetElement.cloneNode(true) as HTMLElement;
    } else {
      const tempDiv = document.createElement('div');
      tempDiv.style.width = '794px';
      tempDiv.style.backgroundColor = '#ffffff';
      tempRoot = createRoot(tempDiv);
      tempRoot.render(<TemplateNotice record={open.record} />);
      clone = tempDiv;
    }

    clone.style.width = '794px';
    clone.style.maxWidth = '794px';
    clone.style.minWidth = '794px';
    clone.style.boxSizing = 'border-box';
    clone.style.backgroundColor = '#ffffff';

    const filename = `${open.record?.measurementPoint?.code || 'PHIEU_CHI_SO'}_${open.record?.measurementPoint?.customer?.fullName || ''}.pdf`;
    const opt = {
      margin: 0,
      filename: filename,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: {
        scale: 2,
        useCORS: true,
        logging: false,
      },
      jsPDF: { unit: 'in', format: 'a5', orientation: 'landscape' },
      pagebreak: { mode: ['avoid-all', 'css', 'legacy'] },
    };

    const runPdf = () => {
      if (action === 'export') {
        html2pdf()
          .set(opt)
          .from(clone)
          .save()
          .finally(() => {
            if (tempRoot) tempRoot.unmount();
          });
      } else {
        html2pdf()
          .set(opt)
          .from(clone)
          .toPdf()
          .get('pdf')
          .then((pdf: any) => {
            const pdfBlobUrl = pdf.output('bloburl');

            const printWindow = window.open(pdfBlobUrl, '_blank');
            if (printWindow) {
              printWindow.onload = () => {
                setTimeout(() => {
                  printWindow.print();
                }, 500);
              };
            }
          })
          .finally(() => {
            if (tempRoot) tempRoot.unmount();
          });
      }
    };

    if (tempRoot) {
      setTimeout(runPdf, 150);
    } else {
      runPdf();
    }
  };

  return (
    <Modal
      open={open.status}
      key={'f-0'}
      centered
      className='w-fit modal-pdf scale-110'
      closable={true}
      footer={false}
      onCancel={handleCancel}
    >
      <div className='modal pb-4'>
        <div className='flex justify-between px-6 py-2'>
          <div className='bg-[#E7EBF9] rounded-md'>
            <p className='text-[#CC0004] p-2 uppercase font-extrabold '>phiếu báo</p>
          </div>
          <div className='flex gap-10 mr-10'>
            <button
              className='text-14-16 text-blue-600 uppercase font-bold flex gap-2 items-center'
              onClick={() => {
                printTemplate('export');
              }}
            >
              <BiSolidDownload size={16} />
              <p>Tải về </p>
            </button>
            <button
              type='button'
              className='text-14-16 text-blue-600 uppercase font-bold flex gap-2 items-center'
              onClick={() => {
                printTemplate('print');
              }}
            >
              <FaPrint size={16} />
              <p>Print </p>
            </button>
          </div>
        </div>
        <div id='template-notice-print-area' className='w-[794px] bg-white'>
          <TemplateNotice record={open.record} />
        </div>
      </div>
    </Modal>
  );
};
export default DialogFormPDF;
