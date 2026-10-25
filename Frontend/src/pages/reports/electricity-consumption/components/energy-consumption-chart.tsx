import { Card, Empty, Radio } from 'antd';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  ChartData,
  ChartOptions,
  Legend,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import React, { useMemo, useState } from 'react';
import { Chart } from 'react-chartjs-2';
import { FiArrowDownLeft, FiArrowUpRight } from 'react-icons/fi';

import {
  EnergyConsumptionChartData,
  EnergyConsumptionItem,
} from '@/core/application/dto/reports/requests/GetEnergyConsumptionReportRequest';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  BarController,
  PointElement,
  LineElement,
  LineController,
  Title,
  Tooltip,
  Legend
);

interface EnergyConsumptionChartProps {
  chartData: EnergyConsumptionChartData;
  items?: EnergyConsumptionItem[];
  title?: string;
  compareWithPrevious?: boolean;
  containerRef?: React.Ref<HTMLDivElement>;
}

export const EnergyConsumptionChart: React.FC<EnergyConsumptionChartProps> = ({
  chartData,
  items = [],
  title = '',
  compareWithPrevious = false,
  containerRef,
}) => {
  const [channel, setChannel] = useState<'GIAO' | 'NHAN'>('GIAO');

  const hasItems = items && items.length > 0;

  const data: ChartData<'bar' | 'line'> = useMemo(() => {
    const labels = hasItems
      ? items.map((it) => it.timeLabel || '')
      : chartData?.labels || [];

    // Lấy dữ liệu 3 biểu theo chiều giao hoặc nhận
    const b1Data = hasItems
      ? items.map((it) =>
          channel === 'GIAO'
            ? it.sanLuongTacDungBTKenhGiao ?? 0
            : it.sanLuongTacDungBTKenhNhan ?? 0
        )
      : [];
    const b2Data = hasItems
      ? items.map((it) =>
          channel === 'GIAO'
            ? it.sanLuongTacDungCDKenhGiao ?? 0
            : it.sanLuongTacDungCDKenhNhan ?? 0
        )
      : [];
    const b3Data = hasItems
      ? items.map((it) =>
          channel === 'GIAO'
            ? it.sanLuongTacDungTDKenhGiao ?? 0
            : it.sanLuongTacDungTDKenhNhan ?? 0
        )
      : [];

    const datasets: any[] = [
      {
        type: 'bar' as const,
        label: 'Biểu 1 (BT)',
        data: b1Data,
        backgroundColor: 'rgba(59, 130, 246, 0.85)', // Xanh dương
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar' as const,
        label: 'Biểu 2 (CD)',
        data: b2Data,
        backgroundColor: 'rgba(239, 68, 68, 0.85)', // Đỏ cam
        borderColor: 'rgb(239, 68, 68)',
        borderWidth: 1,
        stack: 'energyStack',
      },
      {
        type: 'bar' as const,
        label: 'Biểu 3 (TD)',
        data: b3Data,
        backgroundColor: 'rgba(16, 185, 129, 0.85)', // Xanh lá
        borderColor: 'rgb(16, 185, 129)',
        borderWidth: 1,
        stack: 'energyStack',
      },
    ];

    if (compareWithPrevious && hasItems) {
      const prevData = items.map((it) =>
        channel === 'GIAO'
          ? Math.round(it.previousSanLuongTacDungTongKenhGiao ?? 0)
          : Math.round(it.previousSanLuongTacDungTongKenhNhan ?? 0)
      );
      datasets.push({
        type: 'line' as const,
        label: `Cùng kỳ năm trước (${channel === 'GIAO' ? 'Giao' : 'Nhận'})`,
        data: prevData,
        borderColor: 'rgb(245, 158, 11)',
        backgroundColor: 'rgba(245, 158, 11, 0.2)',
        borderWidth: 2,
        borderDash: [5, 5],
        pointRadius: 3,
        pointHoverRadius: 5,
        fill: false,
      });
    }

    return {
      labels,
      datasets,
    };
  }, [chartData, items, channel, compareWithPrevious, hasItems]);

  const options: ChartOptions<'bar' | 'line'> = useMemo(
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
          text: `${title} - ${channel === 'GIAO' ? 'Kênh giao' : 'Kênh nhận'}`,
          font: {
            size: 15,
            weight: 'bold',
          },
          padding: {
            top: 10,
            bottom: 15,
          },
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.dataset.label || '';
              const val = context.raw as number;
              return `${label}: ${Math.round(Number(val)).toLocaleString('vi-VN')} kWh`;
            },
            footer: (tooltipItems) => {
              let sum = 0;
              tooltipItems.forEach((item) => {
                if (item.dataset.type !== 'line') {
                  sum += (item.raw as number) || 0;
                }
              });
              const lines = [
                `Tổng cộng: ${Math.round(sum).toLocaleString('vi-VN')} kWh`,
              ];

              if (compareWithPrevious && hasItems) {
                const idx = tooltipItems[0]?.dataIndex;
                const it = items[idx];
                if (it) {
                  const prevTotal =
                    channel === 'GIAO'
                      ? it.previousSanLuongTacDungTongKenhGiao
                      : it.previousSanLuongTacDungTongKenhNhan;
                  if (prevTotal !== undefined && prevTotal !== null) {
                    lines.push(
                      `Cùng kỳ năm trước: ${Math.round(Number(prevTotal)).toLocaleString('vi-VN')} kWh`
                    );
                  }
                }
              }

              return lines.join('\n');
            },
          },
        },
      },
      scales: {
        x: {
          stacked: true,
          grid: {
            display: false,
          },
          ticks: {
            maxRotation: 45,
            minRotation: 0,
            autoSkip: true,
            maxTicksLimit: 48,
            font: {
              size: 10,
            },
          },
        },
        y: {
          stacked: true,
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
    [title, channel, compareWithPrevious, hasItems, items]
  );

  if (!hasItems) {
    return (
      <Card className='mt-2 bg-white flex justify-center items-center py-16 shadow-sm border border-gray-100 rounded-lg'>
        <Empty
          description={
            <span className='text-gray-500'>
              Chưa có dữ liệu sản lượng điện để hiển thị biểu đồ.
              <br />
              Vui lòng chọn khách hàng, điểm đo và bấm <b>&quot;Truy vấn&quot;</b>.
            </span>
          }
        />
      </Card>
    );
  }

  return (
    <div className='flex flex-col gap-4' ref={containerRef}>
      {/* Bộ chọn chiều giao / chiều nhận tương tự báo cáo Thông số vận hành */}
      <div className='flex flex-wrap items-center justify-start gap-3 bg-gray-50 p-3 rounded-lg border border-gray-200'>
        <Radio.Group
          value={channel}
          onChange={(e) => setChannel(e.target.value)}
          buttonStyle='solid'
          size='middle'
        >
          <Radio.Button value='GIAO'>
            <span className='inline-flex items-center gap-1.5 leading-none font-medium px-2 py-1'>
              <FiArrowUpRight />
              Kênh giao (kWh)
            </span>
          </Radio.Button>
          <Radio.Button value='NHAN'>
            <span className='inline-flex items-center gap-1.5 leading-none font-medium px-2 py-1'>
              <FiArrowDownLeft />
              Kênh nhận (kWh)
            </span>
          </Radio.Button>
        </Radio.Group>
      </div>

      {/* Vùng hiển thị biểu đồ */}
      <Card className='bg-white shadow-sm border border-gray-100 rounded-lg p-2'>
        <div className='h-[480px] w-full'>
          <Chart type='bar' data={data as any} options={options as any} />
        </div>
      </Card>
    </div>
  );
};

export default EnergyConsumptionChart;
