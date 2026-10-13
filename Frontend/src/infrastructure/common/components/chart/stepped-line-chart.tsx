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

type SteppedLineChartProps = {
  data: ChartData<'line'>;
  options: ChartOptions<'line'>;
  datasetIdKey?: string;
};

const SteppedLineChart = ({ data, options, datasetIdKey = 'label' }: SteppedLineChartProps) => {
  return (
    <Card className={'bg-white'}>
      <Line data={data} options={options} className={'h-[500px]'} datasetIdKey={datasetIdKey} />
    </Card>
  );
};

export default SteppedLineChart;
