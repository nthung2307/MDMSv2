import React from 'react';
import { Card, Select, Tooltip } from 'antd';
import {
  CheckCircleOutlined,
  DisconnectOutlined,
  WarningOutlined,
  DashboardOutlined,
  ReloadOutlined,
  AlertOutlined,
} from '@ant-design/icons';

import { MeasurementPointMonitoringHealthSummaryDto } from '@/core/application/dto/measurement-point-monitoring/responses/MeasurementPointMonitoringHealthSummaryDto';

interface MonitoringHealthWidgetProps {
  summary: MeasurementPointMonitoringHealthSummaryDto | null;
  loading?: boolean;
  activeFilter: string;
  onFilterChange: (mode: string) => void;
  timeoutMinutes: number;
  onTimeoutChange: (timeout: number) => void;
  onRefresh?: () => void;
}

export const MonitoringHealthWidget: React.FC<MonitoringHealthWidgetProps> = ({
  summary,
  loading = false,
  activeFilter,
  onFilterChange,
  timeoutMinutes,
  onTimeoutChange,
  onRefresh,
}) => {
  const cards = [
    {
      key: 'all',
      title: 'Tổng số điểm đo',
      count: summary?.totalPoints ?? 0,
      icon: <DashboardOutlined className='text-2xl text-blue-500' />,
      bgColor: 'bg-blue-50/60 hover:bg-blue-50',
      activeBorder: 'border-blue-500 ring-2 ring-blue-200',
    },
    {
      key: 'good',
      title: 'Kết nối tốt',
      count: summary?.goodPoints ?? 0,
      icon: <CheckCircleOutlined className='text-2xl text-emerald-500' />,
      bgColor: 'bg-emerald-50/60 hover:bg-emerald-50',
      activeBorder: 'border-emerald-500 ring-2 ring-emerald-200',
    },
    {
      key: 'modbus_lost',
      title: 'Mất kết nối đo đếm',
      count: summary?.modbusLostPoints ?? 0,
      icon: <DisconnectOutlined className='text-2xl text-rose-500' />,
      bgColor: 'bg-rose-50/60 hover:bg-rose-50',
      activeBorder: 'border-rose-500 ring-2 ring-rose-200',
    },
    {
      key: 'rest_stale',
      title: 'Mất kết nối IOT',
      count: summary?.restStalePoints ?? 0,
      icon: <WarningOutlined className='text-2xl text-amber-500' />,
      bgColor: 'bg-amber-50/60 hover:bg-amber-50',
      activeBorder: 'border-amber-500 ring-2 ring-amber-200',
    },
    {
      key: 'bad_quality',
      title: 'Điểm đo có số liệu nghi ngờ',
      count: summary?.badQualityPoints ?? 0,
      icon: <AlertOutlined className='text-2xl text-purple-500' />,
      bgColor: 'bg-purple-50/60 hover:bg-purple-50',
      activeBorder: 'border-purple-500 ring-2 ring-purple-200',
    },
  ];

  return (
    <div className='mb-4'>
      <div className='flex items-center justify-end gap-3 mb-2 px-1 text-xs'>
        <Tooltip title='Khoảng thời gian không có dữ liệu IoT trước khi hệ thống đánh dấu điểm đo bị ngừng/đóng băng dữ liệu.'>
          <span className='text-gray-600 font-medium cursor-help'>
            Ngưỡng gián đoạn IoT:
          </span>
        </Tooltip>
        <Select
          size='small'
          value={timeoutMinutes}
          onChange={onTimeoutChange}
          style={{ width: 110 }}
          options={[
            { value: 2, label: '2 phút' },
            { value: 3, label: '3 phút' },
            { value: 5, label: '5 phút (chuẩn)' },
            { value: 10, label: '10 phút' },
            { value: 15, label: '15 phút' },
            { value: 30, label: '30 phút' },
          ]}
        />
        {onRefresh && (
          <button
            onClick={onRefresh}
            className='p-1 text-gray-500 hover:text-blue-600 rounded transition'
            title='Làm mới trạng thái kết nối'
            type='button'
          >
            <ReloadOutlined spin={loading} />
          </button>
        )}
      </div>

      <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3'>
        {cards.map((c) => {
          const isSelected = activeFilter === c.key;
          return (
            <Card
              key={c.key}
              hoverable
              onClick={() => onFilterChange(c.key)}
              size='small'
              className={`cursor-pointer transition-all duration-200 border ${
                isSelected
                  ? `${c.activeBorder} shadow-md`
                  : 'border-gray-200 hover:border-gray-300'
              } ${c.bgColor}`}
              bodyStyle={{ padding: '12px 14px' }}
            >
              <div className='mb-1'>
                <span className='text-xs font-semibold text-gray-600 truncate block' title={c.title}>
                  {c.title}
                </span>
              </div>
              <div className='flex items-center justify-between mt-2'>
                <div className='text-2xl font-bold tracking-tight text-gray-800'>
                  {c.count}
                </div>
                <div className='p-2 rounded-full bg-white/80 shadow-sm'>{c.icon}</div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
};

export default MonitoringHealthWidget;
