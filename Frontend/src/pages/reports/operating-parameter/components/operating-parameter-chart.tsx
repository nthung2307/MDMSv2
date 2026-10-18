import { Card, Empty, Radio } from 'antd';
import {
  CategoryScale,
  Chart as ChartJS,
  ChartData,
  ChartOptions,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import React, { useMemo, useState } from 'react';
import { Line } from 'react-chartjs-2';
import { FiActivity, FiCpu, FiZap } from 'react-icons/fi';

import { OperatingParameterItem } from '@/core/application/dto/reports/requests/GetOperatingParameterReportRequest';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface OperatingParameterChartProps {
  data: OperatingParameterItem[];
  loading?: boolean;
  voltageUnit?: string;
}

type ChartCategory =
  | 'voltage'
  | 'current'
  | 'power'
  | 'pf'
  | 'frequency';

export const OperatingParameterChart: React.FC<OperatingParameterChartProps> = ({
  data,
  voltageUnit: propVoltageUnit,
}) => {
  const [selectedChart, setSelectedChart] = useState<ChartCategory>('voltage');

  const voltageUnit = useMemo(() => {
    if (propVoltageUnit) return propVoltageUnit;
    const itemWithUnit = data.find((d) => d.dienApPhaAUnit);
    return itemWithUnit?.dienApPhaAUnit || 'kV';
  }, [data, propVoltageUnit]);

  // Sắp xếp dữ liệu theo thời gian tăng dần để vẽ biểu đồ từ trái qua phải
  const sortedData = useMemo(() => {
    if (!data || !data.length) return [];
    return [...data].sort((a, b) => {
      const timeA = a.createdTime ? new Date(a.createdTime).getTime() : 0;
      const timeB = b.createdTime ? new Date(b.createdTime).getTime() : 0;
      return timeA - timeB;
    });
  }, [data]);

  // Labels trục X: Thời gian (nếu nhiều ngày thì kèm ngày/tháng)
  const timeLabels = useMemo(() => {
    if (!sortedData.length) return [];
    const firstDate = sortedData[0]?.createdTime
      ? new Date(sortedData[0].createdTime).toDateString()
      : '';
    const lastDate = sortedData[sortedData.length - 1]?.createdTime
      ? new Date(sortedData[sortedData.length - 1].createdTime).toDateString()
      : '';
    const isMultiDay = firstDate !== lastDate;

    return sortedData.map((item) => {
      if (!item.createdTime) return '';
      const d = new Date(item.createdTime);
      if (isMultiDay) {
        const timeStr = d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        const dateStr = d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
        return `${timeStr} ${dateStr}`;
      }
      return d.toLocaleTimeString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit',
      });
    });
  }, [sortedData]);

  // Cấu hình datasets theo từng loại biểu đồ
  const chartConfig = useMemo((): {
    chartData: ChartData<'line'>;
    unit: string;
    suggestedMin?: number;
    suggestedMax?: number;
  } => {
    switch (selectedChart) {
      case 'voltage': {
        return {
          unit: voltageUnit,
          chartData: {
            labels: timeLabels,
            datasets: [
              {
                label: `Điện áp pha A (${voltageUnit})`,
                data: sortedData.map((d) => d.dienApPhaA ?? null),
                borderColor: '#f5222d',
                backgroundColor: 'rgba(245, 34, 45, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
              {
                label: `Điện áp pha B (${voltageUnit})`,
                data: sortedData.map((d) => d.dienApPhaB ?? null),
                borderColor: '#fa8c16',
                backgroundColor: 'rgba(250, 140, 22, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
              {
                label: `Điện áp pha C (${voltageUnit})`,
                data: sortedData.map((d) => d.dienApPhaC ?? null),
                borderColor: '#1890ff',
                backgroundColor: 'rgba(24, 144, 255, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
            ],
          },
        };
      }
      case 'current': {
        return {
          unit: 'A',
          chartData: {
            labels: timeLabels,
            datasets: [
              {
                label: 'Dòng điện pha A (A)',
                data: sortedData.map((d) => d.dongDienPhaA ?? null),
                borderColor: '#f5222d',
                backgroundColor: 'rgba(245, 34, 45, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
              {
                label: 'Dòng điện pha B (A)',
                data: sortedData.map((d) => d.dongDienPhaB ?? null),
                borderColor: '#fa8c16',
                backgroundColor: 'rgba(250, 140, 22, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
              {
                label: 'Dòng điện pha C (A)',
                data: sortedData.map((d) => d.dongDienPhaC ?? null),
                borderColor: '#1890ff',
                backgroundColor: 'rgba(24, 144, 255, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
            ],
          },
        };
      }
      case 'power': {
        return {
          unit: 'kW / kVAr',
          chartData: {
            labels: timeLabels,
            datasets: [
              {
                label: 'Công suất tác dụng (kW)',
                data: sortedData.map((d) => d.congSuatTacDungTong3Pha ?? null),
                borderColor: '#52c41a',
                backgroundColor: 'rgba(82, 196, 26, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
              {
                label: 'Công suất phản kháng (kVAr)',
                data: sortedData.map((d) => d.congSuatPhanKhangTong3Pha ?? null),
                borderColor: '#722ed1',
                backgroundColor: 'rgba(114, 46, 209, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
            ],
          },
        };
      }
      case 'pf': {
        return {
          unit: '',
          suggestedMin: 0,
          suggestedMax: 1,
          chartData: {
            labels: timeLabels,
            datasets: [
              {
                label: 'Hệ số công suất',
                data: sortedData.map((d) => d.heSoCongSuatPF ?? null),
                borderColor: '#13c2c2',
                backgroundColor: 'rgba(19, 194, 194, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
            ],
          },
        };
      }
      case 'frequency': {
        return {
          unit: 'Hz',
          chartData: {
            labels: timeLabels,
            datasets: [
              {
                label: 'Tần số (Hz)',
                data: sortedData.map((d) => d.tanSoHeThong ?? null),
                borderColor: '#eb2f96',
                backgroundColor: 'rgba(235, 47, 150, 0.1)',
                tension: 0.3,
                pointRadius: 3,
                pointHoverRadius: 6,
              },
            ],
          },
        };
      }
      default:
        return {
          unit: '',
          chartData: { labels: [], datasets: [] },
        };
    }
  }, [selectedChart, timeLabels, sortedData, voltageUnit]);

  // Options Chart.js
  const options: ChartOptions<'line'> = useMemo(() => {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: {
            usePointStyle: true,
            boxWidth: 8,
            font: { family: 'inherit', size: 12 },
          },
        },
        tooltip: {
          callbacks: {
            title: (tooltipItems) => {
              const idx = tooltipItems[0]?.dataIndex;
              if (idx !== undefined && sortedData[idx]?.createdTime) {
                const d = new Date(sortedData[idx].createdTime);
                return d.toLocaleString('vi-VN', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                });
              }
              return tooltipItems[0]?.label || '';
            },
            label: (context) => {
              const label = context.dataset.label || '';
              const val = context.parsed.y;
              if (val === null || val === undefined) return `${label}: -`;
              return `${label}: ${Number(val).toLocaleString('vi-VN')} ${chartConfig.unit}`.trim();
            },
          },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: {
            maxRotation: 45,
            minRotation: 0,
            autoSkip: true,
            maxTicksLimit: 24,
          },
        },
        y: {
          grid: { color: '#f0f0f0' },
          suggestedMin: chartConfig.suggestedMin,
          suggestedMax: chartConfig.suggestedMax,
          ticks: {
            callback: (val) => `${val} ${chartConfig.unit}`.trim(),
          },
        },
      },
    };
  }, [chartConfig, sortedData]);

  if (!sortedData || !sortedData.length) {
    return (
      <Card className='mt-2 bg-white flex justify-center items-center py-16 shadow-sm border border-gray-100 rounded-lg'>
        <Empty
          description={
            <span className='text-gray-500'>
              Chưa có dữ liệu thông số vận hành để hiển thị biểu đồ.
              <br />
              Vui lòng chọn khách hàng, điểm đo và bấm <b>&quot;Truy vấn&quot;</b>.
            </span>
          }
        />
      </Card>
    );
  }

  return (
    <div className='flex flex-col gap-4'>
      {/* Bộ chọn loại biểu đồ: Bỏ thông tin "Loại thông số hiển thị" theo yêu cầu 3.1 */}
      <div className='flex flex-wrap items-center justify-start gap-3 bg-gray-50 p-3 rounded-lg border border-gray-200'>
        <Radio.Group
          value={selectedChart}
          onChange={(e) => setSelectedChart(e.target.value)}
          buttonStyle='solid'
          size='middle'
        >
          <Radio.Button value='voltage'>
            <span className='flex items-center gap-1.5'>
              <FiZap />
              {`Điện áp (${voltageUnit})`}
            </span>
          </Radio.Button>
          <Radio.Button value='current'>
            <span className='flex items-center gap-1.5'>
              <FiActivity />
              Dòng điện (A)
            </span>
          </Radio.Button>
          <Radio.Button value='power'>
            <span className='flex items-center gap-1.5'>
              <FiCpu />
              Công suất (kW / kVAr)
            </span>
          </Radio.Button>
          <Radio.Button value='pf'>
            <span>Hệ số công suất</span>
          </Radio.Button>
          <Radio.Button value='frequency'>
            <span>Tần số (Hz)</span>
          </Radio.Button>
        </Radio.Group>
      </div>

      {/* Vùng hiển thị biểu đồ */}
      <Card className='bg-white shadow-sm border border-gray-100 rounded-lg p-2'>
        <div className='h-[480px] w-full'>
          <Line data={chartConfig.chartData} options={options} />
        </div>
      </Card>
    </div>
  );
};

export default OperatingParameterChart;
