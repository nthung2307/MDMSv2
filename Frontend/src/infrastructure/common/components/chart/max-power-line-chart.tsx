import { Card } from 'antd';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import { Line } from 'react-chartjs-2';

import { customDashedLinePlugin } from '@/infrastructure/utils/helpers.ts';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  customDashedLinePlugin
);

const MaxPowerLineChart = ({ datasetIdKey = 'label' }) => {
  // Labels for each hour of the day (1 to 24)
  const labels = Array.from({ length: 24 }, (_, i) => (i + 1).toString());

  const data = {
    labels,
    datasets: [
      {
        label: 'Max',
        data: [
          80, 110, 70, 130, 90, 150, 120, 100, 140, 85, 75, 160, 95, 105, 130, 120, 145, 170, 110,
          130, 150, 125, 90, 140,
        ], // Sample max power data in kW for each hour
        borderColor: 'orange',
        backgroundColor: 'rgba(255, 165, 0, 0.2)',
        tension: 0.4, // Adds smoothness to the line
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom',
      },
      title: {
        display: true,
        text: 'Biểu đồ công suất cực đại',
      },
    },
    scales: {
      x: {
        title: {
          display: true,
          text: 't (h)',
        },
      },
      y: {
        title: {
          display: true,
          text: 'P (kW)',
        },
        min: 0, // Adjust the minimum value if needed
        max: 210, // Adjust the maximum value based on your data range
      },
    },
  };

  return (
    <Card className={'flex justify-center bg-white'}>
      <Line
        data={data}
        options={options as any}
        className={'h-[500px]'}
        datasetIdKey={datasetIdKey}
      />
      ;
    </Card>
  );
};

export default MaxPowerLineChart;
