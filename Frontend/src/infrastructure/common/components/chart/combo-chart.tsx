import { Card, Table } from 'antd';
import { ChartData, ChartOptions } from 'chart.js';
import React, { useEffect, useState } from 'react';
import { Chart } from 'react-chartjs-2';

import { splitIntoChunks } from '@/infrastructure/utils/helpers.ts';

type ComboChartProps = {
  data: ChartData<'bar' | 'line'>;
  options: ChartOptions<'bar' | 'line'>;
  datasetIdKey?: string;
  redraw?: boolean;
};

const ComboChartWithTable: React.FC<ComboChartProps> = ({
  data,
  options,
  datasetIdKey = 'label',
  redraw = false,
}) => {
  const [chartKey, setChartKey] = useState(0); // Unique key to force remount

  // Trigger a remount when data or options change
  useEffect(() => {
    setChartKey((prevKey) => prevKey + 1);
  }, [data, options]);

  const numberFormatter = new Intl.NumberFormat('vi-VN', {
    style: 'decimal',
    maximumFractionDigits: 0,
  });

  const getColumnData = (columnChild: any) => {
    return [
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
  };

  return (
    <Card className='bg-white'>
      <div className='relative mb-3 w-full'>
        <Chart
          key={chartKey} // Use unique key to force remount
          type='bar'
          data={data}
          options={options}
          datasetIdKey={datasetIdKey}
          className='h-[500px] w-full'
          redraw={redraw}
        />
      </div>

      <div className='w-full'>
        {data.labels &&
          splitIntoChunks([...data.labels], 11).map((item, chunkIndex) => {
            const startIndex = chunkIndex * 11;
            // Create filtered table data for this chunk
            const chunkTableData = data.datasets
              .map((dataset, datasetIndex) => {
                const rowData: any = {
                  key: datasetIndex,
                  label: dataset.label,
                  color: dataset.borderColor || dataset.backgroundColor || 'black',
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
                pagination={false}
                className='radius-none'
                size='small'
              />
            );
          })}
      </div>
    </Card>
  );
};

export default ComboChartWithTable;
