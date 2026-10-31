import { Button, Input, Modal, Table, TableProps, Tooltip } from 'antd';
import React, { useEffect, useState } from 'react';
import { FiCheck, FiRefreshCw, FiSearch, FiUserCheck } from 'react-icons/fi';

import Constants from '@/core/application/common/constants';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import useGetListAccountHook from '@/infrastructure/repository/account/hooks/useGetAccount.hook';
import { convertDateTime } from '@/infrastructure/utils/helpers';

interface AccountSearchModalProps {
  visible: boolean;
  onClose: () => void;
  onConfirm: (selectedAccounts: any[]) => void;
  alreadySelectedIds: Array<number | string>;
}

export const AccountSearchModal: React.FC<AccountSearchModalProps> = ({
  visible,
  onClose,
  onConfirm,
  alreadySelectedIds = [],
}) => {
  const [getListAccount] = useGetListAccountHook();
  const [loading, setLoading] = useState(false);
  const [accounts, setAccounts] = useState<any[]>([]);

  const [searchUsername, setSearchUsername] = useState('');
  const [searchFullName, setSearchFullName] = useState('');
  const [searchEmail, setSearchEmail] = useState('');

  const debouncedUsername = useDebounce(searchUsername, 300);
  const debouncedFullName = useDebounce(searchFullName, 300);
  const debouncedEmail = useDebounce(searchEmail, 300);

  const [selectedKeys, setSelectedKeys] = useState<React.Key[]>([]);
  const [selectedRowsMap, setSelectedRowsMap] = useState<Record<string, any>>({});

  const [pagination, setPagination] = useState({
    pageSize: 10,
    hasNext: false,
    hasPrevious: false,
    totalCount: 0,
  });

  // Khởi tạo selectedKeys từ alreadySelectedIds khi modal mở
  useEffect(() => {
    if (visible) {
      const keys = alreadySelectedIds.map((id) => Number(id));
      setSelectedKeys(keys);
    }
  }, [visible, alreadySelectedIds]);

  const fetchAccounts = async (direction?: 'next' | 'prev') => {
    const searchByFields: Array<{ searchFieldName: string; searchValue: string }> = [
      {
        searchFieldName: 'roleName',
        searchValue: Constants.Role.Customer.value,
      },
      {
        searchFieldName: 'status',
        searchValue: Constants.AccountStatus.Active,
      },
    ];

    if (debouncedUsername.trim()) {
      searchByFields.push({ searchFieldName: 'userName', searchValue: debouncedUsername.trim() });
    }
    if (debouncedFullName.trim()) {
      searchByFields.push({ searchFieldName: 'fullName', searchValue: debouncedFullName.trim() });
    }
    if (debouncedEmail.trim()) {
      searchByFields.push({ searchFieldName: 'email', searchValue: debouncedEmail.trim() });
    }

    const firstId = accounts[0]?.id;
    const lastId = accounts[accounts.length - 1]?.id;

    const params = new GetListAccountRequest({
      pageSize: pagination.pageSize,
      searchByFields,
      sortByFields: [{ colName: 'id', sortDirection: 'desc' }],
      ...(direction === 'next' && lastId ? { after: lastId } : {}),
      ...(direction === 'prev' && firstId ? { before: firstId } : {}),
    });

    await getListAccount(
      params,
      (res: DataResponse) => {
        const data = res?.data;
        const list = data?.data || [];
        setAccounts(list);

        // Lưu cache các object user đã fetch
        setSelectedRowsMap((prev) => {
          const map = { ...prev };
          list.forEach((item: any) => {
            map[item.id] = item;
          });
          return map;
        });

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
      fetchAccounts();
    }
  }, [visible, debouncedUsername, debouncedFullName, debouncedEmail]);

  const handleReset = () => {
    setSearchUsername('');
    setSearchFullName('');
    setSearchEmail('');
  };

  const handleConfirm = () => {
    const selectedList = selectedKeys.map((key) => {
      const found = selectedRowsMap[key] || accounts.find((a) => a.id === key);
      return (
        found || {
          id: key,
          value: key,
          userName: `User #${key}`,
          fullName: '',
        }
      );
    });
    onConfirm(selectedList);
    onClose();
  };

  const columns: TableProps<any>['columns'] = [
    {
      title: 'Tên đăng nhập',
      dataIndex: 'userName',
      key: 'userName',
      width: 170,
      render: (val) => (
        <span className='font-bold text-blue-700 bg-blue-50 px-2 py-0.5 rounded'>
          {val}
        </span>
      ),
    },
    {
      title: 'Họ và tên',
      dataIndex: 'fullName',
      key: 'fullName',
      render: (val) => (
        <Tooltip title={val}>
          <span className='font-medium text-gray-900'>{val || '---'}</span>
        </Tooltip>
      ),
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      render: (val) => <span className='text-gray-600'>{val || '---'}</span>,
    },
    {
      title: 'Số điện thoại',
      dataIndex: 'phoneNumber',
      key: 'phoneNumber',
      width: 130,
      render: (val) => <span>{val || '---'}</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      align: 'center',
      render: (val) => {
        const isActive = val === Constants.AccountStatus.Active || val === 'ACTIVE';
        return (
          <StatusTagCommon
            status={isActive ? 'Đang hoạt động' : 'Vô hiệu'}
            bgColor={isActive ? '#41A026' : '#B22222'}
            textColor={'#FFFFFF'}
          />
        );
      },
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdTime',
      key: 'createdTime',
      width: 150,
      render: (val) => <span>{val ? convertDateTime(val, 'dd/mm/yyyy') : '---'}</span>,
    },
  ];

  return (
    <Modal
      title={
        <div className='flex items-center gap-2 text-16-24 font-bold text-gray-800 pb-2 border-b border-gray-100'>
          <FiUserCheck className='text-blue-600' size={18} />
          <span>Tra cứu và liên kết tài khoản người dùng (Vai trò Khách hàng)</span>
        </div>
      }
      open={visible}
      onCancel={onClose}
      width={950}
      footer={
        <div className='flex justify-between items-center'>
          <div className='text-13-18 text-gray-600'>
            Đã chọn:{' '}
            <strong className='text-blue-700 font-bold'>
              {selectedKeys.length} tài khoản
            </strong>
          </div>
          <div className='flex gap-2'>
            <Button onClick={onClose}>Hủy</Button>
            <Button
              type='primary'
              onClick={handleConfirm}
              className='bg-blue-600'
            >
              Xác nhận liên kết ({selectedKeys.length})
            </Button>
          </div>
        </div>
      }
    >
      <div className='py-2'>
        {/* Bộ lọc */}
        <div className='grid grid-cols-1 md:grid-cols-4 gap-2 mb-3 bg-gray-50 p-2.5 rounded-lg border border-gray-200'>
          <Input
            placeholder='Tên đăng nhập...'
            value={searchUsername}
            onChange={(e) => setSearchUsername(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <Input
            placeholder='Họ và tên...'
            value={searchFullName}
            onChange={(e) => setSearchFullName(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <Input
            placeholder='Email...'
            value={searchEmail}
            onChange={(e) => setSearchEmail(e.target.value)}
            allowClear
            prefix={<FiSearch className='text-gray-400' />}
          />
          <Button
            icon={<FiRefreshCw size={13} />}
            onClick={handleReset}
            className='flex items-center justify-center gap-1'
          >
            Làm mới bộ lọc
          </Button>
        </div>

        {/* Bảng danh sách tài khoản */}
        <Table
          columns={columns}
          dataSource={accounts}
          rowKey='id'
          loading={loading}
          size='middle'
          pagination={false}
          scroll={{ y: 360 }}
          rowSelection={{
            selectedRowKeys: selectedKeys,
            onChange: (keys, rows) => {
              setSelectedKeys(keys);
              setSelectedRowsMap((prev) => {
                const map = { ...prev };
                rows.forEach((r) => {
                  map[r.id] = r;
                });
                return map;
              });
            },
          }}
          rowClassName={(record) =>
            selectedKeys.includes(record.id)
              ? 'bg-blue-50/70 cursor-pointer font-medium'
              : 'cursor-pointer hover:bg-gray-50'
          }
          onRow={(record) => ({
            onClick: () => {
              const isSelected = selectedKeys.includes(record.id);
              if (isSelected) {
                setSelectedKeys((prev) => prev.filter((k) => k !== record.id));
              } else {
                setSelectedKeys((prev) => [...prev, record.id]);
                setSelectedRowsMap((prev) => ({ ...prev, [record.id]: record }));
              }
            },
          })}
        />

        {/* Phân trang */}
        <div className='flex justify-between items-center mt-3 pt-2 border-t border-gray-100 text-12-16 text-gray-500'>
          <span>Tổng số: {pagination.totalCount} tài khoản khách hàng</span>
          <div className='flex gap-2'>
            <Button
              size='small'
              disabled={!pagination.hasPrevious || loading}
              onClick={() => fetchAccounts('prev')}
            >
              Trang trước
            </Button>
            <Button
              size='small'
              disabled={!pagination.hasNext || loading}
              onClick={() => fetchAccounts('next')}
            >
              Trang sau
            </Button>
          </div>
        </div>
      </div>
    </Modal>
  );
};

export default AccountSearchModal;
