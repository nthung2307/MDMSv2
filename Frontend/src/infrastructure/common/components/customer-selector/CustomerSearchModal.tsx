import { Button, Input, Modal, Select, Table, TableProps, Tooltip } from 'antd';
import React, { useEffect, useState } from 'react';
import { FiCheck, FiRefreshCw, FiSearch, FiUser } from 'react-icons/fi';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import useGetListCustomerHook from '@/infrastructure/repository/customer/hooks/useGetCustomerList.hook';

interface CustomerSearchModalProps {
  visible: boolean;
  onClose: () => void;
  onSelect: (customer: any) => void;
  selectedCustomerId?: number | string;
}

export const CustomerSearchModal: React.FC<CustomerSearchModalProps> = ({
  visible,
  onClose,
  onSelect,
  selectedCustomerId,
}) => {
  const [getListCustomer] = useGetListCustomerHook();
  const [loading, setLoading] = useState(false);
  const [customers, setCustomers] = useState<any[]>([]);
  const [selectedRow, setSelectedRow] = useState<any>(null);

  const [searchCode, setSearchCode] = useState('');
  const [searchName, setSearchName] = useState('');
  const [searchPhone, setSearchPhone] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('ACTIVE');

  const debouncedCode = useDebounce(searchCode, 300);
  const debouncedName = useDebounce(searchName, 300);
  const debouncedPhone = useDebounce(searchPhone, 300);

  const [pagination, setPagination] = useState({
    pageSize: 10,
    hasNext: false,
    hasPrevious: false,
    totalCount: 0,
  });

  const fetchCustomers = async (direction?: 'next' | 'prev') => {
    const searchByFields: Array<{ searchFieldName: string; searchValue: string }> = [];

    if (debouncedCode.trim()) {
      searchByFields.push({ searchFieldName: 'customerCode', searchValue: debouncedCode.trim() });
    }
    if (debouncedName.trim()) {
      searchByFields.push({ searchFieldName: 'fullName', searchValue: debouncedName.trim() });
    }
    if (debouncedPhone.trim()) {
      searchByFields.push({ searchFieldName: 'phoneNumber', searchValue: debouncedPhone.trim() });
    }
    if (statusFilter && statusFilter !== 'ALL') {
      searchByFields.push({ searchFieldName: 'status', searchValue: statusFilter });
    }

    const firstId = customers[0]?.id;
    const lastId = customers[customers.length - 1]?.id;

    const params = new GetListCustomerRequest({
      pageSize: pagination.pageSize,
      searchByFields,
      sortByFields: [{ colName: 'id', sortDirection: 'desc' }],
      ...(direction === 'next' && lastId ? { after: lastId } : {}),
      ...(direction === 'prev' && firstId ? { before: firstId } : {}),
    });

    await getListCustomer(
      params as any,
      (res: DataResponse) => {
        const data = res?.data;
        const list = data?.data || [];
        setCustomers(list);
        setPagination((prev) => ({
          ...prev,
          hasNext: !!data?.hasNext,
          hasPrevious: !!data?.hasPrevious,
          totalCount: data?.totalCount || list.length,
        }));
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    if (visible) {
      fetchCustomers();
    }
  }, [visible, debouncedCode, debouncedName, debouncedPhone, statusFilter]);

  const handleReset = () => {
    setSearchCode('');
    setSearchName('');
    setSearchPhone('');
    setStatusFilter('ACTIVE');
  };

  const handleConfirmSelect = () => {
    if (selectedRow) {
      onSelect(selectedRow);
      onClose();
    }
  };

  const columns: TableProps<any>['columns'] = [
    {
      title: 'Mã KH',
      dataIndex: 'customerCode',
      key: 'customerCode',
      width: 140,
      render: (val, record) => (
        <span className='font-semibold text-blue-700 bg-blue-50 px-1.5 py-0.5 rounded'>
          {val || record.code || 'N/A'}
        </span>
      ),
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'fullName',
      key: 'fullName',
      render: (val) => (
        <Tooltip title={val}>
          <span className='font-medium text-gray-900'>{val}</span>
        </Tooltip>
      ),
    },
    {
      title: 'Số điện thoại',
      dataIndex: 'phoneNumber',
      key: 'phoneNumber',
      width: 130,
      render: (val) => <span>{val || '---'}</span>,
    },
    {
      title: 'Địa chỉ',
      dataIndex: 'address',
      key: 'address',
      ellipsis: true,
      render: (val) => (
        <Tooltip title={val}>
          <span className='text-gray-600 truncate'>{val || '---'}</span>
        </Tooltip>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      align: 'center',
      render: (val) => {
        const s = String(val || '').trim().toUpperCase();
        const isActive = s === 'ACTIVE' || s === '1' || s === 'TRUE';
        return (
          <StatusTagCommon
            status={isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
            bgColor={isActive ? '#41A026' : '#B22222'}
            textColor={'#FFFFFF'}
          />
        );
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 90,
      align: 'center',
      render: (_, record) => (
        <Button
          type={selectedRow?.id === record.id ? 'primary' : 'default'}
          size='small'
          onClick={(e) => {
            e.stopPropagation();
            onSelect(record);
            onClose();
          }}
          className='text-12-16'
        >
          Chọn
        </Button>
      ),
    },
  ];

  return (
    <Modal
      title={
        <div className='flex items-center gap-2 text-16-24 font-bold text-gray-800 pb-2 border-b border-gray-100'>
          <FiUser className='text-blue-600' size={18} />
          <span>Tra cứu và chọn khách hàng liên kết</span>
        </div>
      }
      open={visible}
      onCancel={onClose}
      width={920}
      footer={
        <div className='flex justify-between items-center'>
          <div className='text-12-16 text-gray-500'>
            {selectedRow ? (
              <span>
                Đang chọn:{' '}
                <strong className='text-blue-700'>
                  {selectedRow.fullName} ({selectedRow.customerCode || selectedRow.code})
                </strong>
              </span>
            ) : (
              <span>(Click đúp vào hàng để chọn nhanh)</span>
            )}
          </div>
          <div className='flex gap-2'>
            <Button onClick={onClose}>Đóng</Button>
            <Button
              type='primary'
              disabled={!selectedRow}
              onClick={handleConfirmSelect}
              className='bg-blue-600'
            >
              Xác nhận chọn
            </Button>
          </div>
        </div>
      }
    >
      <div className='py-2'>
        {/* Bộ lọc tìm kiếm nhanh */}
        <div className='grid grid-cols-1 md:grid-cols-4 gap-2 mb-3 bg-gray-50 p-2.5 rounded-lg border border-gray-200'>
          <Input
            placeholder='Mã khách hàng...'
            value={searchCode}
            onChange={(e) => setSearchCode(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <Input
            placeholder='Tên khách hàng...'
            value={searchName}
            onChange={(e) => setSearchName(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <Input
            placeholder='Số điện thoại...'
            value={searchPhone}
            onChange={(e) => setSearchPhone(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <div className='flex gap-1.5'>
            <Select
              className='w-full'
              value={statusFilter}
              onChange={setStatusFilter}
              options={[
                { value: 'ALL', label: 'Tất cả trạng thái' },
                { value: 'ACTIVE', label: 'Đang hoạt động' },
                { value: 'DEACTIVATE', label: 'Ngừng hoạt động' },
              ]}
            />
            <Button
              icon={<FiRefreshCw size={13} />}
              onClick={handleReset}
              title='Làm mới bộ lọc'
            />
          </div>
        </div>

        {/* Bảng danh sách khách hàng */}
        <Table
          columns={columns}
          dataSource={customers}
          rowKey='id'
          loading={loading}
          size='middle'
          pagination={false}
          scroll={{ y: 360 }}
          rowClassName={(record) =>
            record.id === selectedRow?.id || record.id === Number(selectedCustomerId)
              ? 'bg-blue-50 font-medium cursor-pointer'
              : 'cursor-pointer hover:bg-gray-50'
          }
          onRow={(record) => ({
            onClick: () => setSelectedRow(record),
            onDoubleClick: () => {
              onSelect(record);
              onClose();
            },
          })}
        />

        {/* Phân trang đơn giản */}
        <div className='flex justify-between items-center mt-3 pt-2 border-t border-gray-100 text-12-16 text-gray-500'>
          <span>Tổng số: {pagination.totalCount} khách hàng</span>
          <div className='flex gap-2'>
            <Button
              size='small'
              disabled={!pagination.hasPrevious || loading}
              onClick={() => fetchCustomers('prev')}
            >
              Trang trước
            </Button>
            <Button
              size='small'
              disabled={!pagination.hasNext || loading}
              onClick={() => fetchCustomers('next')}
            >
              Trang sau
            </Button>
          </div>
        </div>
      </div>
    </Modal>
  );
};

export default CustomerSearchModal;
