import { Pagination } from 'antd';
import { SetStateAction } from 'jotai';
import { Dispatch, useEffect, useState } from 'react';

interface Props {
  pageSize?: number;
  setSize: Dispatch<SetStateAction<number>> | ((size: number) => void);
  hasNext?: boolean;
  hasPrevious?: boolean;
  onClickPrevious: (callback?: () => void) => void;
  onClickNext: (callback?: () => void) => void;
  total?: number;
  currentCount?: number;
  className?: string;
  pageSizeOptions?: string[];
  defaultPageSize?: number;
}

const PaginationCommon = ({
  pageSize = 10,
  hasNext = false,
  hasPrevious = false,
  onClickNext,
  onClickPrevious,
  setSize,
  total,
  currentCount = 0,
  className = '',
  pageSizeOptions = ['10', '20', '50', '100'],
}: Props) => {
  const [currentPage, setCurrentPage] = useState<number>(1);

  // Reset về trang 1 khi hasPrevious trở về false (ví dụ: khi tìm kiếm mới hoặc reset bộ lọc)
  useEffect(() => {
    if (!hasPrevious) {
      setCurrentPage(1);
    }
  }, [hasPrevious]);

  // Tính total cho Antd Pagination:
  // Nếu có total thực tế truyền vào thì dùng total đó.
  // Nếu dùng cursor-based (chỉ có hasNext, hasPrevious):
  // - Khi hasNext = true: hiển thị ít nhất trang tiếp theo để người dùng bấm được sang trang kế tiếp
  // - Khi hasNext = false: tổng trang dừng lại ở currentPage
  const computedTotal =
    total !== undefined && total !== null
      ? total
      : hasNext
        ? (currentPage + 1) * pageSize
        : currentPage * pageSize;

  const displayTotal = total !== undefined && total !== null ? total : currentCount;

  return (
    <div className={`flex justify-end items-center py-3 select-none ${className}`}>
      <Pagination
        size='small'
        current={currentPage}
        pageSize={pageSize}
        total={computedTotal}
        showSizeChanger
        pageSizeOptions={pageSizeOptions}
        showTotal={() => `Tổng số: ${displayTotal} bản ghi`}
        onChange={(newPage, newPageSize) => {
          if (newPageSize !== pageSize) {
            setCurrentPage(1);
            setSize(newPageSize);
          } else if (newPage > currentPage) {
            setCurrentPage(newPage);
            onClickNext();
          } else if (newPage < currentPage) {
            setCurrentPage(newPage);
            onClickPrevious();
          }
        }}
      />
    </div>
  );
};

export default PaginationCommon;
