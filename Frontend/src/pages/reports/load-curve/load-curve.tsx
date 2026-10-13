import { Card } from 'antd';
import { ChartDataset, ScriptableTooltipContext, TooltipItem } from 'chart.js';
import dayjs from 'dayjs';
import { useCallback, useEffect, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetLoadChartRequest } from '@/core/application/dto/reports/requests/GetLoadChartRequest.ts';
import { IGetLoadChartResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import SteppedLineChart from '@/infrastructure/common/components/chart/stepped-line-chart.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetLoadChartHook from '@/infrastructure/repository/reports/hooks/useGetLoadChart.hook.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import ChartFilter, { FilterStateValue } from '@/pages/reports/components/chart-filter.tsx';

const LoadChart = () => {
  const chartRef = useRef(null);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [getLoadChartRequest] = useGetLoadChartHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [showClearSearch, setShowClearSearch] = useState(false);
  const [listMeasurementPoint, setListMeasurementPoint] = useState<any>([]);
  const [listCustomer, setListCustomer] = useState<any>([]);
  const [chartData, setChartData] = useState<IGetLoadChartResponse | null>(null);
  const [filterData, setFilterData] = useState<FilterStateValue>({
    radioValue: Constants.Reports.TimeSelector.General.Daily.value,
    date: dayjs().toISOString(),
    chartType: Constants.Reports.Type.LoadChart.PhuTaiP.value,
    previousPeriod: false,
  });
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });
  const [chartProps, setChartProps] = useState({ data: null, options: null });
  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);

  const buildDailyChartLabels = () => {
    return Array.from({ length: 25 }, (_, i) => `${i === 24 ? '0' : i}:00`);
  };

  const buildMonthlyChartLabels = (numberOfDays: number) => {
    const labels = Array.from(
      { length: numberOfDays },
      (_, i) => `0:00 ngày ${String(i + 1).padStart(2, '0')}`
    );
    labels.push('0:00 ngày 01'); // Add the first day of the next month at the end
    return labels;
  };

  const buildYearlyChartLabels = () => {
    const labels = Array.from(
      { length: 13 },
      (_, i) => `0:00 ngày 01/${String(i !== 12 ? i + 1 : 1).padStart(2, '0')}`
    );
    // labels.push('0:00 ngày 01/01'); // Add the first day of the next year at the end
    return labels;
  };

  // Daily Chart Builder
  async function buildDailyChartProps(data: any, filterData: FilterStateValue) {
    const shiftedCurrentData = [
      ...data.currentPeriodData,
      { label: '0', value: null, unit: data.currentPeriodData[0]?.unit || '' },
    ];

    const shiftedPreviousData = [
      ...data.previousPeriodData,
      {
        label: '0',
        value: data.previousPeriodData[data.previousPeriodData.length - 1]?.value || null,
        unit: data.previousPeriodData[0]?.unit || '',
      },
    ];

    const labels = buildDailyChartLabels();
    // const currentPeriodValues = shiftedCurrentData.map((item: any) => item.value || null);
    const currentPeriodValues = shiftedCurrentData.map((item: any, index: number) => {
      if ((item.value === 0 || item.value === null) && index > 0) {
        return shiftedCurrentData[index - 1]?.value || null;
      }
      return item.value;
    });
    const previousPeriodValues = shiftedPreviousData.map((item: any) => item.value || null);

    return buildChartProps({
      labels,
      currentPeriodValues,
      previousPeriodValues,
      unit: shiftedCurrentData[0]?.unit || 'kW',
      filterData,
      YAxisLegend:
        filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value ? 'P' : 'Q',
      currentPeriodDate: data.date,
      previousPeriodDate: data.previousPeriodDate,
    });
  }

  // Monthly Chart Builder
  async function buildMonthlyChartProps(data: any, filterData: FilterStateValue) {
    const labels = buildMonthlyChartLabels(data.currentPeriodData?.length);
    const currentPeriodValuesWithZero = [
      ...data.currentPeriodData.map((item: any) => item.value || null),
      data.currentPeriodData[data.currentPeriodData.length - 1]?.value || null,
    ];
    const previousPeriodValuesWithZero = [
      ...data.previousPeriodData.map((item: any) => item.value || null),
      data.previousPeriodData[data.previousPeriodData.length - 1]?.value || null,
    ];

    return buildChartProps({
      labels: labels,
      currentPeriodValues: currentPeriodValuesWithZero,
      previousPeriodValues: previousPeriodValuesWithZero,
      unit: data.currentPeriodData[0]?.unit || 'kW',
      filterData,
      YAxisLegend:
        filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value ? 'P' : 'Q',
      currentPeriodDate: data.date,
      previousPeriodDate: data.previousPeriodDate,
    });
  }

  // Yearly Chart Builder
  async function buildYearlyChartProps(data: any, filterData: FilterStateValue) {
    const labels = buildYearlyChartLabels();
    const currentPeriodValues = data.currentPeriodData?.map((item: any) => item.value || null);
    const previousPeriodValues = data.previousPeriodData?.map((item: any) => item.value || null);

    const currentPeriodValuesWithZero = [
      ...currentPeriodValues,
      currentPeriodValues[currentPeriodValues - 1],
    ];
    const previousPeriodValuesWithZero = [
      ...previousPeriodValues,
      previousPeriodValues[previousPeriodValues - 1],
    ];
    return buildChartProps({
      labels: labels,
      currentPeriodValues: currentPeriodValuesWithZero,
      previousPeriodValues: previousPeriodValuesWithZero,
      unit: data.currentPeriodData[0]?.unit || 'kW',
      filterData,
      YAxisLegend:
        filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value ? 'P' : 'Q',
      currentPeriodDate: data.date,
      previousPeriodDate: data.previousPeriodDate,
    });
  }

  // Common Chart Props Builder
  async function buildChartProps({
    labels,
    currentPeriodValues,
    previousPeriodValues,
    unit,
    filterData,
    YAxisLegend,
    currentPeriodDate,
    previousPeriodDate,
  }: {
    labels: string[];
    currentPeriodValues: number[];
    previousPeriodValues: number[];
    currentPeriodDate: string;
    previousPeriodDate: string;
    unit: string;
    filterData: FilterStateValue;
    YAxisLegend: string;
  }) {
    const parsedCurrentPeriodDate = dayjs(currentPeriodDate); // Parse current period date
    const parsedPreviousPeriodDate = dayjs(previousPeriodDate); // Parse previous period date

    const labelBuilder = () => {
      switch (filterData?.radioValue) {
        case Constants.Reports.TimeSelector.General.Daily.value:
          return {
            current: parsedCurrentPeriodDate.format(Constants.DateTime.DateFormat),
            previous: parsedPreviousPeriodDate.format(Constants.DateTime.DateFormat),
            legend: Constants.Reports.TimeLabelUnit.Daily,
            title:
              filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value
                ? Constants.Reports.Type.LoadChart.PhuTaiP.label
                : Constants.Reports.Type.LoadChart.PhuTaiQ.label,
          };
        case Constants.Reports.TimeSelector.General.Monthly.value:
          return {
            current: parsedCurrentPeriodDate.format(Constants.DateTime.MonthYearFormat),
            previous: parsedPreviousPeriodDate.format(Constants.DateTime.MonthYearFormat),
            legend: Constants.Reports.TimeLabelUnit.Monthly,
            title:
              filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value
                ? Constants.Reports.Type.LoadChart.PhuTaiP.label
                : Constants.Reports.Type.LoadChart.PhuTaiQ.label,
          };
        case Constants.Reports.TimeSelector.General.Yearly.value:
          return {
            current: parsedCurrentPeriodDate.format(Constants.DateTime.YearFormat),
            previous: parsedPreviousPeriodDate.format(Constants.DateTime.YearFormat),
            legend: Constants.Reports.TimeLabelUnit.Yearly,
            title:
              filterData?.chartType === Constants.Reports.Type.LoadChart.PhuTaiP.value
                ? Constants.Reports.Type.LoadChart.PhuTaiP.label
                : Constants.Reports.Type.LoadChart.PhuTaiQ.label,
          };
      }
    };

    const datasets: ChartDataset<'line'>[] = [
      {
        label: labelBuilder()?.current,
        data: currentPeriodValues,
        borderColor: 'blue',
        backgroundColor: 'blue',
        stepped: 'before',
        fill: false,
        pointRadius: 0,
        type: 'line',
      },
      {
        label: labelBuilder()?.previous,
        data: previousPeriodValues,
        borderColor: 'rgba(102,102,102,0.2)',
        backgroundColor: 'rgba(102,102,102,0.2)',
        stepped: 'before',
        fill: false,
        pointRadius: 0,
        type: 'line',
      },
    ];
    return {
      data: {
        labels,
        datasets,
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          mode: 'index',
          intersect: false,
        },
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              boxWidth: 10,
              boxHeight: 10,
              pointStyle: 'circle',
              usePointStyle: true,
            },
          },
          title: {
            display: true,
            text: labelBuilder()?.title,
            font: { size: Constants.Chart.ChartOptions.FontSize },
          },
          tooltip: {
            enabled: (context: ScriptableTooltipContext<'bar' | 'line'>) => {
              // Check if the context has valid dataset and data point
              if (
                context.tooltipItems &&
                context.tooltipItems.length > 0 &&
                context.tooltipItems[0].datasetIndex !== undefined &&
                context.tooltipItems[0].dataIndex !== undefined
              ) {
                const datasetIndex = context.tooltipItems[0].datasetIndex;
                const dataIndex = context.tooltipItems[0].dataIndex;
                const value = context.chart.data.datasets[datasetIndex].data[dataIndex] as
                  | number
                  | null;

                // Disable tooltip if the value is null or 0
                return value !== null && value !== 0;
              }
              return true; // Disable tooltip for invalid context
            },
            callbacks: {
              title: (tooltipItems: any) => {
                return filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value
                  ? `${tooltipItems[0].label}`
                  : null;
              },
              label: (tooltipItem: TooltipItem<'line'>) => {
                if (tooltipItem.raw === null || tooltipItem.raw === undefined) {
                  return null; // Prevent tooltip from rendering
                }

                // const index = tooltipItem.dataIndex + 1;
                const raw = Number(tooltipItem.raw).toFixed(3);

                switch (filterData?.radioValue) {
                  case Constants.Reports.TimeSelector.General.Daily.value:
                    return `${tooltipItem.dataset.label}: ${raw} kW`;
                  case Constants.Reports.TimeSelector.General.Monthly.value:
                    return `${tooltipItem.label}: ${raw} kW`;
                  case Constants.Reports.TimeSelector.General.Yearly.value:
                    return `${tooltipItem.label}: ${raw} kW`;
                }
              },
            },
          },
        },
        scales: {
          y: {
            title: {
              display: true,
              text: `${YAxisLegend} (${unit})`,
            },
            border: {
              color: 'rgba(102,102,102,0.6)',
              width: 1.5,
              dash: [5, 5],
            },
            beginAtZero: true,
          },
          x: {
            offset: true,
            title: {
              display: true,
              text: labelBuilder()?.legend,
            },
            border: {
              color: 'rgba(102,102,102,0.6)',
              width: 1.5,
              dash: [5, 5],
            },
          },
        },
      },
    };
  }
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
        const customers = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.customerCode,
          };
        });
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
        const measurementPoints = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.code,
          };
        });
        setListMeasurementPoint(measurementPoints);
      },
      () => {},
      () => {}
    );
  };

  const getLoadChartAsync = async ({
    type,
    date,
    measurementPointId,
    timeSelector,
    customerId,
  }: GetLoadChartRequest) => {
    const params = {
      type,
      date,
      measurementPointId,
      timeSelector,
      customerId,
    };
    await getLoadChartRequest(
      params,
      (res) => {
        const data = res?.data;
        if (filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value) {
          const lastDataObject = data?.currentPeriodData?.pop();
          const lastPreviousDataObject = data?.previousPeriodData?.pop();
          if (lastDataObject) {
            data?.currentPeriodData?.unshift(lastDataObject);
          }
          if (lastPreviousDataObject) {
            data?.previousPeriodData?.unshift(lastPreviousDataObject);
          }
        }
        setChartData(data);
      },
      () => {},
      () => {}
    );
  };

  const onSearch = (key: any, data: string) => {
    setSearchData({ ...searchData, [key]: data });
  };

  const loadChartTypeArray = useCallback(() => {
    return Object.values(Constants.Reports.Type.LoadChart).map((type) => ({
      label: type.label,
      value: type.value,
    }));
  }, []);

  const clearSearch = () => {
    setDataForm({ customerCode: '', monitorPoint: '' });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setListMeasurementPoint([]);
    setChartData(null);
    setShowClearSearch(false);
  };

  const fileNameBuilder = () => {
    return `${filterData?.chartType}_${listCustomer?.find((customer: any) => customer.value === dataForm?.customerCode)?.label}_${listMeasurementPoint?.find((measurementPoint: any) => measurementPoint.value === dataForm?.monitorPoint)?.label}`;
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerCode) {
      setDataForm({ ...dataForm, monitorPoint: '' });
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
      setDataForm({ ...dataForm, monitorPoint: '' });
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerCode]);

  useEffect(() => {
    if (!dataForm?.customerCode || !dataForm?.monitorPoint) {
      setChartData(null);
    }
  }, [dataForm?.customerCode, dataForm?.monitorPoint]);

  useEffect(() => {
    if (chartData) {
      const fetchChartProps = async () => {
        let props;
        switch (filterData?.radioValue) {
          case Constants.Reports.TimeSelector.General.Daily.value:
            props = await buildDailyChartProps(chartData, filterData!);
            break;
          case Constants.Reports.TimeSelector.General.Monthly.value:
            props = await buildMonthlyChartProps(chartData, filterData!);
            break;
          case Constants.Reports.TimeSelector.General.Yearly.value:
            props = await buildYearlyChartProps(chartData, filterData!);
            break;
        }
        setChartProps(props as any);
      };
      fetchChartProps();
    } else {
      setChartProps({ data: null, options: null });
    }
  }, [chartData, filterData]);

  // Trigger chart refresh when filterData changes
  useEffect(() => {
    if (filterData && dataForm?.customerCode && dataForm?.monitorPoint) {
      getLoadChartAsync({
        type: filterData.chartType as any,
        date: filterData.date,
        measurementPointId: dataForm.monitorPoint,
        timeSelector: filterData.radioValue,
        customerId: dataForm.customerCode,
      });
    }
  }, [filterData, dataForm.customerCode, dataForm.monitorPoint]);

  useEffect(() => {
    // Update clear button visibility based on dataForm's current state
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);

  return (
    <div>
      <PageTitle title={'Báo cáo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[{ name: 'Báo cáo', link: ROUTE_PATH.REPORTS }, { name: 'Biểu đồ phụ tải' }]}
        />
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            disabled={!(chartProps?.data && chartProps?.options)}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div>
          <div className={'mb-10'}>
            <div className='flex gap-3 items-center'>
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
                    onClick={() => {
                      clearSearch();
                    }}
                    variant={'text'}
                  />
                </div>
              )}
            </div>
            <div className={'mt-5'}>
              <ChartFilter
                radioOptions={Constants.Reports.TimeSelector.LoadChart}
                chartTypeOptions={loadChartTypeArray()}
                setFilterData={setFilterData}
              />
            </div>
          </div>
          {chartProps?.data && chartProps?.options && (
            <div ref={chartRef}>
              <SteppedLineChart
                options={chartProps.options}
                data={chartProps.data}
                datasetIdKey='type'
              />
            </div>
          )}
        </div>
      </Card>
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Biểu đồ phụ tải'
        onlyPDF={true}
        chartRef={chartRef.current}
        listHeader={[]}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={''}
      />
    </div>
  );
};

export default LoadChart;
