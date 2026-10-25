import { Button, Card, DatePicker, Select } from 'antd';
import dayjs from 'dayjs';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { FiSearch, FiShare } from 'react-icons/fi';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetActivePointsEnergyReportRequest } from '@/core/application/dto/reports/requests/GetActivePointsEnergyReportRequest.ts';
import { IActivePointsEnergyReportItem } from '@/core/application/interfaces/response/reports/report.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useGetActivePointsEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetActivePointsEnergyReport.hook.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import TableActivePointsEnergyReport from '@/pages/reports/active-points-energy-report/components/table-active-points-energy-report.tsx';

// 48 mốc chu kỳ 30 phút trong ngày
const HALF_HOUR_SLOTS = Array.from({ length: 48 }, (_, i) => {
  const hour = Math.floor(i / 2);
  const minute = i % 2 === 0 ? '00' : '30';
  const val = `${String(hour).padStart(2, '0')}:${minute}`;
  return { label: val, value: val };
});

const getNearestSlotString = () => {
  const now = dayjs();
  const minute = now.minute();
  let roundedMinute = 0;
  let roundedHour = now.hour();
  if (minute < 15) {
    roundedMinute = 0;
  } else if (minute < 45) {
    roundedMinute = 30;
  } else {
    roundedMinute = 0;
    roundedHour = (roundedHour + 1) % 24;
  }
  return `${String(roundedHour).padStart(2, '0')}:${String(roundedMinute).padStart(2, '0')}`;
};

const cleanValForExport = (val: any) => {
  if (val === null || val === undefined || val === '') return '-';
  const str = String(val).trim();
  if (str === '' || str === '-') return '-';
  return str;
};

const ActivePointsEnergyReport: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [tableData, setTableData] = useState<IActivePointsEnergyReportItem[]>([]);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);

  // Filters: Chỉ chọn Ngày (DatePicker) và Giờ (chu kỳ 30 phút)
  const [selectedDate, setSelectedDate] = useState<dayjs.Dayjs>(dayjs());
  const [selectedSlot, setSelectedSlot] = useState<string>(getNearestSlotString());

  const [getActivePointsEnergyReportRequest] = useGetActivePointsEnergyReportHook();

  // Ghép ngày và mốc slot 30 phút thành ISO string
  const getCombinedTimeSlot = () => {
    const [h, m] = selectedSlot.split(':').map(Number);
    return selectedDate.hour(h).minute(m).second(0).millisecond(0).toISOString();
  };

  const fetchReportData = async () => {
    const targetTime = getCombinedTimeSlot();
    const params = new GetActivePointsEnergyReportRequest({
      selectedTime: targetTime,
    });

    await getActivePointsEnergyReportRequest(
      params,
      (res: any) => {
        const raw = res?.data || [];
        setTableData(raw);
      },
      () => {
        setTableData([]);
      },
      setLoading
    );
  };

  // Tự động truy vấn khi người dùng thay đổi Ngày hoặc Giờ lấy dữ liệu (Không tự động khi mới mở trang)
  const isInitialMount = useRef(true);
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }
    fetchReportData();
  }, [selectedDate, selectedSlot]);

  // Chuẩn bị dữ liệu xuất file (gồm STT, Thời gian, Mã điểm đo, các chỉ số giao/nhận)
  const dataExport = useMemo(() => {
    if (!tableData || !tableData.length) return [];
    return tableData.map((item, idx) => ({
      index: idx + 1,
      createdTime: item.createdTime
        ? dayjs(item.createdTime).format('DD/MM/YYYY HH:mm')
        : `${selectedDate.format('DD/MM/YYYY')} ${selectedSlot}`,
      measurementPointCode: item.measurementPointCode || '-',
      chiSoTichLuyDienNangTacDungAPTongKenhGiao: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPTongKenhGiao
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao
      ),
      chiSoTichLuyDienNangPhanKhangAQTongKenhGiao: cleanValForExport(
        item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiao
      ),
      chiSoTichLuyDienNangTacDungAPTongKenhNhan: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPTongKenhNhan
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan
      ),
      chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan: cleanValForExport(
        item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan
      ),
      chiSoTichLuyDienNangPhanKhangAQTongKenhNhan: cleanValForExport(
        item.chiSoTichLuyDienNangPhanKhangAQTongKenhNhan
      ),
    }));
  }, [tableData, selectedDate, selectedSlot]);

  const exportConfig = useMemo(() => {
    return {
      title: 'Báo cáo chỉ số điện các điểm đo',
      fileNameCsv: `BaoCao_ChiSoDien_CacDiemDo_${selectedDate.format('YYYYMMDD')}_${selectedSlot.replace(':', '')}`,
      typeExportAll: '',
      listHeader: Constants.listHeaderExport.ActivePointsEnergyReport,
      requestBody: {
        selectedTime: getCombinedTimeSlot(),
      },
    };
  }, [selectedDate, selectedSlot]);

  return (
    <div>
      <PageTitle title={'Chỉ số điện các điểm đo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Chỉ số điện các điểm đo' },
          ]}
        />
        <div className='flex gap-1'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => setIsShowExport(true)}
            icon={<FiShare size={20} />}
            disabled={!tableData || tableData.length === 0}
          />
        </div>
      </div>

      <Card className='mt-5 shadow-custom'>
        {/* Thanh công cụ lọc dữ liệu: Chọn Ngày (DatePicker) & Giờ (chu kỳ 30 phút) + Nút Truy vấn (Canh lề phải) */}
        <div className='mb-5 pb-3 border-b flex justify-end items-center gap-3 flex-wrap'>
          <div className='flex items-center gap-2'>
            <span className='text-gray-600 font-medium'>Ngày:</span>
            <DatePicker
              format='DD/MM/YYYY'
              value={selectedDate}
              onChange={(d) => d && setSelectedDate(d)}
              allowClear={false}
              style={{ width: 140 }}
            />
          </div>

          <div className='flex items-center gap-2'>
            <span className='text-gray-600 font-medium'>Giờ:</span>
            <Select
              value={selectedSlot}
              onChange={(val) => setSelectedSlot(val)}
              options={HALF_HOUR_SLOTS}
              style={{ width: 110 }}
            />
          </div>

          <Button
            type='primary'
            icon={<FiSearch />}
            onClick={fetchReportData}
            loading={loading}
            className='flex items-center gap-1.5'
          >
            Truy vấn
          </Button>
        </div>

        {/* Bảng dữ liệu: STT, Mã điểm đo, các chỉ số biểu chiều giao và chiều nhận */}
        <div>
          <TableActivePointsEnergyReport dataSource={tableData} loading={loading} />
        </div>
      </Card>

      {/* Dialog xuất file: Xuất client-side trực tiếp từ dataSource đầy đủ, không gọi backend tránh lỗi 404 */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => setIsShowExport(false)}
        title={exportConfig.title}
        dataSource={dataExport}
        listHeader={exportConfig.listHeader}
        fileNameCsv={exportConfig.fileNameCsv}
        typeExportAll={''}
        extraInfo={[]}
      />
    </div>
  );
};

export default ActivePointsEnergyReport;
