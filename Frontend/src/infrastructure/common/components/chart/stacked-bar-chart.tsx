import { Card, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ChartData,
  ChartOptions,
} from 'chart.js';
import { useEffect, useState } from 'react';
import { Bar } from 'react-chartjs-2';

import { customDashedLinePlugin, splitIntoChunks } from '@/infrastructure/utils/helpers.ts';

// Register the components for Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  customDashedLinePlugin
);

type StackedBarLineChartProps = {
  data: ChartData<'bar' | 'line'>;
  options: ChartOptions<'bar' | 'line'>;
  datasetIdKey?: string;
  redraw?: boolean;
};

const numberFormatter = new Intl.NumberFormat('vi-VN', {
  style: 'decimal',
  maximumFractionDigits: 0,
});

const StackedBarLineChart = ({
  data,
  options,
  datasetIdKey = 'label',
  redraw = false,
}: StackedBarLineChartProps) => {
  // Define columns for Ant Design Table

  const [chartKey, setChartKey] = useState(0); // Unique key to force remount

  // Trigger a remount when data or options change
  useEffect(() => {
    setChartKey((prevKey) => prevKey + 1);
  }, [data, options]);

  const getColumnData = (columnChild: any) => {
    const columns: ColumnsType<any> = [
      {
        title: '',
        dataIndex: 'label',
        key: 'label',
        fixed: 'left',
        align: 'left',
        render: (text: string, record: any) => (
          <span style={{ color: record.color }} className='text-12-16'>
            {text}
          </span>
        ),
        width: '10%',
      },
      ...(columnChild.map((label: any, index: number) => ({
        title: <div className='text-12-16 text-center'>{label}</div>,
        dataIndex: `value_${index}`,
        key: `value_${index}`,
        render: (value: any) => (
          <div className='text-12-16 text-right'>{label && numberFormatter.format(value)}</div>
        ),
        width: '8%',
      })) || []),
    ];

    return columns;
  };

  return (
    <Card className='bg-white'>
      <div className={'flex justify-center relative'}>
        <Bar
          key={chartKey}
          data={data as any}
          options={options as any}
          className={'h-[500px] w-full'}
          datasetIdKey={datasetIdKey}
          redraw={redraw}
        />
      </div>
      <div className='mt-3 w-full'>
        {/* <Table
          columns={columns}
          dataSource={tableData}
          bordered
          pagination={false}
          className='mt-5'
          size={'small'}
          scroll={{ x: 'max-content' }}
        /> */}
        {data.labels &&
          splitIntoChunks([...data.labels], 11).map((item, chunkIndex) => {
            const startIndex = chunkIndex * 11;
            // Create filtered table data for this chunk
            const chunkTableData = data.datasets
              .map((dataset, datasetIndex) => {
                const rowData: any = {
                  key: datasetIndex,
                  label: dataset.label,
                  color:
                    (dataset.backgroundColor as string) ||
                    (dataset.borderColor as string) ||
                    'black',
                };
                // Map chunk data to local indices (0, 1, 2, ...)
                item.forEach((_, labelIndex) => {
                  const actualIndex = startIndex + labelIndex;
                  rowData[`value_${labelIndex}`] = dataset.data[actualIndex] ?? 0;
                });
                return rowData;
              })
              .reverse();

            return (
              <Table
                columns={getColumnData(item)}
                key={item[0]}
                dataSource={chunkTableData}
                bordered
                pagination={false}
                className='radius-none'
                size={'small'}
                // scroll={{ x: 'max-content' }}
              />
            );
          })}
      </div>
    </Card>
  );
};

export default StackedBarLineChart;
