import { Empty, Typography } from 'antd';
import { ChartDataset, ChartOptions, ScriptableTooltipContext } from 'chart.js';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { GetFinalizedOutputElectricityChartRequest } from '@/core/application/dto/reports/requests/GetFinalizedOutputElectricityChartRequest.ts';
import StackedBarLineChart from '@/infrastructure/common/components/chart/stacked-bar-chart.tsx';
import useGetFinalizedOutputElectricityChartHook from '@/infrastructure/repository/reports/hooks/useGetFinalizedOutputElectricityChart.hook.tsx';

interface StackedBarLineContainerProps {
  filterData: any;
  dataForm: { customerCode: string; monitorPoint: string };
}

const FinalizedActivePowerOutputHourly = ({
  filterData,
  dataForm,
}: StackedBarLineContainerProps) => {
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

    // Create labels and extract values
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

    const normalValues = data?.currentPeriodData.map((item: any) => item.bt || null); // Ensure numeric values
    const peakValues = data?.currentPeriodData.map((item: any) => item.cd || null);
    const lowValues = data?.currentPeriodData.map((item: any) => item.td || null);

    const totalValues = data?.currentPeriodData.map((item: any) => {
      const normal = item.bt || 0;
      const peak = item.cd || 0;
      const low = item.td || 0;
      return normal + peak + low; // Sum up the values
    });

    return { labels, normalValues, peakValues, lowValues, totalValues };
  };

  async function chartPropsBuilder(data: any) {
    const { labels, normalValues, peakValues, lowValues, totalValues } = processDataForChart(data);

    if (
      labels.length === 0 ||
      normalValues.length === 0 ||
      peakValues.length === 0 ||
      lowValues.length === 0
    ) {
      return null; // No chart props if no data
    }

    const currentPeriodColorNormal = '#4caf50';
    const currentPeriodColorPeak = '#e87230';
    const currentPeriodColorLow = '#3b5ba5';
    const currentPeriodYear = dayjs(data.date).year();

    const maxValue = Math.max(...totalValues) || 1; // Ensure at least 1 to avoid issues
    const adjustedMax = Math.round(Math.ceil(maxValue * 1.2)); // Increase max limit by 20%

    const datasets: ChartDataset<'bar' | 'line'>[] = [
      {
        label: 'Thấp điểm',
        data: lowValues,
        backgroundColor: currentPeriodColorLow,
        stack: 'currentYear',
        type: 'bar',
      },
      {
        label: 'Bình thường',
        data: normalValues,
        backgroundColor: currentPeriodColorNormal,
        stack: 'currentYear',
        type: 'bar',
      },
      {
        label: 'Cao điểm',
        data: peakValues,
        backgroundColor: currentPeriodColorPeak,
        stack: 'currentYear',
        type: 'bar',
      },
    ];

    const chartOptions: ChartOptions<'bar' | 'line'> = {
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
          text: `${Constants.Reports.Type.ElectricityConsumption.SanLuongPChot.fullLabel} năm ${currentPeriodYear}`,
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
          itemSort: (a, b) => {
            // Keep items in the same order as the dataset
            return b.datasetIndex - a.datasetIndex;
          },
        },
      },
      scales: {
        y: {
          title: { display: true, text: data.unit || 'kWh' },
          beginAtZero: true,
          min: 0,
          max: adjustedMax,
          border: {
            color: 'rgba(102,102,102,0.6)', // Axis line color
            width: 1.5, // Axis line width
            dash: [5, 5], // Dashed axis line
          },
        },
        x: {
          title: { display: true, text: 'Thời gian' },
          border: {
            color: 'rgba(102,102,102,0.6)', // Axis line color
            width: 1.5, // Axis line width
            dash: [5, 5], // Dashed axis line
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

export default FinalizedActivePowerOutputHourly;
