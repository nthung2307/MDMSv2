import { ChartData, ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import React, { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { IGetVoltageChartResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import LineChartCommon from '@/infrastructure/common/components/chart/line-common.tsx';
import useGetVoltageChartHook from '@/infrastructure/repository/reports/hooks/useGetVoltageChart.hook.tsx';

dayjs.extend(customParseFormat);

interface VoltageChartProps {
  filterData: {
    chartType: string;
    date: string;
    radioValue: string;
    previousPeriod?: boolean;
  };
  dataForm: { customerCode: string; monitorPoint: string };
}

const VoltageChart: React.FC<VoltageChartProps> = ({ filterData, dataForm }) => {
  const [getVoltageChartRequest] = useGetVoltageChartHook();
  const [chartData, setChartData] = useState<IGetVoltageChartResponse | null>(null);

  // Fetch chart data from API
  // const getVoltageChartAsync = async () => {
  //   if (!dataForm.customerCode || !dataForm.monitorPoint || !filterData) return;

  //   const params = {
  //     type: filterData.chartType,
  //     date: filterData.date,
  //     timeSelector: filterData.radioValue,
  //     measurementPointId: Number(dataForm.monitorPoint),
  //     customerId: Number(dataForm.customerCode),
  //   };

  //   await getVoltageChartRequest(
  //     params,
  //     (res) => {
  //       const data = res?.data;
  //       const lastDataObject = data?.currentPeriodData?.pop();
  //       if (lastDataObject) {
  //         data?.currentPeriodData?.unshift(lastDataObject);
  //       }
  //       setChartData(data);
  //     },
  //     () => {},
  //     () => {}
  //   );
  // };

  const getVoltageChartAsync = async () => {
    if (!dataForm.customerCode || !dataForm.monitorPoint || !filterData) return;

    const selectedDate = dayjs(filterData.date); // Parse the user-selected date
    const currentDate = dayjs(); // Get the current system date and time
    const isDaily = filterData.radioValue === Constants.Reports.TimeSelector.General.Daily.value;
    const isMonthly =
      filterData.radioValue === Constants.Reports.TimeSelector.General.Monthly.value;

    // Check if the selected date matches the current day/month
    const isCurrentDate =
      (isDaily && selectedDate.isSame(currentDate, 'day')) ||
      (isMonthly && selectedDate.isSame(currentDate, 'month'));

    const params = {
      type: filterData.chartType,
      date: filterData.date,
      timeSelector: filterData.radioValue,
      measurementPointId: Number(dataForm.monitorPoint),
      customerId: Number(dataForm.customerCode),
    };

    await getVoltageChartRequest(
      params,
      (res) => {
        const data = res?.data;

        if (!data || !data.currentPeriodData) {
          setChartData(null); // Handle case when there is no data
          return;
        }

        if (filterData.radioValue === Constants.Reports.TimeSelector.General.Daily.value) {
          const lastDataObject = data?.currentPeriodData?.pop();
          data?.currentPeriodData?.unshift(lastDataObject); // Shift last object to the beginning
        }

        // Only display data up to the current time if the user selects the current date/month
        if (isCurrentDate) {
          const currentHour = currentDate.hour(); // Get the current hour for daily filtering
          const currentDay = currentDate.date(); // Get the current day for monthly filtering

          data.currentPeriodData = isDaily
            ? data.currentPeriodData.slice(0, currentHour) // Include data up to the current hour
            : data.currentPeriodData.slice(0, currentDay); // Include data up to the current day
        }

        setChartData(data);
      },
      () => {},
      () => {}
    );
  };

  useEffect(() => {
    getVoltageChartAsync();
  }, [filterData, dataForm]);

  // Chart data and options builder
  const buildChartProps = (): {
    data: ChartData<'line'> | null;
    options: ChartOptions<'line'> | null;
  } => {
    if (!chartData) return { data: null, options: null };

    const isDaily = filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value;

    // Generate labels based on the time selector (Daily or Monthly)
    const labels = isDaily
      ? Array.from({ length: chartData.currentPeriodData.length }, (_, i) => `${i}:00`) // Daily: 0:00, 1:00, ..., 23:00, 0:00
      : Array.from(
          { length: chartData.currentPeriodData.length },
          (_, i) => `Ngày ${String(i + 1).padStart(2, '0')}` // Monthly: 0:00 ngày 01, ...
        );

    const unit = chartData.currentPeriodData.find((item) => item.phaAUnit)?.phaAUnit; // Check if there is any data for phase A
    const phaAValues = chartData.currentPeriodData.map((item) => item.phaA || null); // Default to 0 if null
    const phaBValues = chartData.currentPeriodData.map((item) => item.phaB || null);
    const phaCValues = chartData.currentPeriodData.map((item) => item.phaC || null);
    const currentPeriodMonthYear = dayjs(chartData.date).format('MM/YYYY');
    const currentPeriodDateMonthYear = dayjs(chartData.date).format('DD/MM/YYYY');

    const datasets: ChartDataset<'line'>[] = [
      {
        label: 'Va',
        data: phaAValues,
        borderColor: 'rgb(255, 165, 0)', // Line color
        backgroundColor: 'rgb(255, 165, 0)', // Area color (optional)
        tension: 0.4, // Curve
        type: 'line' as const,
        pointRadius: 0,
      },
      {
        label: 'Vb',
        data: phaBValues,
        borderColor: 'rgb(0, 128, 0)', // Line color
        backgroundColor: 'rgb(0, 128, 0)', // Area color (optional)
        tension: 0.4,
        pointRadius: 0,
        type: 'line' as const,
      },
      {
        label: 'Vc',
        data: phaCValues,
        borderColor: 'rgb(255, 0, 0)', // Line color
        backgroundColor: 'rgb(255, 0, 0)', // Area color (optional)
        tension: 0.4,
        pointRadius: 0,
        type: 'line' as const,
      },
    ];

    const data = {
      labels,
      datasets,
    };

    const options: ChartOptions<'line'> = {
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
          text: `${Constants.Reports.Type.OperatingParameter.DienApTrungBinh3Pha.label} ${isDaily ? `ngày ${currentPeriodDateMonthYear}` : `tháng ${currentPeriodMonthYear}`}`,
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
          mode: 'index',
          intersect: false,
          callbacks: {
            title: function (tooltipItems) {
              const index = tooltipItems[0].dataIndex;
              if (filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value) {
                return `${index}:00`;
              } else if (
                filterData?.radioValue === Constants.Reports.TimeSelector.General.Monthly.value
              ) {
                return `${index + 1}/${currentPeriodMonthYear}`;
              }
              return null as any;
            },
          },
        },
      },
      scales: {
        x: {
          offset: true,
          title: {
            display: true,
            text:
              filterData.radioValue === Constants.Reports.TimeSelector.General.Daily.value
                ? Constants.Reports.TimeLabelUnit.Daily
                : Constants.Reports.TimeLabelUnit.Monthly,
          },
          border: {
            color: 'rgba(102,102,102,0.6)', // Color of the axis line
            width: 1.5, // Makes the axis line bolder
            dash: [5, 5], // Dashed lines with a pattern of 5px dash and 5px gap
          },
        },
        y: {
          title: {
            display: true,
            text: `U (${unit})`,
          },
          border: {
            color: 'rgba(102,102,102,0.6)', // Color of the axis line
            width: 1.5, // Makes the axis line bolder
            dash: [5, 5], // Dashed lines with a pattern of 5px dash and 5px gap
          },
          beginAtZero: true, // Ensures the y-axis starts at 0
          min: 0, // Explicitly set the minimum value to 0
        },
      },
    };

    return { data, options } as any;
  };

  const { data, options } = buildChartProps();

  return (
    <div className={'flex flex-col'}>
      <div className={'mt-5'}>
        {data && options && <LineChartCommon data={data} options={options} />}
      </div>
    </div>
  );
};

export default VoltageChart;
