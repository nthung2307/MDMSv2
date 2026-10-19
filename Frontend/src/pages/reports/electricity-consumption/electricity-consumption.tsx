import { Button, Card, DatePicker, Radio, Segmented, Switch } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { FiBarChart2, FiShare, FiTable } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useSearchParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import {
  EnergyConsumptionItem,
  EnergyConsumptionReportResponse,
  GetEnergyConsumptionReportRequest,
} from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifyWarning } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetEnergyConsumptionReportHook from '@/infrastructure/repository/reports/hooks/useGetEnergyConsumptionReport.hook.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import EnergyConsumptionChart from '@/pages/reports/electricity-consumption/components/energy-consumption-chart.tsx';
import EnergyConsumptionTable from '@/pages/reports/electricity-consumption/components/energy-consumption-table.tsx';

const ElectricityConsumption = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialCustomer = searchParams.get('customerCode') || searchParams.get('customerId') || '';
  const initialPoint = searchParams.get('monitorPoint') || searchParams.get('measurementPointId') || '';
  const initialView = (searchParams.get('view') as 'table' | 'chart') || 'table';
  const initialCycle = (searchParams.get('cycle') as 'HALF_HOUR' | 'MONTHLY' | 'YEARLY') || 'HALF_HOUR';

  const [cycleType, setCycleType] = useState<'HALF_HOUR' | 'MONTHLY' | 'YEARLY'>(initialCycle);
  const [viewMode, setViewMode] = useState<'table' | 'chart'>(initialView);
  const [compareWithPrevious, setCompareWithPrevious] = useState<boolean>(true);

  const [selectedDate, setSelectedDate] = useState<dayjs.Dayjs>(dayjs());
  const [selectedMonth, setSelectedMonth] = useState<dayjs.Dayjs>(dayjs());
  const [selectedYear, setSelectedYear] = useState<dayjs.Dayjs>(dayjs());

  const [reportData, setReportData] = useState<EnergyConsumptionReportResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);

  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [getEnergyConsumptionReport] = useGetEnergyConsumptionReportHook();

  const [dataForm, setDataForm] = useStateDataForm({
    customerCode: initialCustomer,
    monitorPoint: initialPoint,
  });
  const [showClearSearch, setShowClearSearch] = useState(!!initialCustomer || !!initialPoint);
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
    setReportData(null);
  };

  const fetchReportData = async () => {
    if (!dataForm?.customerCode || !dataForm?.monitorPoint) {
      setReportData(null);
      return;
    }

    const params: GetEnergyConsumptionReportRequest = {
      customerId: Number(dataForm.customerCode),
      measurementPointId: Number(dataForm.monitorPoint),
      cycleType,
      date: cycleType === 'HALF_HOUR' ? selectedDate.format('YYYY-MM-DD') : undefined,
      month: cycleType === 'MONTHLY' ? selectedMonth.month() + 1 : undefined,
      year:
        cycleType === 'YEARLY'
          ? selectedYear.year()
          : cycleType === 'MONTHLY'
            ? selectedMonth.year()
            : selectedDate.year(),
      compareWithPrevious,
    };

    setLoading(true);
    await getEnergyConsumptionReport(
      params,
      (res: any) => {
        setReportData(res?.data || null);
        setLoading(false);
      },
      () => {
        setReportData(null);
        setLoading(false);
      },
      setLoading
    );
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  const prevCustomerCodeRef = useRef<any>(initialCustomer || null);

  useEffect(() => {
    if (dataForm?.customerCode) {
      if (prevCustomerCodeRef.current !== dataForm.customerCode) {
        prevCustomerCodeRef.current = dataForm.customerCode;
        setDataForm((prev: any) => ({ ...prev, monitorPoint: '' }));
      }
      getListMeasurementPointAsync({
        pageSize: 100,
        searchByFields: debouncedMeasurementPointSearch
          ? [
              {
                searchFieldName: 'code',
                searchValue: debouncedMeasurementPointSearch,
              },
            ]
          : [],
        customerId: dataForm?.customerCode,
      });
    } else {
      prevCustomerCodeRef.current = null;
      setListMeasurementPoint([]);
      setDataForm((prev: any) => ({ ...prev, monitorPoint: '' }));
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerCode]);

  useEffect(() => {
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);

  // Đồng bộ params lên URL để khi mở tab mới vẫn giữ nguyên khách hàng, điểm đo và chế độ xem
  useEffect(() => {
    const params: Record<string, string> = {};
    if (dataForm?.customerCode) params.customerCode = String(dataForm.customerCode);
    if (dataForm?.monitorPoint) params.monitorPoint = String(dataForm.monitorPoint);
    if (viewMode && viewMode !== 'table') params.view = viewMode;
    if (cycleType && cycleType !== 'HALF_HOUR') params.cycle = cycleType;
    setSearchParams(params, { replace: true });
  }, [dataForm?.customerCode, dataForm?.monitorPoint, viewMode, cycleType]);

  // Tự động truy vấn khi chọn đầy đủ khách hàng và điểm đo
  useEffect(() => {
    if (dataForm?.customerCode && dataForm?.monitorPoint) {
      fetchReportData();
    }
  }, [
    dataForm?.customerCode,
    dataForm?.monitorPoint,
    cycleType,
    selectedDate,
    selectedMonth,
    selectedYear,
    compareWithPrevious,
  ]);

  const handleQuery = () => {
    if (!dataForm?.customerCode || !dataForm?.monitorPoint) {
      notifyWarning('', 'Vui lòng chọn khách hàng và điểm đo');
      return;
    }
    fetchReportData();
  };

  const fileNameBuilder = () => {
    const customer = listCustomer?.find((c: any) => c.value === dataForm?.customerCode)?.label || '';
    const point = listMeasurementPoint?.find((p: any) => p.value === dataForm?.monitorPoint)?.label || '';
    return `Bao_cao_san_luong_${cycleType}_${customer}_${point}`;
  };

  const chartTitle = useMemo(() => {
    switch (cycleType) {
      case 'HALF_HOUR':
        return `Đồ thị sản lượng chu kỳ 30 phút ngày ${selectedDate.format('DD/MM/YYYY')}`;
      case 'MONTHLY':
        return `Đồ thị sản lượng hàng ngày tháng ${selectedMonth.format('MM/YYYY')}`;
      case 'YEARLY':
        return `Đồ thị sản lượng hàng tháng năm ${selectedYear.format('YYYY')}`;
      default:
        return 'Đồ thị sản lượng điện';
    }
  }, [cycleType, selectedDate, selectedMonth, selectedYear]);

  const exportRequestBody = useMemo(() => {
    return {
      customerId: Number(dataForm?.customerCode),
      measurementPointId: Number(dataForm?.monitorPoint),
      cycleType,
      date: cycleType === 'HALF_HOUR' ? selectedDate.format('YYYY-MM-DD') : undefined,
      month: cycleType === 'MONTHLY' ? selectedMonth.month() + 1 : undefined,
      year:
        cycleType === 'YEARLY'
          ? selectedYear.year()
          : cycleType === 'MONTHLY'
            ? selectedMonth.year()
            : selectedDate.year(),
      compareWithPrevious,
    };
  }, [
    dataForm?.customerCode,
    dataForm?.monitorPoint,
    cycleType,
    selectedDate,
    selectedMonth,
    selectedYear,
    compareWithPrevious,
  ]);

  return (
    <div>
      <PageTitle title={'Báo cáo sản lượng điện'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Sản lượng điện' },
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
          {/* Customer & Measurement Point selector */}
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

          {/* Report Toolbar: Lề trái (Bảng dữ liệu, Biểu đồ, So sánh) - Lề phải (Hàng ngày, Hàng tháng, Hàng năm, DatePicker, Truy vấn) */}
          <div className='mb-4 flex flex-wrap gap-4 items-center justify-between pb-3 border-b'>
            {/* Lề trái: Bảng dữ liệu / Biểu đồ, So sánh */}
            <div className='flex items-center gap-4'>
              <Segmented
                options={[
                  {
                    label: (
                      <div className='flex items-center gap-2 px-2 py-1 font-medium'>
                        <FiTable />
                        <span>Bảng dữ liệu</span>
                      </div>
                    ),
                    value: 'table',
                  },
                  {
                    label: (
                      <div className='flex items-center gap-2 px-2 py-1 font-medium'>
                        <FiBarChart2 />
                        <span>Biểu đồ</span>
                      </div>
                    ),
                    value: 'chart',
                  },
                ]}
                value={viewMode}
                onChange={(val) => setViewMode(val as 'table' | 'chart')}
              />

              <div className='flex items-center gap-2'>
                <Switch
                  checked={compareWithPrevious}
                  onChange={(checked) => setCompareWithPrevious(checked)}
                  id='compare-switch'
                />
                <label htmlFor='compare-switch' className='text-sm text-gray-700 font-medium cursor-pointer select-none'>
                  So sánh
                </label>
              </div>
            </div>

            {/* Lề phải: Hàng ngày, Hàng tháng, Hàng năm, DatePicker, Truy vấn */}
            <div className='flex flex-wrap items-center gap-3'>
              <Radio.Group
                value={cycleType}
                onChange={(e) => setCycleType(e.target.value)}
                buttonStyle='solid'
              >
                <Radio.Button value='HALF_HOUR'>Hàng ngày</Radio.Button>
                <Radio.Button value='MONTHLY'>Hàng tháng</Radio.Button>
                <Radio.Button value='YEARLY'>Hàng năm</Radio.Button>
              </Radio.Group>

              {/* Date Filter based on cycle */}
              {cycleType === 'HALF_HOUR' && (
                <DatePicker
                  value={selectedDate}
                  onChange={(d) => d && setSelectedDate(d)}
                  format='DD/MM/YYYY'
                  allowClear={false}
                />
              )}
              {cycleType === 'MONTHLY' && (
                <DatePicker
                  picker='month'
                  value={selectedMonth}
                  onChange={(m) => m && setSelectedMonth(m)}
                  format='MM/YYYY'
                  allowClear={false}
                />
              )}
              {cycleType === 'YEARLY' && (
                <DatePicker
                  picker='year'
                  value={selectedYear}
                  onChange={(y) => y && setSelectedYear(y)}
                  format='YYYY'
                  allowClear={false}
                />
              )}

              <Button
                type='primary'
                onClick={handleQuery}
                loading={loading}
                icon={<IoSearch size={16} />}
              >
                Truy vấn
              </Button>
            </div>
          </div>

          {/* View Content */}
          {viewMode === 'table' ? (
            <EnergyConsumptionTable
              data={reportData?.items || []}
              total={reportData?.total}
              loading={loading}
            />
          ) : (
            <EnergyConsumptionChart
              chartData={
                reportData?.chart || {
                  labels: [],
                  currentPeriodGiao: [],
                  previousPeriodGiao: [],
                  currentPeriodNhan: [],
                  previousPeriodNhan: [],
                }
              }
              items={reportData?.items || []}
              title={chartTitle}
              compareWithPrevious={compareWithPrevious}
            />
          )}
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Báo cáo sản lượng điện'
        onlyPDF={false}
        chartRef={null}
        listHeader={[]}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={'report'}
        subTypeExportAll={'energy-consumption'}
        requestBody={exportRequestBody}
        onlyExportAll={true}
      />
    </div>
  );
};

export default ElectricityConsumption;

