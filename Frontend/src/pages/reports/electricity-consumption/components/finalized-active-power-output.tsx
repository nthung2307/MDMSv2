import { Empty, Typography } from 'antd';
import { ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { GetFinalizedOutputElectricityChartRequest } from '@/core/application/dto/reports/requests/GetFinalizedOutputElectricityChartRequest.ts';
import StackedBarLineChart from '@/infrastructure/common/components/chart/stacked-bar-chart.tsx';
import useGetFinalizedOutputElectricityChartHook from '@/infrastructure/repository/reports/hooks/useGetFinalizedOutputElectricityChart.hook.tsx';
import { numberFormatter } from '@/infrastructure/utils/helpers.ts';
import { FilterStateValue } from '@/pages/reports/components/chart-filter';

interface FinalizedPowerOutputProps {
  filterData: FilterStateValue;
  dataForm: { customerCode: string; monitorPoint: string };
}

const FinalizedActivePowerOutput = ({ filterData, dataForm }: FinalizedPowerOutputProps) => {
  const [getFinalizedOutputElectricityChartRequest] = useGetFinalizedOutputElectricityChartHook();
  const [chartData, setChartData] = useState<any>(null);
  const [chartProps, setChartProps] = useState<{ data: any; options: any } | null>(null);

  const processDataForChart = (data: any) => {
    if (!data?.currentPeriodData || data.currentPeriodData.length === 0) {
      return { labels: [], values: [] }; // No data case
    }

    // // Current date filter as a dayjs object
    // const currentDateFilter = dayjs(filterData.date);

    // // Generate labels and values
    // const filteredData = data.currentPeriodData.filter((item: any) => {
    //   const currentDate = dayjs(`${item?.day}/${item?.month}/${item?.year}`, 'D/M/YYYY').subtract(
    //     1,
    //     'day'
    //   );
    //   // Keep only items that match the current date filter year
    //   return currentDate.year() === currentDateFilter.year();
    // });

    const labels = data?.currentPeriodData.map((item: any) => {
      const currentDate = dayjs(`${item?.day}/${item?.month}/${item?.year}`, 'D/M/YYYY')
        .subtract(1, 'day')
        .format('D/M/YYYY');
      const previousDate = dayjs(
        `${item?.dayPrevious}/${item?.monthPrevious}/${item?.yearPrevious}`,
        'D/M/YYYY'
      ).format('D/M/YYYY');
      if (!item.monthPrevious || !item.yearPrevious) {
        return `${currentDate}`;
      }

      // Combine the label
      return `${previousDate} - ${currentDate}`;
    });

    const values = data?.currentPeriodData.map((item: any) => item.sg || null);

    return { labels, values };
  };

  // Function to build chart properties
  async function chartPropsBuilder(data: any) {
    const { labels, values } = processDataForChart(data);

    if (labels.length === 0 || values.length === 0) {
      return null; // No chart props if no data
    }

    const maxValue = Math.max(...values) || 1; // Ensure non-zero maxValue
    const adjustedMax = Math.round(Math.ceil(maxValue * 1.2)); // Increase max limit by 20%

    const datasets: ChartDataset<'bar'>[] = [
      {
        label: 'Sản lượng',
        data: values,
        backgroundColor: '#e87230',
        borderColor: '#e87230',
        borderWidth: 1,
      },
    ];

    const chartOptions: ChartOptions<'bar'> = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
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
          text: `${Constants.Reports.Type.ElectricityConsumption.TongSanLuongPChot.fullLabel} năm ${dayjs(filterData.date).year()}`,
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
            title: (tooltipItems: any) => tooltipItems[0].label,
            label: (tooltipItem: any) =>
              `${tooltipItem.dataset.label}: ${numberFormatter.format(tooltipItem.raw as number)} kWh`,
          },
        },
      },
      scales: {
        y: {
          title: { display: true, text: data.unit || 'kWh' },
          beginAtZero: true,
          max: adjustedMax, // Dynamically adjust based on values
          ticks: {
            stepSize: Math.ceil(adjustedMax / 5), // Dynamically adjust step size
          },
          border: {
            color: 'rgba(102,102,102,0.6)',
            width: 1.5,
            dash: [5, 5],
          },
        },
        x: {
          title: { display: true, text: 'Thời gian' },
          border: {
            color: 'rgba(102,102,102,0.6)',
            width: 1.5,
            dash: [5, 5],
          },
          ticks: {
            maxRotation: 45, // Maximum tilt angle
            minRotation: 0, // Minimum tilt angle
            autoSkip: true, // Skip labels to avoid overlap
          },
        },
      },
    };

    return { data: { labels, datasets }, options: chartOptions };
  }

  // Function to fetch chart data
  const getFinalizedOutputAsync = async () => {
    if (!dataForm.customerCode || !dataForm.monitorPoint || !filterData) return;

    const params: GetFinalizedOutputElectricityChartRequest = {
      date: filterData.date,
      measurementPointId: Number(dataForm.monitorPoint),
      customerId: Number(dataForm.customerCode),
    };

    await getFinalizedOutputElectricityChartRequest(
      params,
      (res) => {
        setChartData(res?.data);
      },
      () => {},
      () => {}
    );
  };

  // Fetch and update chartProps when chartData or filterData changes
  useEffect(() => {
    getFinalizedOutputAsync();
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
      {chartProps?.data && chartProps?.options ? (
        <StackedBarLineChart
          data={chartProps.data}
          options={chartProps.options}
          datasetIdKey='type'
        />
      ) : (
        <Empty description={<Typography.Text>Không có dữ liệu chốt.</Typography.Text>}></Empty>
      )}
    </div>
  );
};

export default FinalizedActivePowerOutput;
