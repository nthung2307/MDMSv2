import { Card } from 'antd';
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  ChartData,
  ChartOptions,
  Legend,
  LinearScale,
  Title,
  Tooltip,
} from 'chart.js';
import React, { useMemo } from 'react';
import { Bar } from 'react-chartjs-2';

import { EnergyConsumptionChartData } from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface EnergyConsumptionChartProps {
  chartData: EnergyConsumptionChartData;
  title: string;
  compareWithPrevious?: boolean;
}

export const EnergyConsumptionChart: React.FC<EnergyConsumptionChartProps> = ({
  chartData,
  title,
  compareWithPrevious = false,
}) => {
  const data: ChartData<'bar'> = useMemo(() => {
    const labels = chartData?.labels || [];
    const datasets: any[] = [
      {
        label: 'Sản lượng Kênh Giao (kỳ này)',
        data: chartData?.currentPeriodGiao || [],
        backgroundColor: 'rgba(239, 68, 68, 0.85)',
        borderColor: 'rgb(239, 68, 68)',
        borderWidth: 1,
        borderRadius: 4,
      },
      {
        label: 'Sản lượng Kênh Nhận (kỳ này)',
        data: chartData?.currentPeriodNhan || [],
        backgroundColor: 'rgba(16, 185, 129, 0.85)',
        borderColor: 'rgb(16, 185, 129)',
        borderWidth: 1,
        borderRadius: 4,
      },
    ];

    if (compareWithPrevious) {
      datasets.push({
        label: 'Sản lượng Kênh Giao (kỳ trước)',
        data: chartData?.previousPeriodGiao || [],
        backgroundColor: 'rgba(249, 115, 22, 0.45)',
        borderColor: 'rgb(249, 115, 22)',
        borderWidth: 1,
        borderDash: [4, 4],
        borderRadius: 4,
      });
      datasets.push({
        label: 'Sản lượng Kênh Nhận (kỳ trước)',
        data: chartData?.previousPeriodNhan || [],
        backgroundColor: 'rgba(59, 130, 246, 0.45)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
        borderDash: [4, 4],
        borderRadius: 4,
      });
    }

    return {
      labels,
      datasets,
    };
  }, [chartData, compareWithPrevious]);

  const options: ChartOptions<'bar'> = useMemo(
    () => ({
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        mode: 'index',
        intersect: false,
      },
      plugins: {
        legend: {
          position: 'top',
          labels: {
            boxWidth: 14,
            usePointStyle: true,
            font: {
              size: 12,
            },
          },
        },
        title: {
          display: true,
          text: title,
          font: {
            size: 15,
            weight: 'bold',
          },
          padding: {
            top: 10,
            bottom: 20,
          },
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.dataset.label || '';
              const val = context.raw as number;
              return `${label}: ${Number(val).toLocaleString('vi-VN', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              })} kWh`;
            },
          },
        },
      },
      scales: {
        x: {
          grid: {
            display: false,
          },
          ticks: {
            maxRotation: 45,
            minRotation: 0,
            autoSkip: true,
            maxTicksLimit: 25,
          },
        },
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: 'Sản lượng (kWh)',
            font: {
              size: 13,
              weight: 'bold',
            },
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.06)',
          },
          ticks: {
            callback: (val) => Number(val).toLocaleString('vi-VN'),
          },
        },
      },
    }),
    [title]
  );

  return (
    <Card className='shadow-sm'>
      <div className='w-full h-[520px]'>
        <Bar data={data} options={options} />
      </div>
    </Card>
  );
};

export default EnergyConsumptionChart;
