import { ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import React, { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { GetLoadChartRequest } from '@/core/application/dto/reports/requests/GetLoadChartRequest.ts';
import StackedBarLineChart from '@/infrastructure/common/components/chart/stacked-bar-chart.tsx';
import useGetConsumptionOutputChartHook from '@/infrastructure/repository/reports/hooks/useGetConsumptionOutputChart.hook.tsx';
import { numberFormatter } from '@/infrastructure/utils/helpers.ts';

interface StackedBarLineContainerProps {
  filterData: any;
  dataForm: { customerCode: string; monitorPoint: string };
}

const ActivePowerOutputHourly: React.FC<StackedBarLineContainerProps> = ({
  filterData,
  dataForm,
}) => {
  const [getConsumptionOutputChartRequest] = useGetConsumptionOutputChartHook();
  const [chartData, setChartData] = useState(null);
  const [chartProps, setChartProps] = useState<{ data: any; options: any } | null>(null);

  const chartPropsBuilder = async (data: any) => {
    const isDaily = filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value;
    const currentPeriodColorNormal = '#4caf50';
    const currentPeriodColorPeak = '#e87230';
    const currentPeriodColorLow = '#3b5ba5';

    // Generate labels based on the time selector (daily or monthly)
    const labels = data.currentPeriodData.map((_item: any, index: number) => {
      // Monthly format: "0:00 - 23:59 ngày 01", "0:00 - 23:59 ngày 02", ...
      const day = String(index + 1).padStart(2, '0');
      return `0:00 - 23:59 ngày ${day}`;
    });

    // Compute current and previous period values
    const currentPeriodValues = data.currentPeriodData.map((item: any) => {
      const normal = item.bt || 0;
      const peak = item.cd || 0;
      const low = item.td || 0;
      return normal + peak + low > 0 ? normal + peak + low : null; // Sum up all values for this period
    });

    const lowValues = data.currentPeriodData.map((item: any) => item.td || null);
    const normalValues = data.currentPeriodData.map((item: any) => item.bt || null);
    const peakValues = data.currentPeriodData.map((item: any) => item.cd || null);

    const previousPeriodValues = filterData?.previousPeriod
      ? labels.map((_label: any, index: any) => {
          const matchingDay = data.previousPeriodData.find(
            (item: any) => `${index + 1}` === item.label
          );
          if (!matchingDay) return 0;

          const normal = matchingDay.bt || 0;
          const peak = matchingDay.cd || 0;
          const low = matchingDay.td || 0;
          return normal + peak + low > 0 ? normal + peak + low : null; // Sum up all values for this period
        })
      : data.previousPeriodData.map((item: any) => {
          const normal = item.bt || 0;
          const peak = item.cd || 0;
          const low = item.td || 0;
          return normal + peak + low > 0 ? normal + peak + low : null; // Sum up all values for this period
        });

    // Calculate max value and add a 20% buffer
    const maxValue = Math.max(...currentPeriodValues, ...previousPeriodValues) || 1;
    const adjustedMax = Math.ceil(maxValue * 1.2); // Increase max limit by 20%

    const previousPeriodColor = 'rgba(54, 162, 235, 1)';
    const currentPeriodDate = dayjs(data.date);
    const currentPeriodYear = currentPeriodDate.year();
    const currentPeriodMonth = currentPeriodDate.month() + 1;

    const previousLabel =
      filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value
        ? `${currentPeriodDate.subtract(1, 'day').format('DD/MM/YYYY')}`
        : `${currentPeriodMonth === 1 ? 12 : currentPeriodMonth - 1}/${
            currentPeriodMonth === 1 ? currentPeriodYear - 1 : currentPeriodYear
          }`;

    const buildDatasets = () => {
      const datasets: ChartDataset<'bar' | 'line'>[] = [
        {
          id: 'prev-month-apoh',
          type: 'line',
          label: previousLabel,
          data: previousPeriodValues,
          backgroundColor: previousPeriodColor,
          borderColor: previousPeriodColor,
          fill: false,
          pointRadius: 0,
        } as ChartDataset<'line'> & { id: string },
        {
          id: 'current-apoh-low',
          label: 'Thấp điểm',
          data: lowValues,
          backgroundColor: currentPeriodColorLow,
          stack: 'currentMonth',
          type: 'bar',
        } as ChartDataset<'bar'> & { id: string },
        {
          id: 'current-apoh-normal',
          label: 'Bình thường',
          data: normalValues,
          backgroundColor: currentPeriodColorNormal,
          stack: 'currentMonth',
          type: 'bar',
        } as ChartDataset<'bar'> & { id: string },
        {
          id: 'current-apoh-peak',
          label: 'Cao điểm',
          data: peakValues,
          backgroundColor: currentPeriodColorPeak,
          stack: 'currentMonth',
          type: 'bar',
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
        },
        title: {
          display: true,
          text: `${Constants.Reports.Type.ElectricityConsumption.SanLuongPTieuThu.fullLabel} ${
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
              return tooltipItems[0].label; // The bar label (e.g., "0:00 - 0:59", etc.)
            },
            label: function (tooltipItem) {
              const datasetLabel = tooltipItem.dataset.label; // The dataset's label (e.g., date)
              const value = numberFormatter.format(tooltipItem.raw as number); // Format the value
              return `${datasetLabel}: ${value}`; // Format: "date: value"
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
            text: Constants.Reports.TimeLabelUnit.Monthly,
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

  // Fetch and update chartProps when chartData or filterData changes
  useEffect(() => {
    getConsumptionOutputAsync();
  }, [filterData, dataForm]);

  useEffect(() => {
    if (chartData) {
      const fetchChartProps = async () => {
        const props = await chartPropsBuilder(chartData);
        setChartProps(props);
      };
      fetchChartProps();
    } else {
      setChartProps({ data: null, options: null });
    }
  }, [chartData]);

  return (
    <div>
      {chartProps?.data && chartProps?.options && (
        <StackedBarLineChart
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

export default ActivePowerOutputHourly;
