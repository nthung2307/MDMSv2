import { ChartData, ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { GetVoltageChartRequest } from '@/core/application/dto/reports/requests/GetVoltageChartRequest.ts';
import { IGetPowerChartResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import LineChartCommon from '@/infrastructure/common/components/chart/line-common.tsx';
import useGetPowerChartHook from '@/infrastructure/repository/reports/hooks/useGetPowerChart.hook.tsx';

interface ReactivePowerProps {
  filterData: any;
  dataForm: { customerCode: string; monitorPoint: string };
}

const ReactivePower = ({ filterData, dataForm }: ReactivePowerProps) => {
  const [getPowerChartRequest] = useGetPowerChartHook();
  const [chartData, setChartData] = useState<IGetPowerChartResponse | null>(null);

  // Fetch chart data from API
  const getPowerChartAsync = async () => {
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

    const params: GetVoltageChartRequest = {
      type: filterData.chartType,
      date: filterData.date,
      timeSelector: filterData.radioValue,
      measurementPointId: Number(dataForm.monitorPoint),
      customerId: Number(dataForm.customerCode),
    };

    await getPowerChartRequest(
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
    getPowerChartAsync();
  }, [filterData, dataForm]);

  // Chart data and options builder
  const buildChartProps = (): {
    data: ChartData<'line'> | null;
    options: ChartOptions<'line'> | null;
  } => {
    if (!chartData) return { data: null, options: null };

    const isDaily = filterData?.radioValue === Constants.Reports.TimeSelector.General.Daily.value;

    const labels = isDaily
      ? Array.from({ length: chartData.currentPeriodData.length }, (_, i) => `${i}:00`) // Daily: 0:00, 1:00, ..., 23:00, 0:00
      : Array.from(
          { length: chartData.currentPeriodData.length },
          (_, i) => `Ngày ${String(i + 1).padStart(2, '0')}` // Monthly: 0:00 ngày 01, ...
        );
    const unit = chartData.currentPeriodData.find((item) => item.unit)?.unit; // Assuming API provides `unit`

    // Extract Min and Max values
    const minValues = chartData.currentPeriodData.map((item) => item.valueMin || null); // Assuming API provides `valueMin`
    const maxValues = chartData.currentPeriodData.map((item) => item.valueMax || null); // Assuming API provides `valueMax`

    const currentPeriodMonthYear = dayjs(chartData.date).format('MM/YYYY');
    const currentPeriodDateMonthYear = dayjs(chartData.date).format('DD/MM/YYYY');

    const datasets: ChartDataset<'line'>[] = [
      {
        label: 'Min',
        data: minValues,
        borderColor: 'blue',
        backgroundColor: 'blue',
        tension: 0.4,
        pointRadius: 0,
        fill: false,
      },
      {
        label: 'Max',
        data: maxValues,
        borderColor: 'red',
        backgroundColor: 'red',
        tension: 0.4,
        fill: false,
        pointRadius: 0,
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
          text: `Công suất phản kháng ${isDaily ? `ngày ${currentPeriodDateMonthYear}` : `tháng ${currentPeriodMonthYear}`}`,
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
            text: `Q (${unit})`,
          },
          border: {
            color: 'rgba(102,102,102,0.6)', // Color of the axis line
            width: 1.5, // Makes the axis line bolder
            dash: [5, 5], // Dashed lines with a pattern of 5px dash and 5px gap
          },
        },
      },
    };

    return { data, options } as any;
  };

  const { data, options } = buildChartProps();

  return (
    <div>
      <div className={'mt-5'}>
        {data && options ? (
          <LineChartCommon data={data} options={options} />
        ) : (
          <div>No data available</div>
        )}
      </div>
    </div>
  );
};

export default ReactivePower;
