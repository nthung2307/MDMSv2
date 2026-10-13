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
    const tempDiv = document.createElement('div');
    const root = createRoot(tempDiv);
    root.render(<TemplateNotice record={open.record} />);
    const filename = `${open.record.measurementPoint.code}_${open.record.measurementPoint.customer.fullName}.pdf`;
    let opt = {
      margin: 0,
      filename: filename,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { scale: 2 },
      jsPDF: { unit: 'in', format: 'a5', orientation: 'landscape' },
    };

    if (action === 'export') {
      html2pdf().set(opt).from(tempDiv).save();
    } else {
      html2pdf()
        .set(opt)
        .from(tempDiv)
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
        });
    }

    setTimeout(() => {
      root.unmount();
    }, 0);
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
        <div className='h-[560px] w-[794px]'>
          <TemplateNotice record={open.record} />
        </div>
      </div>
    </Modal>
  );
};
export default DialogFormPDF;
