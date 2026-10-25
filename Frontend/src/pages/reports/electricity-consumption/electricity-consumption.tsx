import { Button, Card, DatePicker, Radio, Segmented, Switch } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { FiBarChart2, FiShare, FiTable } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useSearchParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { Endpoint } from '@/core/application/common/endPoint.ts';
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
import { ChartSheetItem } from '@/infrastructure/utils/excelHelper';
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
  const compareWithPrevious = cycleType === 'YEARLY';

  const [selectedDate, setSelectedDate] = useState<dayjs.Dayjs>(dayjs());
  const [selectedMonth, setSelectedMonth] = useState<dayjs.Dayjs>(dayjs());
  const [selectedYear, setSelectedYear] = useState<dayjs.Dayjs>(dayjs());

  const [reportData, setReportData] = useState<EnergyConsumptionReportResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const chartContainerRef = useRef<HTMLDivElement>(null);

  const energyConsumptionHeaders = useMemo(
    () => [
      { label: 'STT', attr: 'stt', width: 7, align: 'center' as const, numFmt: '0' },
      { label: 'Thời gian', attr: 'timeLabel', width: 18, align: 'center' as const },
      { label: 'Kênh giao (kWh) - Tổng', attr: 'sanLuongTacDungTongKenhGiao', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh giao (kWh) - Biểu 1 (BT)', attr: 'sanLuongTacDungBTKenhGiao', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh giao (kWh) - Biểu 2 (CD)', attr: 'sanLuongTacDungCDKenhGiao', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh giao (kWh) - Biểu 3 (TD)', attr: 'sanLuongTacDungTDKenhGiao', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh giao (kWh) - VC (kVArh)', attr: 'sanLuongPhanKhangTongKenhGiao', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh nhận (kWh) - Tổng', attr: 'sanLuongTacDungTongKenhNhan', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh nhận (kWh) - Biểu 1 (BT)', attr: 'sanLuongTacDungBTKenhNhan', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh nhận (kWh) - Biểu 2 (CD)', attr: 'sanLuongTacDungCDKenhNhan', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh nhận (kWh) - Biểu 3 (TD)', attr: 'sanLuongTacDungTDKenhNhan', width: 15, align: 'right' as const, numFmt: '#,##0' },
      { label: 'Kênh nhận (kWh) - VC (kVArh)', attr: 'sanLuongPhanKhangTongKenhNhan', width: 15, align: 'right' as const, numFmt: '#,##0' },
    ],
    []
  );

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
    const point =
      listMeasurementPoint?.find((p: any) => p.value === dataForm?.monitorPoint)?.label ||
      dataForm?.monitorPoint ||
      '';
    const pointPart = point ? `_${point}` : '';
    let dateStr = '';
    if (cycleType === 'HALF_HOUR') {
      dateStr = selectedDate.format('YYYYMMDD');
    } else if (cycleType === 'MONTHLY') {
      dateStr = selectedMonth.format('YYYYMM');
    } else {
      dateStr = selectedYear.format('YYYY');
    }
    return `BaoCao_SanLuongDien${pointPart}_${dateStr}`;
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

  const exportDataSource = useMemo(() => {
    if (!reportData?.items) return [];
    return reportData.items.map((it, idx) => ({
      stt: idx + 1,
      timeLabel: it.timeLabel || '',
      sanLuongTacDungTongKenhGiao:
        it.sanLuongTacDungTongKenhGiao !== null && it.sanLuongTacDungTongKenhGiao !== undefined
          ? Math.round(Number(it.sanLuongTacDungTongKenhGiao))
          : '-',
      sanLuongTacDungBTKenhGiao:
        it.sanLuongTacDungBTKenhGiao !== null && it.sanLuongTacDungBTKenhGiao !== undefined
          ? Math.round(Number(it.sanLuongTacDungBTKenhGiao))
          : '-',
      sanLuongTacDungCDKenhGiao:
        it.sanLuongTacDungCDKenhGiao !== null && it.sanLuongTacDungCDKenhGiao !== undefined
          ? Math.round(Number(it.sanLuongTacDungCDKenhGiao))
          : '-',
      sanLuongTacDungTDKenhGiao:
        it.sanLuongTacDungTDKenhGiao !== null && it.sanLuongTacDungTDKenhGiao !== undefined
          ? Math.round(Number(it.sanLuongTacDungTDKenhGiao))
          : '-',
      sanLuongPhanKhangTongKenhGiao:
        it.sanLuongPhanKhangTongKenhGiao !== null && it.sanLuongPhanKhangTongKenhGiao !== undefined
          ? Math.round(Number(it.sanLuongPhanKhangTongKenhGiao))
          : '-',
      sanLuongTacDungTongKenhNhan:
        it.sanLuongTacDungTongKenhNhan !== null && it.sanLuongTacDungTongKenhNhan !== undefined
          ? Math.round(Number(it.sanLuongTacDungTongKenhNhan))
          : '-',
      sanLuongTacDungBTKenhNhan:
        it.sanLuongTacDungBTKenhNhan !== null && it.sanLuongTacDungBTKenhNhan !== undefined
          ? Math.round(Number(it.sanLuongTacDungBTKenhNhan))
          : '-',
      sanLuongTacDungCDKenhNhan:
        it.sanLuongTacDungCDKenhNhan !== null && it.sanLuongTacDungCDKenhNhan !== undefined
          ? Math.round(Number(it.sanLuongTacDungCDKenhNhan))
          : '-',
      sanLuongTacDungTDKenhNhan:
        it.sanLuongTacDungTDKenhNhan !== null && it.sanLuongTacDungTDKenhNhan !== undefined
          ? Math.round(Number(it.sanLuongTacDungTDKenhNhan))
          : '-',
      sanLuongPhanKhangTongKenhNhan:
        it.sanLuongPhanKhangTongKenhNhan !== null && it.sanLuongPhanKhangTongKenhNhan !== undefined
          ? Math.round(Number(it.sanLuongPhanKhangTongKenhNhan))
          : '-',
    }));
  }, [reportData?.items]);

  const exportSummaryRows = useMemo(() => {
    if (!reportData?.items || reportData.items.length === 0) return [];

    const keys: (keyof EnergyConsumptionItem)[] = [
      'sanLuongTacDungTongKenhGiao',
      'sanLuongTacDungBTKenhGiao',
      'sanLuongTacDungCDKenhGiao',
      'sanLuongTacDungTDKenhGiao',
      'sanLuongPhanKhangTongKenhGiao',
      'sanLuongTacDungTongKenhNhan',
      'sanLuongTacDungBTKenhNhan',
      'sanLuongTacDungCDKenhNhan',
      'sanLuongTacDungTDKenhNhan',
      'sanLuongPhanKhangTongKenhNhan',
    ];

    const sumRow: Record<string, any> = { stt: 'Tổng', timeLabel: '' };
    const minRow: Record<string, any> = { stt: 'Min', timeLabel: '' };
    const maxRow: Record<string, any> = { stt: 'Max', timeLabel: '' };

    const total = reportData.total;

    keys.forEach((key) => {
      let sum = 0;
      let count = 0;
      let minVal = Infinity;
      let maxVal = -Infinity;

      reportData.items.forEach((it) => {
        const v = it[key];
        if (v !== undefined && v !== null && v !== '' && !isNaN(Number(v))) {
          const n = Number(v);
          sum += n;
          count++;
          if (n < minVal) minVal = n;
          if (n > maxVal) maxVal = n;
        }
      });

      if (count === 0) {
        sumRow[key] = '-';
        minRow[key] = '-';
        maxRow[key] = '-';
      } else {
        if (
          total &&
          total[key] !== undefined &&
          total[key] !== null &&
          total[key] !== '' &&
          !isNaN(Number(total[key]))
        ) {
          sumRow[key] = Math.round(Number(total[key]));
        } else {
          sumRow[key] = Math.round(sum);
        }
        minRow[key] = Math.round(minVal);
        maxRow[key] = Math.round(maxVal);
      }
    });

    return [sumRow, minRow, maxRow];
  }, [reportData?.items, reportData?.total]);

  const energyChartSheets = useMemo((): ChartSheetItem[] => {
    if (!reportData?.items || reportData.items.length === 0) return [];
    const items = reportData.items;
    const labels = items.map((it) => it.timeLabel || '');

    // Kênh giao
    const b1Giao = items.map((it) => Math.round(Number(it.sanLuongTacDungBTKenhGiao || 0)));
    const b2Giao = items.map((it) => Math.round(Number(it.sanLuongTacDungCDKenhGiao || 0)));
    const b3Giao = items.map((it) => Math.round(Number(it.sanLuongTacDungTDKenhGiao || 0)));

    const datasetsGiao: any[] = [
      {
        type: 'bar',
        label: 'Biểu 1 (BT)',
        data: b1Giao,
        backgroundColor: 'rgba(59, 130, 246, 0.85)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar',
        label: 'Biểu 2 (CD)',
        data: b2Giao,
        backgroundColor: 'rgba(239, 68, 68, 0.85)',
        borderColor: 'rgb(239, 68, 68)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar',
        label: 'Biểu 3 (TD)',
        data: b3Giao,
        backgroundColor: 'rgba(16, 185, 129, 0.85)',
        borderColor: 'rgb(16, 185, 129)',
        borderWidth: 1,
        stack: 'energyStack',
      },
    ];

    if (compareWithPrevious) {
      const prevDataGiao = items.map((it) =>
        Math.round(Number(it.previousSanLuongTacDungTongKenhGiao || 0))
      );
      datasetsGiao.push({
        type: 'line',
        label: 'Cùng kỳ năm trước (Giao)',
        data: prevDataGiao,
        borderColor: 'rgb(245, 158, 11)',
        backgroundColor: 'rgba(245, 158, 11, 0.2)',
        borderWidth: 2,
        borderDash: [5, 5],
        pointRadius: 3,
        fill: false,
      });
    }

    // Kênh nhận
    const b1Nhan = items.map((it) => Math.round(Number(it.sanLuongTacDungBTKenhNhan || 0)));
    const b2Nhan = items.map((it) => Math.round(Number(it.sanLuongTacDungCDKenhNhan || 0)));
    const b3Nhan = items.map((it) => Math.round(Number(it.sanLuongTacDungTDKenhNhan || 0)));

    const datasetsNhan: any[] = [
      {
        type: 'bar',
        label: 'Biểu 1 (BT)',
        data: b1Nhan,
        backgroundColor: 'rgba(59, 130, 246, 0.85)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar',
        label: 'Biểu 2 (CD)',
        data: b2Nhan,
        backgroundColor: 'rgba(239, 68, 68, 0.85)',
        borderColor: 'rgb(239, 68, 68)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar',
        label: 'Biểu 3 (TD)',
        data: b3Nhan,
        backgroundColor: 'rgba(16, 185, 129, 0.85)',
        borderColor: 'rgb(16, 185, 129)',
        borderWidth: 1,
        stack: 'energyStack',
      },
    ];

    if (compareWithPrevious) {
      const prevDataNhan = items.map((it) =>
        Math.round(Number(it.previousSanLuongTacDungTongKenhNhan || 0))
      );
      datasetsNhan.push({
        type: 'line',
        label: 'Cùng kỳ năm trước (Nhận)',
        data: prevDataNhan,
        borderColor: 'rgb(245, 158, 11)',
        backgroundColor: 'rgba(245, 158, 11, 0.2)',
        borderWidth: 2,
        borderDash: [5, 5],
        pointRadius: 3,
        fill: false,
      });
    }

    const baseOptions = (subTitleText: string) => ({
      responsive: false,
      maintainAspectRatio: false,
      animation: false,
      plugins: {
        legend: {
          position: 'top' as const,
          labels: { boxWidth: 14, font: { size: 12 } },
        },
        title: {
          display: true,
          text: subTitleText,
          font: { size: 14, weight: 'bold' as const },
        },
      },
      scales: {
        x: { stacked: true, grid: { display: false } },
        y: { stacked: true, beginAtZero: true },
      },
    });

    return [
      {
        sheetName: 'BĐ Kênh giao',
        title: `${chartTitle} - Kênh giao (kWh)`,
        chartConfig: {
          type: 'bar',
          data: { labels, datasets: datasetsGiao },
          options: baseOptions(`${chartTitle} - Kênh giao (kWh)`),
        },
      },
      {
        sheetName: 'BĐ Kênh nhận',
        title: `${chartTitle} - Kênh nhận (kWh)`,
        chartConfig: {
          type: 'bar',
          data: { labels, datasets: datasetsNhan },
          options: baseOptions(`${chartTitle} - Kênh nhận (kWh)`),
        },
      },
    ];
  }, [reportData?.items, chartTitle, compareWithPrevious]);

  const exportTotalRow = useMemo(() => {
    if (!reportData?.total) return undefined;
    const tot = reportData.total;
    return {
      stt: 'Tổng',
      timeLabel: '',
      sanLuongTacDungTongKenhGiao:
        tot.sanLuongTacDungTongKenhGiao !== null && tot.sanLuongTacDungTongKenhGiao !== undefined
          ? Math.round(Number(tot.sanLuongTacDungTongKenhGiao))
          : '-',
      sanLuongTacDungBTKenhGiao:
        tot.sanLuongTacDungBTKenhGiao !== null && tot.sanLuongTacDungBTKenhGiao !== undefined
          ? Math.round(Number(tot.sanLuongTacDungBTKenhGiao))
          : '-',
      sanLuongTacDungCDKenhGiao:
        tot.sanLuongTacDungCDKenhGiao !== null && tot.sanLuongTacDungCDKenhGiao !== undefined
          ? Math.round(Number(tot.sanLuongTacDungCDKenhGiao))
          : '-',
      sanLuongTacDungTDKenhGiao:
        tot.sanLuongTacDungTDKenhGiao !== null && tot.sanLuongTacDungTDKenhGiao !== undefined
          ? Math.round(Number(tot.sanLuongTacDungTDKenhGiao))
          : '-',
      sanLuongPhanKhangTongKenhGiao:
        tot.sanLuongPhanKhangTongKenhGiao !== null && tot.sanLuongPhanKhangTongKenhGiao !== undefined
          ? Math.round(Number(tot.sanLuongPhanKhangTongKenhGiao))
          : '-',
      sanLuongTacDungTongKenhNhan:
        tot.sanLuongTacDungTongKenhNhan !== null && tot.sanLuongTacDungTongKenhNhan !== undefined
          ? Math.round(Number(tot.sanLuongTacDungTongKenhNhan))
          : '-',
      sanLuongTacDungBTKenhNhan:
        tot.sanLuongTacDungBTKenhNhan !== null && tot.sanLuongTacDungBTKenhNhan !== undefined
          ? Math.round(Number(tot.sanLuongTacDungBTKenhNhan))
          : '-',
      sanLuongTacDungCDKenhNhan:
        tot.sanLuongTacDungCDKenhNhan !== null && tot.sanLuongTacDungCDKenhNhan !== undefined
          ? Math.round(Number(tot.sanLuongTacDungCDKenhNhan))
          : '-',
      sanLuongTacDungTDKenhNhan:
        tot.sanLuongTacDungTDKenhNhan !== null && tot.sanLuongTacDungTDKenhNhan !== undefined
          ? Math.round(Number(tot.sanLuongTacDungTDKenhNhan))
          : '-',
      sanLuongPhanKhangTongKenhNhan:
        tot.sanLuongPhanKhangTongKenhNhan !== null && tot.sanLuongPhanKhangTongKenhNhan !== undefined
          ? Math.round(Number(tot.sanLuongPhanKhangTongKenhNhan))
          : '-',
    };
  }, [reportData?.total]);

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
          <div style={{ display: viewMode === 'table' ? 'block' : 'none' }}>
            <EnergyConsumptionTable
              data={reportData?.items || []}
              total={reportData?.total}
              loading={loading}
            />
          </div>
          <div
            ref={chartContainerRef}
            style={{ display: viewMode === 'chart' ? 'block' : 'none' }}
          >
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
          </div>
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='BÁO CÁO SẢN LƯỢNG ĐIỆN'
        chartRef={chartContainerRef.current}
        chartSheets={energyChartSheets}
        dataSource={exportDataSource}
        totalRow={exportTotalRow}
        summaryRows={exportSummaryRows}
        listHeader={energyConsumptionHeaders}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={''}
        serverExcelEndpoint={Endpoint.Report.ExportExcelEnergyConsumption}
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
              cycleType === 'HALF_HOUR'
                ? selectedDate.format('DD/MM/YYYY')
                : cycleType === 'MONTHLY'
                ? selectedMonth.format('MM/YYYY')
                : selectedYear.format('YYYY'),
          },
        ]}
      />
    </div>
  );
};

export default ElectricityConsumption;

