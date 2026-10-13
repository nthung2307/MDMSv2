import { Table, TableProps } from 'antd';
import { Empty } from 'antd/lib';
import React from 'react';
import { useRecoilValue } from 'recoil';
import styled from 'styled-components';

import { ErrorDataState } from '@/core/application/common/atoms/global/errorTable';

const StyledTable = styled(Table)`
  // .ant-table-cell-fix-left,
  // .ant-table-cell-fix-left-last {
  //   background-color: #ffffff !important; /* Body of fixed columns has a white background */
  // }

  /* Ensure header of fixed columns retains the default style */
  .ant-table-thead > tr > th.ant-table-cell-fix-left,
  .ant-table-thead > tr > th.ant-table-cell-fix-left-last,
  .ant-table-thead > tr > th.ant-table-cell-fix-right,
  .ant-table-thead > tr > th.ant-table-cell-fix-right-first {
    background-color: #d3dae5 !important; /* Match default header background color */
  }

  .ant-table-thead > tr > th {
    border-bottom: 1px solid #a6a6a7db !important;
  }
  .ant-table-thead,
  .ant-table-header {
    position: relative;
    z-index: 99999;
    box-shadow: 1px 4px 4px #edecec;
  }

  /* Default border and alignment for table */
  .ant-table-thead > tr > th,
  .ant-table-tbody > tr > td {
    border-bottom: 1px solid #d3dae5;
  }

  .ant-table-tbody > tr > td {
    border-bottom: 1px solid #a6a6a7db !important;
  }

  .ant-table-tbody > tr {
    // height: 60px;
  }

  .ant-table-tbody > tr:last > td {
    border-bottom: 0 !important;
  }

  .ant-table-tbody > tr > .ant-table-cell {
    padding: 4px 8px !important;
  }

  .ant-table-container {
    border: 1px solid #a6a6a7db !important;
  }

  .ant-table-body {
    background-color: #ffffff;
    height: 66vh;
  }

  .ant-table-tbody {
    background-color: #ffffff;
  }

  .ant-table-placeholder {
    background: #fff !important;
    height: 64vh;
  }
`;

const TableCommon: React.FC<TableProps<any>> = (props) => {
  const errorData = useRecoilValue(ErrorDataState);
  return (
    <StyledTable
      locale={{
        emptyText: <Empty description={errorData ? 'Lỗi hệ thống' : 'Không có dữ liệu'} />,
      }}
      size={'small'}
      scroll={props.scroll ? props.scroll : { y: 55 * 10 }}
      {...props}
    />
  );
};

export default TableCommon;
