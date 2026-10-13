import { TableProps } from 'antd';
import { useEffect, useState } from 'react';

import { IGetListFinalizedOutputElectricityResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import { numberFormatter } from '@/infrastructure/utils/helpers.ts';

interface TableOutputMeasurementProps {
  dataList: IGetListFinalizedOutputElectricityResponse;
}

const TableOutputMeasurement = ({ dataList }: TableOutputMeasurementProps) => {
  const [tableData, setTableData] = useState<any>([]);

  const columns: TableProps['columns'] = [
    {
      title: 'Mã KH',
      dataIndex: 'customerCode',
      width: '10%',
    },
    {
      title: 'Tên KH',
      dataIndex: 'customerName',
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
    },
    {
      title: 'Mô tả điểm đo',
      dataIndex: 'measurementPointDescription',
    },
    {
      title: 'Giờ BT',
      dataIndex: 'bt',
      width: '10%',
    },
    {
      title: 'Giờ TĐ',
      dataIndex: 'td',
      width: '10%',
    },
    {
      title: 'Giờ CĐ',
      dataIndex: 'cd',
      width: '10%',
    },
    {
      title: 'Tổng sản lượng',
      dataIndex: 'total',
      width: '10%',
    },
  ];

  useEffect(() => {
    if (dataList) {
      const items = dataList?.map((it) => {
        return {
          customerCode: it.customerCode,
          customerName: it.customerName,
          measurementPointCode: it.measurementPointCode,
          measurementPointDescription: it.measurementPointDescription,
          bt: numberFormatter.format(it.bt),
          td: numberFormatter.format(it.td),
          cd: numberFormatter.format(it.cd),
          total: numberFormatter.format(it.sg),
        };
      });
      setTableData(items);
    }
  }, [dataList]);

  return (
    <>
      <TableCommon columns={columns} dataSource={tableData} pagination={false} />
    </>
  );
};

export default TableOutputMeasurement;
