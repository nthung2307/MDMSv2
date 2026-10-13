import { Table, TableProps } from 'antd';
import { Empty } from 'antd/lib';

const TablePDFCommon: React.FC<TableProps<any>> = (props) => {
  return (
    <Table
      locale={{
        emptyText: <Empty description='Không có dữ liệu' />,
      }}
      {...props}
    />
  );
};

export default TablePDFCommon;
