import { ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import React, { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { GetLoadChartRequest } from '@/core/application/dto/reports/requests/GetLoadChartRequest.ts';
import ComboChartWithTable from '@/infrastructure/common/components/chart/combo-chart.tsx';
import useGetConsumptionOutputChartHook from '@/infrastructure/repository/reports/hooks/useGetConsumptionOutputChart.hook.tsx';
import { numberFormatter } from '@/infrastructure/utils/helpers.ts';

interface ComboChartContainerProps {
  filterData: any;
  dataForm: { customerCode: string; monitorPoint: string };
}

const ActivePowerOutputChart: React.FC<ComboChartContainerProps> = ({ filterData, dataForm }) => {
  const [getConsumptionOutputChartRequest] = useGetConsumptionOutputChartHook();
  const [chartData, setChartData] = useState(null);
  const [chartProps, setChartProps] = useState<{ data: any; options: any } | null>(null);

  // Function to build chart properties

  const chartPropsBuilder = async (data: any) => {
    const isDaily = filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value;

    const adjustYAxis = (datasets: ChartDataset<'bar' | 'line'>[]) => {
      // Filter out hidden datasets
      const visibleData = datasets
        .filter((dataset) => !dataset.hidden)
        .flatMap((dataset) => dataset.data);

      // Calculate max value for visible datasets
      if (visibleData.length > 0) {
        const maxValue = Math.max(...(visibleData as number[])) || 1;
        const bufferValue = Math.ceil(maxValue * 1.2); // Add 20% buffer
        const roundedMax = Math.pow(10, Math.floor(Math.log10(bufferValue))); // Nearest power of 10
        return Math.ceil(bufferValue / roundedMax) * roundedMax; // Round up to nearest step
      }

      return 1; // Default max value if no datasets are visible
    };

    // Generate labels based on the time selector (daily or monthly)
    const labels = isDaily
      ? data.currentPeriodData.map((_item: any, index: number) => {
          // Daily format: "0:00 - 0:59", "1:00 - 1:59", ..., "23:00 - 23:59"
          return `${index}:00 - ${index}:59`;
        })
      : data.currentPeriodData.map((_: any, index: number) => {
          // Monthly format: "0:00 - 23:59 ngày 01", "0:00 - 23:59 ngày 02", ...
          const day = String(index + 1).padStart(2, '0');
          return `0:00 - 23:59 ngày ${day}`;
        });

    const currentPeriodValues = data.currentPeriodData.map((item: any) => item.sg || null);

    const previousPeriodValues = data.previousPeriodData.map(
      (item: any) => item.previousSg || null
    );

    // Calculate max value and round it up
    const maxValue = Math.max(...currentPeriodValues, ...previousPeriodValues) || 1;
    const bufferValue = Math.ceil(maxValue * 1.2); // Add a 20% buffer
    const roundedMax = Math.pow(10, Math.floor(Math.log10(bufferValue))); // Get the nearest power of 10
    const adjustedMax = Math.ceil(bufferValue / roundedMax) * roundedMax; // Round up to the nearest roundedMax

    const currentPeriodColor = '#e87230';
    const previousPeriodColor = 'rgba(54, 162, 235, 1)';
    const currentPeriodDate = dayjs(data.date);
    const currentPeriodYear = currentPeriodDate.year();
    const currentPeriodMonth = currentPeriodDate.month() + 1;

    const currentLabel =
      filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value
        ? `${currentPeriodDate.format('DD/MM/YYYY')}`
        : `${currentPeriodMonth}/${currentPeriodYear}`;

    const previousLabel =
      filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value
        ? `${currentPeriodDate.subtract(1, 'day').format('DD/MM/YYYY')}`
        : `${currentPeriodMonth === 1 ? 12 : currentPeriodMonth - 1}/${
            currentPeriodMonth === 1 ? currentPeriodYear - 1 : currentPeriodYear
          }`;

    const buildDatasets = () => {
      const datasets: ChartDataset<'bar' | 'line'>[] = [
        {
          id: 'p-period-apo',
          type: 'line',
          label: previousLabel,
          data: previousPeriodValues,
          backgroundColor: previousPeriodColor,
          borderColor: previousPeriodColor,
          fill: false,
          pointHoverRadius: 5,
          pointRadius: 0,
        } as ChartDataset<'line'> & { id: string },
        {
          id: 'current-period-apo',
          label: currentLabel,
          type: 'bar',
          data: currentPeriodValues,
          backgroundColor: currentPeriodColor,
          borderColor: currentPeriodColor,
          pointHoverRadius: 5,
          pointRadius: 0,
        } as ChartDataset<'bar'> & { id: string },
      ];

      return datasets;
    };

    const datasets = buildDatasets();

    const chartOptions: ChartOptions<'bar' | 'line'> = {
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
          onClick: (_, legendItem, legend) => {
            const index = legendItem.datasetIndex;
            const chart = legend.chart;
            chart.data.datasets[index as number].hidden =
              !chart.data.datasets[index as number].hidden;
            const newMax = adjustYAxis(chart.data.datasets as ChartDataset<'bar' | 'line'>[]);
            chart.options.scales!.y!.max = newMax; // Dynamically adjust Y-axis max
            chart.update(); // Update the chart
          },
        },
        title: {
          display: true,
          text: `${Constants.Reports.Type.ElectricityConsumption.TongSanLuongPTieuThu.fullLabel} ${
            isDaily
              ? `ngày ${currentPeriodDate.format('DD/MM/YYYY')}`
              : `tháng ${currentPeriodMonth} năm ${currentPeriodYear}`
          }`,
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
              // Skip tooltip title if raw data is null
              if (tooltipItems[0].raw === null || tooltipItems[0].raw === undefined) {
                return null as any; // Prevent tooltip from rendering
              }
              return tooltipItems[0].label; // Otherwise, show the label
            },
            label: function (tooltipItem) {
              const datasetLabel = tooltipItem.dataset.label; // The dataset's label
              const value = tooltipItem.raw; // Raw value of the data point

              // Check if the data point is null or undefined
              if (value === null || value === undefined) {
                return null as any; // Skip rendering this tooltip
              }

              // Format and return the label
              return `${datasetLabel}: ${numberFormatter.format(value as number)} kW`;
            },
          },
        },
      },
      scales: {
        y: {
          title: { display: true, text: 'kWh' },
          beginAtZero: true,
          max: adjustedMax,
          border: {
            color: 'rgba(102,102,102,0.6)', // Axis line color
            width: 1.5, // Makes the axis line bolder
            dash: [5, 5], // Dashed lines with a pattern of 5px dash and 5px gap
          },
        },
        x: {
          title: {
            display: true,
            text: isDaily
              ? Constants.Reports.TimeLabelUnit.Daily
              : Constants.Reports.TimeLabelUnit.Monthly,
          },
          ticks: {
            autoSkip: true,
            maxRotation: 45, // Rotate labels if too crowded
            minRotation: 0,
          },
          border: {
            color: 'rgba(102,102,102,0.6)', // Axis line color
            width: 1.5, // Makes the axis line bolder
            dash: [5, 5],
          },
        },
      },
    };

    return { data: { labels, datasets }, options: chartOptions };
  };

  // Function to fetch chart data
  const getConsumptionOutputAsync = async () => {
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

    const params: GetLoadChartRequest = {
      date: filterData.date,
      measurementPointId: Number(dataForm.monitorPoint),
      timeSelector: filterData.radioValue,
      customerId: Number(dataForm.customerCode),
    };

    await getConsumptionOutputChartRequest(
      params,
      (res) => {
        const data = res?.data;

        if (!data || !data.currentPeriodData) {
          setChartData(null); // Handle case when there is no data
          return;
        }

        if (isDaily && Array.isArray(data.currentPeriodData)) {
          const lastObjectValue =
            data.currentPeriodData.length > 0 ? data.currentPeriodData.pop() : null;
          if (lastObjectValue) {
            data.currentPeriodData.unshift(lastObjectValue); // Move the last object to the beginning
          }
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

  // Fetch and update chartProps when chartData or filterData changes
  useEffect(() => {
    getConsumptionOutputAsync();
  }, [filterData, dataForm]);

  useEffect(() => {
    let isMounted = true;

    if (chartData) {
      const fetchChartProps = async () => {
        const props = await chartPropsBuilder(chartData);
        if (isMounted) {
          setChartProps(props);
        }
      };
      fetchChartProps();
    } else {
      setChartProps({ data: null, options: null });
    }

    return () => {
      isMounted = false;
    };
  }, [chartData]);

  return (
    <div>
      {chartProps?.data && chartProps?.options && filterData?.radioValue && (
        <ComboChartWithTable
          key={JSON.stringify(chartProps)}
          data={chartProps.data}
          options={chartProps.options}
          datasetIdKey='id'
          redraw={true}
        />
      )}
    </div>
  );
};

export default ActivePowerOutputChart;
