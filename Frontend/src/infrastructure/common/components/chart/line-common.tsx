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
  ChartData,
  ChartOptions,
} from 'chart.js';
import React from 'react';
import { Line } from 'react-chartjs-2';

import { customDashedLinePlugin } from '@/infrastructure/utils/helpers.ts';

// Register the components for Chart.js
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

// Define the props interface
interface LineChartProps {
  data: ChartData<'line'>;
  options: ChartOptions<'line'>;
  datasetIdKey?: string;
}

const LineChartCommon: React.FC<LineChartProps> = ({ data, options, datasetIdKey = 'label' }) => {
  return (
    <Card className={'bg-white'}>
      <Line
        data={data}
        options={options}
        className={'h-[500px] w-full'}
        datasetIdKey={datasetIdKey}
      />
    </Card>
  );
};

export default LineChartCommon;
