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

// Register components for Chart.js
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

const VoltageLineChart = ({ datasetIdKey = 'label' }) => {
  // Sample data for 24 hours
  const labels = Array.from({ length: 24 }, (_, i) => (i + 1).toString()); // Hours from 1 to 24

  const data = {
    labels,
    datasets: [
      {
        label: 'Va',
        data: [
          12.3, 11.8, 13.1, 12.5, 11.9, 12.8, 13.0, 12.3, 12.5, 12.8, 13.2, 12.6, 11.9, 12.4, 13.1,
          12.0, 12.3, 12.7, 11.8, 13.0, 12.2, 12.5, 12.7, 13.4,
        ],
        borderColor: 'orange',
        backgroundColor: 'rgba(255, 165, 0, 0.2)',
        tension: 0.4,
      },
      {
        label: 'Vb',
        data: [
          11.8, 12.3, 12.9, 12.2, 13.1, 12.7, 12.4, 13.3, 12.1, 12.0, 13.3, 12.4, 12.6, 13.0, 12.9,
          12.3, 12.8, 13.1, 12.5, 12.8, 12.6, 13.1, 12.2, 12.9,
        ],
        borderColor: 'red',
        backgroundColor: 'rgba(255, 0, 0, 0.2)',
        tension: 0.4,
      },
      {
        label: 'Vc',
        data: [
          12.5, 12.0, 13.3, 11.9, 12.8, 12.6, 12.9, 12.7, 13.0, 12.3, 12.7, 13.0, 12.8, 12.5, 12.6,
          12.1, 13.3, 12.7, 12.4, 13.1, 12.9, 12.3, 12.8, 12.7,
        ],
        borderColor: 'green',
        backgroundColor: 'rgba(0, 128, 0, 0.2)',
        tension: 0.4,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: 'Đồ thị điện áp',
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
          text: 'V (kV)',
        },
        min: 10, // Adjust the minimum value based on your data range
        max: 15, // Adjust the maximum value based on your data range
      },
    },
  };

  return (
    <Card className={'bg-white'}>
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

export default VoltageLineChart;
