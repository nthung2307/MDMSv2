import { Button, Card, DatePicker, Radio, Segmented } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { FiBarChart2, FiShare, FiTable } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { Endpoint } from '@/core/application/common/endPoint.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import {
  GetOperatingParameterReportRequest,
  OperatingParameterItem,
} from '@/core/application/dto/reports/requests/GetOperatingParameterReportRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetOperatingParameterReportHook from '@/infrastructure/repository/reports/hooks/useGetOperatingParameterReport.hook.ts';
import { ChartSheetItem } from '@/infrastructure/utils/excelHelper';
import { _t } from '@/infrastructure/utils/translation.ts';
import OperatingParameterChart from '@/pages/reports/operating-parameter/components/operating-parameter-chart.tsx';
import OperatingParameterTable from '@/pages/reports/operating-parameter/components/operating-parameter-table.tsx';

const { RangePicker } = DatePicker;

const OperatingParameter = () => {
  const [activeTab, setActiveTab] = useState<'table' | 'chart'>('table');
  const [dateMode, setDateMode] = useState<'single' | 'range'>('single');
  const [singleDate, setSingleDate] = useState<dayjs.Dayjs>(dayjs());
  const [rangeDate, setRangeDate] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().startOf('day'),
    dayjs().endOf('day'),
  ]);

  const [tableData, setTableData] = useState<OperatingParameterItem[]>([]);
  const [tableLoading, setTableLoading] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const chartRef = useRef<HTMLDivElement>(null);

  const currentVoltageUnit = useMemo(() => {
    const itemWithUnit = tableData.find((d) => d.dienApPhaAUnit);
    return itemWithUnit?.dienApPhaAUnit || 'V';
  }, [tableData]);

  const operatingHeaders = useMemo(
    () => [
      { label: 'STT', attr: 'stt', width: 7, align: 'center' as const, numFmt: '0' },
      { label: 'Thời gian', attr: 'timeLabel', width: 18, align: 'center' as const },
      { label: `Ua (${currentVoltageUnit})`, attr: 'dienApPhaA', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: `Ub (${currentVoltageUnit})`, attr: 'dienApPhaB', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: `Uc (${currentVoltageUnit})`, attr: 'dienApPhaC', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Ia (A)', attr: 'dongDienPhaA', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Ib (A)', attr: 'dongDienPhaB', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Ic (A)', attr: 'dongDienPhaC', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Pt (kW)', attr: 'congSuatTacDungTong3Pha', width: 16, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Qt (kVAr)', attr: 'congSuatPhanKhangTong3Pha', width: 16, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'PF', attr: 'heSoCongSuatPF', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
      { label: 'Tần số (Hz)', attr: 'tanSoHeThong', width: 14, align: 'right' as const, numFmt: '#,##0.00' },
    ],
    [currentVoltageUnit]
  );

  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [getOperatingParameterReport] = useGetOperatingParameterReportHook();

  const [dataForm, setDataForm] = useStateDataForm({});
  const [showClearSearch, setShowClearSearch] = useState(false);
  const [listMeasurementPoint, setListMeasurementPoint] = useState<any>([]);
  const [listCustomer, setListCustomer] = useState<any>([]);
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });

  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);

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
        const customers = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.customerCode,
        }));
        setListCustomer(customers);
      },
      () => {},
      () => {}
    );
  };

  const getListMeasurementPointAsync = async ({
    pageSize,
    searchByFields,
    sortByFields,
    before,
    after,
    customerId,
  }: GetListMeasurementPointsRequest) => {
    const params = { pageSize, searchByFields, sortByFields, before, after, customerId };
    await getListMeasurementPointRequest(
      params,
      (res) => {
        const measurementPoints = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.code,
        }));
        setListMeasurementPoint(measurementPoints);
      },
      () => {},
      () => {}
    );
  };

  const onSearch = (key: any, data: string) => {
    setSearchData({ ...searchData, [key]: data });
  };

  const clearSearch = () => {
    setDataForm({ customerCode: '', monitorPoint: '' });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setListMeasurementPoint([]);
    setShowClearSearch(false);
    setTableData([]);
  };

  const fileNameBuilder = () => {
    const point = listMeasurementPoint?.find((p: any) => p.value === dataForm?.monitorPoint)?.label || dataForm?.monitorPoint || '';
    const dateStr = dateMode === 'single' ? singleDate.format('YYYYMMDD') : `${rangeDate[0].format('YYYYMMDD')}_${rangeDate[1].format('YYYYMMDD')}`;
    const pointPart = point ? `_${point}` : '';
    return `BaoCao_ThongSoVanHanh${pointPart}_${dateStr}`;
  };

  const normalizeOperatingHalfHourData = (
    records: OperatingParameterItem[]
  ): OperatingParameterItem[] => {
    if (!records || !records.length) return [];

    const slotMap = new Map<string, { record: OperatingParameterItem; diffMs: number }>();

    for (const item of records) {
      if (!item.createdTime) continue;
      const d = dayjs(item.createdTime);
      const minute = d.minute();

      let rounded: dayjs.Dayjs | null = null;

      if (minute >= 55) {
        rounded = d.add(1, 'hour').minute(0).second(0).millisecond(0);
      } else if (minute <= 5) {
        rounded = d.minute(0).second(0).millisecond(0);
      } else if (minute >= 25 && minute <= 35) {
        rounded = d.minute(30).second(0).millisecond(0);
      }

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
      .sort(
        (a, b) =>
          new Date(a.createdTime || 0).getTime() - new Date(b.createdTime || 0).getTime()
      );
  };

  const fetchTableData = async () => {
    if (!dataForm?.customerCode || !dataForm?.monitorPoint) {
      setTableData([]);
      return;
    }

    const startTime =
      dateMode === 'single'
        ? singleDate.startOf('day').toISOString()
        : rangeDate[0].startOf('day').toISOString();
    const endTime =
      dateMode === 'single'
        ? singleDate.endOf('day').toISOString()
        : rangeDate[1].endOf('day').toISOString();

    const params: GetOperatingParameterReportRequest = {
      customerId: Number(dataForm.customerCode),
      measurementPointId: Number(dataForm.monitorPoint),
      startTime,
      endTime,
      pageSize: 500,
    };

    setTableLoading(true);
    await getOperatingParameterReport(
      params,
      (res: any) => {
        const raw = res?.data?.data || [];
        setTableData(normalizeOperatingHalfHourData(raw));
        setTableLoading(false);
      },
      () => {
        setTableData([]);
        setTableLoading(false);
      },
      setTableLoading
    );
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerCode) {
      getListMeasurementPointAsync({
        pageSize: 10,
        searchByFields: [
          {
            searchFieldName: 'code',
            searchValue: debouncedMeasurementPointSearch || '',
          },
        ],
        customerId: dataForm?.customerCode,
      });
    } else {
      setListMeasurementPoint([]);
      setDataForm((prev: any) => ({ ...prev, monitorPoint: '' }));
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerCode]);

  useEffect(() => {
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);

  useEffect(() => {
    if (dataForm?.customerCode && dataForm?.monitorPoint) {
      fetchTableData();
    }
  }, [dataForm?.customerCode, dataForm?.monitorPoint, dateMode, singleDate, rangeDate]);

  const exportRequestBody = useMemo(() => {
    const startTime =
      dateMode === 'single'
        ? singleDate.startOf('day').toISOString()
        : rangeDate[0].startOf('day').toISOString();
    const endTime =
      dateMode === 'single'
        ? singleDate.endOf('day').toISOString()
        : rangeDate[1].endOf('day').toISOString();

    return {
      customerId: Number(dataForm?.customerCode),
      measurementPointId: Number(dataForm?.monitorPoint),
      startTime,
      endTime,
      pageSize: 1000,
    };
  }, [dataForm?.customerCode, dataForm?.monitorPoint, dateMode, singleDate, rangeDate]);

  const exportDataSource = useMemo(() => {
    return tableData.map((it, idx) => ({
      ...it,
      stt: idx + 1,
      timeLabel: it.createdTime ? dayjs(it.createdTime).format('DD/MM/YYYY HH:mm') : '-',
      congSuatTacDungTong3Pha:
        it.congSuatTacDungTong3Pha !== undefined && it.congSuatTacDungTong3Pha !== null
          ? it.congSuatTacDungTong3Pha
          : '-',
      congSuatPhanKhangTong3Pha:
        it.congSuatPhanKhangTong3Pha !== undefined && it.congSuatPhanKhangTong3Pha !== null
          ? it.congSuatPhanKhangTong3Pha
          : '-',
      heSoCongSuatPF:
        it.heSoCongSuatPF !== undefined && it.heSoCongSuatPF !== null ? it.heSoCongSuatPF : '-',
      tanSoHeThong:
        it.tanSoHeThong !== undefined && it.tanSoHeThong !== null ? it.tanSoHeThong : '-',
    }));
  }, [tableData]);

  const operatingChartSheets = useMemo((): ChartSheetItem[] => {
    if (!tableData || tableData.length === 0) return [];

    // Sắp xếp dữ liệu theo thời gian tăng dần
    const sorted = [...tableData].sort((a, b) => {
      const tA = a.createdTime ? new Date(a.createdTime).getTime() : 0;
      const tB = b.createdTime ? new Date(b.createdTime).getTime() : 0;
      return tA - tB;
    });

    const isMultiDay =
      sorted.length > 1 &&
      new Date(sorted[0].createdTime || '').toDateString() !==
        new Date(sorted[sorted.length - 1].createdTime || '').toDateString();

    const labels = sorted.map((it) => {
      if (!it.createdTime) return '';
      const d = dayjs(it.createdTime);
      return isMultiDay ? d.format('HH:mm DD/MM') : d.format('HH:mm');
    });

    const baseLineOptions = (_titleText?: string, unitText?: string) => ({
      responsive: false,
      maintainAspectRatio: false,
      animation: false,
      plugins: {
        legend: {
          position: 'top' as const,
          labels: { boxWidth: 14, font: { size: 12 } },
        },
        title: {
          display: false,
        },
      },
      scales: {
        x: { grid: { display: false } },
        y: {
          beginAtZero: false,
          title: unitText ? { display: true, text: unitText } : undefined,
        },
      },
    });

    return [
      {
        sheetName: 'BĐ Điện áp',
        title: `ĐIỆN ÁP (${currentVoltageUnit})`,
        chartConfig: {
          type: 'line',
          data: {
            labels,
            datasets: [
              {
                label: `Điện áp pha A (${currentVoltageUnit})`,
                data: sorted.map((d) => d.dienApPhaA ?? null),
                borderColor: '#f5222d',
                backgroundColor: 'rgba(245, 34, 45, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
              {
                label: `Điện áp pha B (${currentVoltageUnit})`,
                data: sorted.map((d) => d.dienApPhaB ?? null),
                borderColor: '#fa8c16',
                backgroundColor: 'rgba(250, 140, 22, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
              {
                label: `Điện áp pha C (${currentVoltageUnit})`,
                data: sorted.map((d) => d.dienApPhaC ?? null),
                borderColor: '#1890ff',
                backgroundColor: 'rgba(24, 144, 255, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
            ],
          },
          options: baseLineOptions(`ĐIỆN ÁP (${currentVoltageUnit})`, currentVoltageUnit),
        },
      },
      {
        sheetName: 'BĐ Dòng điện',
        title: 'DÒNG ĐIỆN (A)',
        chartConfig: {
          type: 'line',
          data: {
            labels,
            datasets: [
              {
                label: 'Dòng điện pha A (A)',
                data: sorted.map((d) => d.dongDienPhaA ?? null),
                borderColor: '#f5222d',
                backgroundColor: 'rgba(245, 34, 45, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
              {
                label: 'Dòng điện pha B (A)',
                data: sorted.map((d) => d.dongDienPhaB ?? null),
                borderColor: '#fa8c16',
                backgroundColor: 'rgba(250, 140, 22, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
              {
                label: 'Dòng điện pha C (A)',
                data: sorted.map((d) => d.dongDienPhaC ?? null),
                borderColor: '#1890ff',
                backgroundColor: 'rgba(24, 144, 255, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
            ],
          },
          options: baseLineOptions('DÒNG ĐIỆN (A)', 'A'),
        },
      },
      {
        sheetName: 'BĐ Công suất',
        title: 'CÔNG SUẤT Pt (kW) & Qt (kVAr)',
        chartConfig: {
          type: 'line',
          data: {
            labels,
            datasets: [
              {
                label: 'Công suất tác dụng Pt (kW)',
                data: sorted.map((d) => d.congSuatTacDungTong3Pha ?? null),
                borderColor: '#52c41a',
                backgroundColor: 'rgba(82, 196, 26, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
              {
                label: 'Công suất phản kháng Qt (kVAr)',
                data: sorted.map((d) => d.congSuatPhanKhangTong3Pha ?? null),
                borderColor: '#722ed1',
                backgroundColor: 'rgba(114, 46, 209, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
            ],
          },
          options: baseLineOptions('CÔNG SUẤT Pt (kW) & Qt (kVAr)', 'kW / kVAr'),
        },
      },
      {
        sheetName: 'BĐ Hệ số PF',
        title: 'HỆ SỐ PF',
        chartConfig: {
          type: 'line',
          data: {
            labels,
            datasets: [
              {
                label: 'Hệ số công suất PF',
                data: sorted.map((d) => d.heSoCongSuatPF ?? null),
                borderColor: '#13c2c2',
                backgroundColor: 'rgba(19, 194, 194, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
            ],
          },
          options: {
            ...baseLineOptions('HỆ SỐ PF', 'PF'),
            scales: {
              x: { grid: { display: false } },
              y: { min: 0, max: 1, title: { display: true, text: 'PF' } },
            },
          },
        },
      },
      {
        sheetName: 'BĐ Tần số',
        title: 'TẦN SỐ (Hz)',
        chartConfig: {
          type: 'line',
          data: {
            labels,
            datasets: [
              {
                label: 'Tần số (Hz)',
                data: sorted.map((d) => d.tanSoHeThong ?? null),
                borderColor: '#eb2f96',
                backgroundColor: 'rgba(235, 47, 150, 0.1)',
                tension: 0.3,
                pointRadius: 2,
              },
            ],
          },
          options: baseLineOptions('TẦN SỐ (Hz)', 'Hz'),
        },
      },
    ];
  }, [tableData, currentVoltageUnit]);

  return (
    <div>
      <PageTitle title={'Thông số vận hành'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Thông số vận hành' },
          ]}
        />
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            disabled={!(dataForm.customerCode && dataForm.monitorPoint)}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>

      <Card className='mt-5 shadow-custom'>
        <div>
          {/* Customer & Measurement Point Selection */}
          <div className='flex gap-3 items-center mb-5'>
            <div className='basis-1/2 flex items-center gap-3'>
              <div className='basis-1/2'>
                <InputSelectCommon
                  isLabel={false}
                  label={''}
                  dataAttribute={dataForm?.customerCode}
                  attribute={'customerCode'}
                  setData={setDataForm}
                  isRequired={false}
                  placeholder={'Nhập mã khách hàng'}
                  listDataItem={listCustomer}
                  onSearch={(value) => {
                    onSearch('customerCode', value);
                  }}
                  showSearch={true}
                  allowClear={true}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  isLabel={false}
                  label={''}
                  attribute={'monitorPoint'}
                  dataAttribute={dataForm?.monitorPoint}
                  setData={setDataForm}
                  isRequired={false}
                  placeholder={'Nhập mã điểm đo'}
                  onSearch={(value) => {
                    onSearch('monitorPoint', value);
                  }}
                  listDataItem={listMeasurementPoint}
                  showSearch={true}
                  allowClear={true}
                  disabled={!dataForm?.customerCode}
                />
              </div>
            </div>
            {showClearSearch && (
              <div>
                <ButtonCommonCustomizable
                  title={_t('')}
                  classButton='text-button-variant'
                  tooltipText='Xoá tất cả'
                  icon={<IoBackspaceOutline size={25} />}
                  onClick={clearSearch}
                  variant={'text'}
                />
              </div>
            )}
          </div>

          {/* Mode Switcher: Table vs Chart & Unified Time Controls */}
          <div className='flex flex-wrap justify-between items-center gap-4 mb-5 pb-3 border-b'>
            <Segmented
              options={[
                {
                  label: (
                    <div className='flex items-center gap-2 px-3 py-1 font-medium'>
                      <FiTable />
                      <span>Bảng dữ liệu</span>
                    </div>
                  ),
                  value: 'table',
                },
                {
                  label: (
                    <div className='flex items-center gap-2 px-3 py-1 font-medium'>
                      <FiBarChart2 />
                      <span>Biểu đồ</span>
                    </div>
                  ),
                  value: 'chart',
                },
              ]}
              value={activeTab}
              onChange={(val) => setActiveTab(val as 'table' | 'chart')}
            />

            <div className='flex items-center gap-3'>
              <Radio.Group
                value={dateMode}
                onChange={(e) => setDateMode(e.target.value)}
                buttonStyle='solid'
              >
                <Radio.Button value='single'>Hàng ngày</Radio.Button>
                <Radio.Button value='range'>Khoảng thời gian</Radio.Button>
              </Radio.Group>

              {dateMode === 'single' ? (
                <DatePicker
                  value={singleDate}
                  onChange={(d) => d && setSingleDate(d)}
                  format='DD/MM/YYYY'
                  allowClear={false}
                />
              ) : (
                <RangePicker
                  value={rangeDate}
                  onChange={(dates) => {
                    if (dates && dates[0] && dates[1]) {
                      setRangeDate([dates[0], dates[1]]);
                    }
                  }}
                  format='DD/MM/YYYY'
                  allowClear={false}
                />
              )}

              <Button type='primary' onClick={fetchTableData} loading={tableLoading}>
                Truy vấn
              </Button>
            </div>
          </div>

          {/* Tab 1: Table Content */}
          <div style={{ display: activeTab === 'table' ? 'block' : 'none' }}>
            <OperatingParameterTable
              data={tableData}
              loading={tableLoading}
              voltageUnit={currentVoltageUnit}
            />
          </div>

          {/* Tab 2: Chart Content bound directly to tableData */}
          <div
            ref={chartRef}
            style={{ display: activeTab === 'chart' ? 'block' : 'none' }}
          >
            <OperatingParameterChart
              data={tableData}
              loading={tableLoading}
              voltageUnit={currentVoltageUnit}
            />
          </div>
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='BÁO CÁO THÔNG SỐ VẬN HÀNH'
        chartRef={chartRef.current}
        chartSheets={operatingChartSheets}
        dataSource={exportDataSource}
        listHeader={operatingHeaders}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={''}
        serverExcelEndpoint={Endpoint.Report.ExportExcelOperatingParameter}
        requestBody={exportRequestBody}
        extraInfo={[
          {
            label: 'Khách hàng',
            value:
              listCustomer?.find((c: any) => c.value === dataForm.customerCode)?.label ||
              dataForm.customerCode ||
              'Tất cả',
          },
          {
            label: 'Điểm đo',
            value:
              listMeasurementPoint?.find((p: any) => p.value === dataForm.monitorPoint)?.label ||
              dataForm.monitorPoint ||
              'Tất cả',
          },
          {
            label: 'Thời gian',
            value:
              dateMode === 'single'
                ? singleDate.format('DD/MM/YYYY')
                : `${rangeDate[0].format('DD/MM/YYYY')} - ${rangeDate[1].format('DD/MM/YYYY')}`,
          },
        ]}
      />
    </div>
  );
};

export default OperatingParameter;
