import { Button, Card, DatePicker, Radio } from 'antd';
import dayjs from 'dayjs';
import React, { useEffect, useMemo, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';
import { useSearchParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetListDailyDailyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetDailyFinalizedEnergyReportRequest.ts';
import { GetListHalfHourFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetListHalfHourFinalizedEnergyReportRequest';
import { GetYearlyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetYearlyFinalizedEnergyReportRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetDailyFinalizedEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetDailyFinalizedEnergyReport.hook';
import useGetHalfHourFinalizedEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetHalfHourFinalizedEnergyReport.hook';
import useGetYearlyFinalizedEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetYearlyFinalizedEnergyReport.hook';
import { _t } from '@/infrastructure/utils/translation';
import TableHalfHourReport from '@/pages/reports/half-hour-finalized-energy-report/components/table-half-hour-report';

const { RangePicker } = DatePicker;

export type ReportType = 'daily' | 'monthly' | 'yearly' | 'range';

interface HalfHourFinalizedEnergyReportProps {
  defaultReportType?: ReportType;
}

/**
 * Chuẩn hóa và làm sạch dữ liệu chu kỳ 30 phút:
 * 1. Tinh chỉnh mốc thời gian lẻ như "00:01", "09:31" về mốc chuẩn ":00" hoặc ":30".
 * 2. Loại bỏ các dòng không đúng mốc chu kỳ 30 phút (lệch quá 5 phút).
 * 3. Khử trùng lặp cùng mốc (deduplicate).
 * 4. Sắp xếp thứ tự thời gian tăng dần.
 */
const normalizeHalfHourData = (records: any[]): any[] => {
  if (!records || !records.length) return [];

  const slotMap = new Map<string, { record: any; diffMs: number }>();

  for (const item of records) {
    if (!item.createdTime) continue;
    const d = dayjs(item.createdTime);
    const minute = d.minute();

    let rounded: dayjs.Dayjs | null = null;

    if (minute >= 55) {
      // 09:59 -> 10:00
      rounded = d.add(1, 'hour').minute(0).second(0).millisecond(0);
    } else if (minute <= 5) {
      // 00:01, 00:02 -> 00:00
      rounded = d.minute(0).second(0).millisecond(0);
    } else if (minute >= 25 && minute <= 35) {
      // 09:31, 09:29 -> 09:30
      rounded = d.minute(30).second(0).millisecond(0);
    }

    // Nếu không nằm trong khoảng hợp lệ ±5 phút quanh mốc 30 phút -> loại bỏ
    if (!rounded) continue;

    const slotKey = rounded.toISOString();
    const diffMs = Math.abs(d.diff(rounded));

    if (!slotMap.has(slotKey) || diffMs < slotMap.get(slotKey)!.diffMs) {
      slotMap.set(slotKey, {
        record: {
          ...item,
          createdTime: rounded.toISOString(),
        },
        diffMs,
      });
    }
  }

  return Array.from(slotMap.values())
    .map((v) => v.record)
    .sort((a, b) => new Date(a.createdTime).getTime() - new Date(b.createdTime).getTime());
};

/**
 * Chuẩn hóa dữ liệu ngày/tháng (chốt lúc 0:00):
 * Tinh chỉnh mốc 00:01, 00:02 về đúng 00:00
 */
const normalizeDailyData = (records: any[]): any[] => {
  if (!records || !records.length) return [];

  const slotMap = new Map<string, { record: any; diffMs: number }>();

  for (const item of records) {
    if (!item.createdTime) continue;
    const d = dayjs(item.createdTime);
    const rounded = d.minute(0).second(0).millisecond(0);
    const slotKey = rounded.format('YYYY-MM-DD');
    const diffMs = Math.abs(d.diff(rounded));

    if (!slotMap.has(slotKey) || diffMs < slotMap.get(slotKey)!.diffMs) {
      slotMap.set(slotKey, {
        record: {
          ...item,
          createdTime: rounded.toISOString(),
        },
        diffMs,
      });
    }
  }

  return Array.from(slotMap.values())
    .map((v) => v.record)
    .sort((a, b) => new Date(a.createdTime).getTime() - new Date(b.createdTime).getTime());
};

const HalfHourFinalizedEnergyReport: React.FC<HalfHourFinalizedEnergyReportProps> = ({
  defaultReportType,
}) => {
  const [searchParams] = useSearchParams();
  const initialTypeFromUrl = (searchParams.get('type') as ReportType) || defaultReportType || 'daily';

  const [reportType, setReportType] = useState<ReportType>(initialTypeFromUrl);
  const [singleDate, setSingleDate] = useState<dayjs.Dayjs>(dayjs());
  const [rangeDate, setRangeDate] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().startOf('day'),
    dayjs().endOf('day'),
  ]);
  const [selectedMonth, setSelectedMonth] = useState<dayjs.Dayjs>(dayjs());
  const [selectedYear, setSelectedYear] = useState<dayjs.Dayjs>(dayjs());

  const [dataForm, setDataForm] = useStateDataForm({
    customerId: null,
    measurementPointId: null,
  });

  const [getHalfHourReport] = useGetHalfHourFinalizedEnergyReportHook();
  const [getDailyReport] = useGetDailyFinalizedEnergyReportHook();
  const [getYearlyReport] = useGetYearlyFinalizedEnergyReportHook();
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();

  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [tableData, setTableData] = useState<any[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [measurementPoints, setMeasurementPoints] = useState<any[]>([]);
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });

  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);

  const handleClearSearch = () => {
    setDataForm({
      customerId: null,
      measurementPointId: null,
    });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setMeasurementPoints([]);
    setTableData([]);
  };

  const getListCustomerAsync = async () => {
    const params = new GetListCustomerRequest({
      pageSize: 10,
      searchByFields: [
        {
          searchFieldName: 'customerCode',
          searchValue: debouncedCustomerCodeSearch || '',
        },
        {
          searchFieldName: 'status',
          searchValue: 'ACTIVE',
        },
      ],
    });
    await getListCustomerRequest(
      params as any,
      (res) => {
        const customersList = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.customerCode,
          fullName: it?.fullName,
        }));
        setCustomers(customersList);
      },
      () => {},
      () => {}
    );
  };

  const getListMeasurementPointAsync = async ({
    pageSize,
    searchByFields,
    customerId,
  }: GetListMeasurementPointsRequest) => {
    const params = { pageSize, searchByFields, customerId };
    await getListMeasurementPointRequest(
      params,
      (res) => {
        const measurementPointsList = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.code,
        }));
        setMeasurementPoints(measurementPointsList);
      },
      () => {},
      () => {}
    );
  };

  const onSearchCustomer = (data: string) => {
    setSearchData({ ...searchData, customerCode: data });
  };

  const onSearchMeasurementPoint = (data: string) => {
    setSearchData({ ...searchData, monitorPoint: data });
  };

  const fetchReportData = async () => {
    if (!dataForm.customerId || !dataForm.measurementPointId) {
      setTableData([]);
      return;
    }

    setLoading(true);

    if (reportType === 'daily') {
      const params: GetListHalfHourFinalizedEnergyReportRequest = {
        customerId: Number(dataForm.customerId),
        measurementPointId: Number(dataForm.measurementPointId),
        startTime: singleDate.startOf('day').toDate(),
        endTime: singleDate.endOf('day').toDate(),
        pageSize: 1000,
      };
      await getHalfHourReport(
        params,
        (res: DataResponse) => {
          const raw = res?.data?.data || [];
          setTableData(normalizeHalfHourData(raw));
          setLoading(false);
        },
        () => {
          setTableData([]);
          setLoading(false);
        },
        setLoading
      );
    } else if (reportType === 'range') {
      const params: GetListHalfHourFinalizedEnergyReportRequest = {
        customerId: Number(dataForm.customerId),
        measurementPointId: Number(dataForm.measurementPointId),
        startTime: rangeDate[0].startOf('day').toDate(),
        endTime: rangeDate[1].endOf('day').toDate(),
        pageSize: 1000,
      };
      await getHalfHourReport(
        params,
        (res: DataResponse) => {
          const raw = res?.data?.data || [];
          setTableData(normalizeHalfHourData(raw));
          setLoading(false);
        },
        () => {
          setTableData([]);
          setLoading(false);
        },
        setLoading
      );
    } else if (reportType === 'monthly') {
      const params: GetListDailyDailyFinalizedEnergyReportRequest = {
        customerId: Number(dataForm.customerId),
        measurementPointId: Number(dataForm.measurementPointId),
        month: selectedMonth.month() + 1,
        year: selectedMonth.year(),
        pageSize: 100,
      };
      await getDailyReport(
        params,
        (res: DataResponse) => {
          const raw = res?.data?.data || [];
          setTableData(normalizeDailyData(raw));
          setLoading(false);
        },
        () => {
          setTableData([]);
          setLoading(false);
        },
        setLoading
      );
    } else if (reportType === 'yearly') {
      const params: GetYearlyFinalizedEnergyReportRequest = {
        customerId: Number(dataForm.customerId),
        measurementPointId: Number(dataForm.measurementPointId),
        year: selectedYear.year(),
        pageSize: 50,
      };
      await getYearlyReport(
        params,
        (res: DataResponse) => {
          const raw = res?.data?.data || [];
          setTableData(normalizeDailyData(raw));
          setLoading(false);
        },
        () => {
          setTableData([]);
          setLoading(false);
        },
        setLoading
      );
    }
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerId) {
      setDataForm({ ...dataForm, measurementPointId: null });
      setTableData([]);
      getListMeasurementPointAsync({
        pageSize: 10,
        searchByFields: [
          {
            searchFieldName: 'code',
            searchValue: debouncedMeasurementPointSearch || '',
          },
        ],
        customerId: dataForm?.customerId,
      });
    } else {
      setMeasurementPoints([]);
      setDataForm({ ...dataForm, measurementPointId: null });
      setTableData([]);
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerId]);

  useEffect(() => {
    if (dataForm.customerId && dataForm.measurementPointId) {
      fetchReportData();
    }
  }, [
    dataForm.customerId,
    dataForm.measurementPointId,
    reportType,
    singleDate,
    rangeDate,
    selectedMonth,
    selectedYear,
  ]);

  const cleanValForExport = (val: any) => {
    if (val === null || val === undefined || val === '') {
      return '-';
    }
    const str = String(val).trim();
    if (str === '' || str === '-') return '-';
    return str;
  };

  const dataExport = useMemo(() => {
    return tableData.map((item, index) => ({
      index: index + 1,
      createdTime: item.createdTime ? dayjs(item.createdTime).format('DD/MM/YYYY HH:mm') : '-',
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
  }, [tableData]);

  const exportConfig = useMemo(() => {
    const point =
      measurementPoints?.find((p: any) => p.value === dataForm?.measurementPointId)?.label ||
      dataForm?.measurementPointId ||
      '';
    const pointPart = point ? `_${point}` : '';

    switch (reportType) {
      case 'daily':
        return {
          title: 'Báo cáo chỉ số điện hàng ngày',
          fileNameCsv: `BaoCao_ChiSoDien${pointPart}_${singleDate.format('YYYYMMDD')}`,
          typeExportAll: 'half-hour-finalized-energy',
          listHeader: Constants.listHeaderExport.HalfHourFinalizedEnergyReport,
          requestBody: {
            customerId: Number(dataForm.customerId),
            measurementPointId: Number(dataForm.measurementPointId),
            startTime: singleDate.startOf('day').toDate(),
            endTime: singleDate.endOf('day').toDate(),
          },
        };
      case 'range':
        return {
          title: 'Báo cáo chỉ số điện theo khoảng thời gian',
          fileNameCsv: `BaoCao_ChiSoDien${pointPart}_${rangeDate[0].format('YYYYMMDD')}_${rangeDate[1].format('YYYYMMDD')}`,
          typeExportAll: 'half-hour-finalized-energy',
          listHeader: Constants.listHeaderExport.HalfHourFinalizedEnergyReport,
          requestBody: {
            customerId: Number(dataForm.customerId),
            measurementPointId: Number(dataForm.measurementPointId),
            startTime: rangeDate[0].startOf('day').toDate(),
            endTime: rangeDate[1].endOf('day').toDate(),
          },
        };
      case 'monthly':
        return {
          title: 'Báo cáo chỉ số điện hàng tháng',
          fileNameCsv: `BaoCao_ChiSoDien${pointPart}_${selectedMonth.format('YYYYMM')}`,
          typeExportAll: 'daily-finalized-energy',
          listHeader: Constants.listHeaderExport.DailyFinalizedEnergyReport,
          requestBody: {
            customerId: Number(dataForm.customerId),
            measurementPointId: Number(dataForm.measurementPointId),
            month: selectedMonth.month() + 1,
            year: selectedMonth.year(),
          },
        };
      case 'yearly':
        return {
          title: 'Báo cáo chỉ số điện hàng năm',
          fileNameCsv: `BaoCao_ChiSoDien${pointPart}_${selectedYear.format('YYYY')}`,
          typeExportAll: 'yearly-finalized-energy',
          listHeader: Constants.listHeaderExport.DailyFinalizedEnergyReport,
          requestBody: {
            customerId: Number(dataForm.customerId),
            measurementPointId: Number(dataForm.measurementPointId),
            year: selectedYear.year(),
          },
        };
    }
  }, [
    reportType,
    measurementPoints,
    dataForm.customerId,
    dataForm.measurementPointId,
    singleDate,
    rangeDate,
    selectedMonth,
    selectedYear,
  ]);

  const exportExtraInfo = useMemo(() => {
    const customer =
      customers?.find((c: any) => c.value === dataForm.customerId)?.label ||
      dataForm.customerId ||
      'Tất cả';
    const point =
      measurementPoints?.find((p: any) => p.value === dataForm.measurementPointId)?.label ||
      dataForm.measurementPointId ||
      'Tất cả';
    let timeLabel = '';
    if (reportType === 'daily') {
      timeLabel = singleDate.format('DD/MM/YYYY');
    } else if (reportType === 'range') {
      timeLabel = `${rangeDate[0].format('DD/MM/YYYY')} - ${rangeDate[1].format('DD/MM/YYYY')}`;
    } else if (reportType === 'monthly') {
      timeLabel = selectedMonth.format('MM/YYYY');
    } else {
      timeLabel = selectedYear.format('YYYY');
    }
    return [
      { label: 'Khách hàng', value: String(customer) },
      { label: 'Điểm đo', value: String(point) },
      { label: 'Thời gian', value: timeLabel },
    ];
  }, [
    customers,
    measurementPoints,
    dataForm.customerId,
    dataForm.measurementPointId,
    reportType,
    singleDate,
    rangeDate,
    selectedMonth,
    selectedYear,
  ]);

  const showClearSearch = !!dataForm?.customerId || !!dataForm?.measurementPointId;

  return (
    <div>
      <PageTitle title={'Chỉ số điện'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[{ name: 'Báo cáo', link: ROUTE_PATH.REPORTS }, { name: 'Chỉ số điện' }]}
        />
        <div className='flex gap-1'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            icon={<FiShare size={20} />}
            disabled={!tableData || tableData.length === 0}
          />
        </div>
      </div>

      <Card className='mt-5 shadow-custom'>
        {/* Hàng 1: Khách hàng và Điểm đo */}
        <div className='mb-5 flex items-center'>
          <div className='flex items-center w-full max-w-2xl gap-3'>
            <div className='basis-1/2'>
              <InputSelectCommon
                label={null}
                attribute={'customerId'}
                dataAttribute={dataForm?.customerId}
                placeholder={'Nhập mã khách hàng'}
                setData={setDataForm}
                isRequired={false}
                allowClear={true}
                listDataItem={customers}
                showSearch={true}
                onSearch={onSearchCustomer}
              />
            </div>
            <div className='basis-1/2'>
              <InputSelectCommon
                label={null}
                attribute={'measurementPointId'}
                dataAttribute={dataForm?.measurementPointId}
                placeholder={'Nhập mã điểm đo'}
                setData={setDataForm}
                isRequired={false}
                allowClear={true}
                listDataItem={measurementPoints}
                disabled={!dataForm.customerId}
                showSearch={true}
                onSearch={onSearchMeasurementPoint}
              />
            </div>
            {showClearSearch && (
              <div>
                <ButtonCommonCustomizable
                  title={_t('')}
                  tooltipText='Xoá tất cả'
                  icon={<IoBackspaceOutline size={25} />}
                  classButton='text-button-variant'
                  onClick={handleClearSearch}
                  variant={'text'}
                />
              </div>
            )}
          </div>
        </div>

        {/* Hàng 2: Bộ chọn chế độ báo cáo, thời gian và nút Truy vấn - Canh lề phải */}
        <div className='mb-5 pb-3 border-b flex flex-wrap justify-end items-center gap-3'>
          <Radio.Group
            value={reportType}
            onChange={(e) => setReportType(e.target.value)}
            buttonStyle='solid'
          >
            <Radio.Button value='daily'>Hàng ngày</Radio.Button>
            <Radio.Button value='monthly'>Hàng tháng</Radio.Button>
            <Radio.Button value='yearly'>Hàng năm</Radio.Button>
            <Radio.Button value='range'>Khoảng thời gian</Radio.Button>
          </Radio.Group>

          {reportType === 'daily' && (
            <DatePicker
              format='DD/MM/YYYY'
              value={singleDate}
              onChange={(d) => d && setSingleDate(d)}
              allowClear={false}
            />
          )}

          {reportType === 'monthly' && (
            <DatePicker
              picker='month'
              format='MM/YYYY'
              value={selectedMonth}
              onChange={(d) => d && setSelectedMonth(d)}
              allowClear={false}
            />
          )}

          {reportType === 'yearly' && (
            <DatePicker
              picker='year'
              format='YYYY'
              value={selectedYear}
              onChange={(d) => d && setSelectedYear(d)}
              allowClear={false}
            />
          )}

          {reportType === 'range' && (
            <RangePicker
              format='DD/MM/YYYY'
              value={rangeDate}
              onChange={(dates) => {
                if (dates && dates[0] && dates[1]) {
                  setRangeDate([dates[0], dates[1]]);
                }
              }}
              allowClear={false}
            />
          )}

          <Button type='primary' onClick={fetchReportData} loading={loading}>
            Truy vấn
          </Button>
        </div>

        {/* Hàng 3: Bảng dữ liệu với phân trang Ant Design chuẩn ở bottomRight */}
        <div>
          <TableHalfHourReport dataSource={tableData} loading={loading} />
        </div>
      </Card>

      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Báo cáo chỉ số điện'
        dataSource={dataExport}
        listHeader={exportConfig.listHeader}
        fileNameCsv={exportConfig.fileNameCsv}
        typeExportAll={''}
        extraInfo={exportExtraInfo}
      />
    </div>
  );
};

export default HalfHourFinalizedEnergyReport;
