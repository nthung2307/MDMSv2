import { Col, Row } from 'antd';

import exportIc from '@/assets/export-pdf-ic.svg';
import exportItem from '@/assets/export-pdf-item.svg';
import TablePDFCommon from '@/infrastructure/common/components/table/table-pdf-common';
import { formatNumberWithCommas, renderTextNumber } from '@/infrastructure/utils/helpers';

type Props = {
  record: any;
};

const TemplateNotice = (props: Props) => {
  const { record } = props;
  const displayNumber = (value: any) => {
    return value === null || value === undefined ? '0' : formatNumberWithCommas(value);
  };

  const readingTime =
    !record.previousDay || !record.previousMonth || !record.previousYear
      ? '.........'
      : `${renderTextNumber(record.previousDay)}/${renderTextNumber(record.previousMonth)}/${record.previousYear}`;
  const readingDate = `${renderTextNumber(record.day)}/${renderTextNumber(record.month)}/${record.year}`;
  const resultCosTB = (AP: any, AQ: any) => {
    return AP == 0 ? 0 : (AP / Math.sqrt(AP * AP + AQ * AQ)).toFixed(2);
  };
  const data = [
    {
      key: '1',
      description: (
        <div>
          <p>Previous meter reading</p>
          <p>(Chỉ số cũ)</p>
          <p className='text-nowrap tracking-[-0.03em]'>{`Reading date: ${readingTime}`}</p>
          <p>(Ngày ghi)</p>
        </div>
      ),
      normalHours: displayNumber(record.previousBt),
      peakHours: displayNumber(record.previousCd),
      offPeakHours: displayNumber(record.previousTd),
      totalActivePower: displayNumber(record.previousSg),
      totalReactivePower: displayNumber(record.previousVc),
      pMax: displayNumber(record.previousPmax),
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
      normalHours: displayNumber(record.bt),
      peakHours: displayNumber(record.cd),
      offPeakHours: displayNumber(record.td),
      totalActivePower: displayNumber(record.sg),
      totalReactivePower: displayNumber(record.vc),
      pMax: displayNumber(record.pmax),
      cosPhi: '',
      remarks: '',
    },
    {
      key: '3',
      description: (
        <div>
          <p>Multiplier</p>
          <p>(hệ số)</p>
        </div>
      ),
      normalHours: displayNumber(record.measurementPoint.multiplierFactor),
      peakHours: displayNumber(record.measurementPoint.multiplierFactor),
      offPeakHours: displayNumber(record.measurementPoint.multiplierFactor),
      totalActivePower: displayNumber(record.measurementPoint.multiplierFactor),
      totalReactivePower: displayNumber(record.measurementPoint.multiplierFactor),
      pMax: '0',
      cosPhi: '0',
      remarks: '0',
    },
    {
      key: '4',
      description: (
        <div>
          <p>Actual consumption</p>
          <p>(Số tiêu thụ thực tế)</p>
        </div>
      ),
      normalHours: displayNumber(record.consumtionBt),
      peakHours: displayNumber(record.consumtionCd),
      offPeakHours: displayNumber(record.consumtionTd),
      totalActivePower: displayNumber(record.consumtionSg),
      totalReactivePower: displayNumber(record.consumtionVc),
      pMax: '0',
      cosPhi: displayNumber(resultCosTB(record.consumtionSg, record.consumtionVc)),
      remarks: '0',
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

  // Định nghĩa cột của bảng
  const columns = [
    {
      title: titleCustome('Description', '(Mô tả)'),
      dataIndex: 'description',
      key: 'description',
      width: 109,
    },
    {
      title: titleCustome('Normal hours kWh meter 1', '(Giờ bình thường)'),
      dataIndex: 'normalHours',
      key: 'normalHours',
      width: 91,
    },
    {
      title: titleCustome('Peak hours kWh meter 2', '(Giờ cao điểm)'),
      dataIndex: 'peakHours',
      key: 'peakHours',
      width: 91,
    },
    {
      title: titleCustome('Off-peak hours kWh meter 3', '(Giờ thấp điểm)'),
      dataIndex: 'offPeakHours',
      key: 'offPeakHours',
      width: 91,
    },
    {
      title: titleCustome('Total active power', '(Tổng công suất tác dụng)', 'AP'),
      dataIndex: 'totalActivePower',
      key: 'totalActivePower',
      width: 109,
    },
    {
      title: titleCustome('Total reactive power', '(Tổng công suất phản kháng)', 'AQ'),
      dataIndex: 'totalReactivePower',
      key: 'totalReactivePower',
      width: 109,
    },
    {
      title: titleCustome('Pmax', '(Công suất cực đại)'),
      dataIndex: 'pMax',
      key: 'pMax',
      width: 91,
    },
    {
      title: titleCustome('Cos φ TB', '(≥0.9)'),
      dataIndex: 'cosPhi',
      key: 'cosPhi',
      width: 42,
    },
    {
      title: titleCustome('Remarks', '(Ghi chú)'),
      dataIndex: 'remarks',
      key: 'remarks',
      width: 43,
    },
  ];

  return (
    <div className='relative form-pdf'>
      <div className='w-[500px] mx-auto pl-10 pt-4 mb-2 text-center '>
        <p className='text-[14px] leading-[15px] font-bold tracking-[-0.08em]'>
          VSIP HAI PHONG CO.,LTD
        </p>
        <div className='text-[13px] leading-[14px] font-normal tracking-[-0.08em]'>
          <p>
            VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service
            Park,
          </p>
          <p>Hoa Bình Ward, Hai Phong City, Vietnam</p>
          <p>Tel: 84-225-3959868 Fax: 84-225-3959886</p>
        </div>
      </div>
      <div className='w-[425px] pl-10 mx-auto mb-[5px] text-center'>
        <p className='text-[14px] leading-[17px] font-bold tracking-[-0.08em]'>
          MONTHLY POWER METER READING
        </p>
        <div className='tracking-[-0.08em]'>
          <div>
            <p className='text-[14px] leading-[15px] font-bold'>(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)</p>
            <p className='text-[14px] leading-[15px] font-medium'>
              (M&E Copy – Liên cho Phòng Cơ Điện)
            </p>
          </div>
          <div className='text-12-18 text-code text-start font-medium absolute top-[95px] right-[80px] tracking-[0.0025em]'>
            <p>M&E-1/01 PR04</p>
            <p>
              Serial No (số):{' '}
              <span className='text-red-500 text-16-16 tracking-[2pxp]'>{record.serialNumber}</span>
            </p>
          </div>
        </div>
      </div>
      <img src={exportItem} alt='exportItem' className='absolute top-0 right-0 w-[190px]' />
      <img src={exportIc} alt='exportIc' className='absolute top-5 left-[54px] w-[113px]' />
      <div className='mx-4'>
        <div className='ml-4 mb-1'>
          <div className='flex text-[11px] leading-[20px] pr-1'>
            <p className='w-[150px]'>Tenant Name (Tên khách hàng):</p>
            <span className='flex-1 result-pdf relative pl-2'>
              {record.measurementPoint.customer.fullName}
            </span>
          </div>
          <div className='flex text-[11px] leading-[20px] pr-1'>
            <p className='w-[150px]'>Meter ID (Mã điểm đo):</p>
            <span className='flex-1 result-pdf relative pl-2'>{record.measurementPoint.code}</span>
          </div>
          <Row>
            <Col span={8} className='flex text-[11px] leading-[20px] pr-4'>
              <p>Reading Time (Giờ ghi):</p>
              <span className='result-pdf relative flex-1 text-left pl-2'>00:00</span>
            </Col>
            <Col span={8} className='flex text-[11px] leading-[20px] pr-4'>
              <p>Reading Date (Ngày ghi):</p>
              <span className='result-pdf relative flex-1 text-left pl-2'>{readingDate}</span>
            </Col>
            <Col span={8} className='flex text-[11px] leading-[20px] pr-1'>
              <p>For month (Tháng):</p>
              <span className='result-pdf relative flex-1 text-left pl-2'>
                {`Chỉ số T${renderTextNumber(record.previousMonth)}/${record.previousYear}`}
              </span>
            </Col>
          </Row>
        </div>

        <div>
          <TablePDFCommon
            className='table-pdf'
            columns={columns}
            dataSource={data}
            bordered
            pagination={false}
            scroll={{ x: '100%' }}
          />
        </div>
        <div>
          <div className='my-1 ml-[10px] text-[11px] leading-[11px] tracking-[0.0025em]'>
            <p>The above meter reading has been jointly read by the following Parties:</p>
            <p> (Các chỉ số trên đã được các Bên dưới đây xác nhận)</p>
          </div>
          <div className='flex gap-14 w-full mx-auto'>
            <div
              className='py-[8px] px-[6px] flex-1 text-[11px] leading-[11px]'
              style={{ border: '1px solid #000000' }}
            >
              <div className='mb-3'>
                <p>Party A (VSIP Hai Phong Co., Ltd.)</p>
                <p>Bên A (Công ty TNHH VSIP Hải Phòng)</p>
              </div>
              <div>
                <p>Name and Sigature:</p>
                <p>(Ký và ghi rõ họ tên)</p>
              </div>
            </div>
            <div
              className='py-[8px] px-[6px] flex-1 text-[11px] leading-[11px]'
              style={{ border: '1px solid #000000' }}
            >
              <div className='mb-4'>
                <p>Party B (Tenant)</p>
                <p>Bên B (Khách hàng)</p>
              </div>
              <div>
                <p>Name and Sigature:</p>
                <p>(Ký và ghi rõ họ tên)</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TemplateNotice;
