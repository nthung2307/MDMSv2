import { Button, Col, Row, Select, Space, Spin, Tooltip } from 'antd';
import React, { useEffect, useState } from 'react';
import { FiSearch } from 'react-icons/fi';
import styled from 'styled-components';

import Constants from '@/core/application/common/constants';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { AccountSearchModal } from '@/infrastructure/common/components/account-selector/AccountSearchModal';
import { LinkedAccountsList } from '@/infrastructure/common/components/account-selector/LinkedAccountsList';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import useGetListAccountHook from '@/infrastructure/repository/account/hooks/useGetAccount.hook';

const AccountSelectStyled = styled(Select)`
  &.ant-select .ant-select-selector {
    min-height: 38px !important;
    border-color: #a6a6a7db !important;
    font-size: 14px !important;
  }
  &.ant-select-focused .ant-select-selector {
    border-color: #a6a6a7db !important;
    box-shadow: none !important;
  }
`;

interface UserAccountSelectorProps {
  label?: React.ReactNode;
  attribute?: string;
  dataAttribute?: any;
  setData: (data: any) => void;
  isRequired?: boolean;
  disabled?: boolean;
  initialAccounts?: any[];
  placeholder?: string;
}

export const UserAccountSelector: React.FC<UserAccountSelectorProps> = ({
  label = 'Liên kết tài khoản',
  attribute = 'userIds',
  dataAttribute,
  setData,
  isRequired = false,
  disabled = false,
  initialAccounts = [],
  placeholder = 'Gõ tên đăng nhập, họ tên hoặc email để tìm kiếm...',
}) => {
  const [getListAccount] = useGetListAccountHook();

  const [searchKeyword, setSearchKeyword] = useState('');
  const debouncedKeyword = useDebounce(searchKeyword, 300);

  const [loading, setLoading] = useState(false);
  const [options, setOptions] = useState<any[]>([]);
  const [selectedAccounts, setSelectedAccounts] = useState<any[]>([]);
  const [modalVisible, setModalVisible] = useState(false);

  // Khởi tạo và đồng bộ selectedAccounts khi có initialAccounts hoặc dataAttribute
  useEffect(() => {
    if (initialAccounts && initialAccounts.length > 0) {
      const normalized = initialAccounts.map((item) => {
        if (item.user) {
          return {
            id: item.userId || item.user.id,
            userName: item.user.userName,
            fullName: item.user.fullName,
            email: item.user.email,
            phoneNumber: item.user.phoneNumber,
          };
        }
        return item;
      });
      setSelectedAccounts(normalized);
    } else if (Array.isArray(dataAttribute) && dataAttribute.length > 0) {
      setSelectedAccounts((prev) => {
        const existingMap = new Map(prev.map((a) => [Number(a.id || a.value), a]));
        return dataAttribute.map((item: any) => {
          const id = typeof item === 'object' ? (item.id || item.value) : item;
          if (existingMap.has(Number(id))) {
            return existingMap.get(Number(id));
          }
          return {
            id: Number(id),
            value: Number(id),
            userName: typeof item === 'object' ? (item.label || item.userName) : `User #${id}`,
          };
        });
      });
    } else if (!dataAttribute || dataAttribute.length === 0) {
      if (!initialAccounts || initialAccounts.length === 0) {
        setSelectedAccounts([]);
      }
    }
  }, [initialAccounts, dataAttribute]);

  // Tìm kiếm tài khoản vai trò CUSTOMER từ Backend
  const searchAccounts = async (keyword: string) => {
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

    if (keyword.trim()) {
      searchByFields.push({
        searchFieldName: 'userName',
        searchValue: keyword.trim(),
      });
    }

    const params = new GetListAccountRequest({
      pageSize: 30,
      searchByFields,
      sortByFields: [{ colName: 'id', sortDirection: 'desc' }],
    });

    await getListAccount(
      params,
      (res: DataResponse) => {
        const data = res?.data;
        const list = data?.data || [];
        setOptions(list);
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    searchAccounts(debouncedKeyword);
  }, [debouncedKeyword]);

  // Ghép các accounts đã chọn vào options nếu chưa có
  const allOptions = [...options];
  selectedAccounts.forEach((acc) => {
    const accId = Number(acc.id || acc.value);
    if (!allOptions.some((o) => o.id === accId)) {
      allOptions.unshift({
        id: accId,
        userName: acc.userName || acc.label,
        fullName: acc.fullName,
        email: acc.email,
        phoneNumber: acc.phoneNumber,
      });
    }
  });

  const selectedIds = selectedAccounts.map((a) => Number(a.id || a.value));

  const handleSelectChange = (values: number[]) => {
    const updatedAccounts = values.map((val) => {
      const found = allOptions.find((o) => o.id === val);
      if (found) return found;
      const prevFound = selectedAccounts.find((a) => Number(a.id || a.value) === val);
      return prevFound || { id: val, value: val, userName: `User #${val}` };
    });

    setSelectedAccounts(updatedAccounts);
    const updateObj = {
      [attribute]: values,
      userIds: values,
    };
    setData((prev: any) => ({
      ...prev,
      ...updateObj,
    }));
    setData(updateObj);
  };

  const handleRemoveAccount = (idToRemove: number | string) => {
    const updatedAccounts = selectedAccounts.filter(
      (a) => Number(a.id || a.value) !== Number(idToRemove)
    );
    const updatedIds = updatedAccounts.map((a) => Number(a.id || a.value));

    setSelectedAccounts(updatedAccounts);
    const updateObj = {
      [attribute]: updatedIds,
      userIds: updatedIds,
    };
    setData((prev: any) => ({
      ...prev,
      ...updateObj,
    }));
    setData(updateObj);
  };

  const handleConfirmModal = (confirmedAccounts: any[]) => {
    setSelectedAccounts(confirmedAccounts);
    const updatedIds = confirmedAccounts.map((a) => Number(a.id || a.value));
    const updateObj = {
      [attribute]: updatedIds,
      userIds: updatedIds,
    };
    setData((prev: any) => ({
      ...prev,
      ...updateObj,
    }));
    setData(updateObj);
  };

  return (
    <div className='flex flex-col w-full'>
      {/* Label chuẩn mực khớp với InputTextCommon */}
      {label && (
        <Row className='mb-1 h-5 flex justify-between items-center'>
          <div className='flex items-center'>
            <span className='font-bold text-14-20 text-[#3B3B3B]'>{label}</span>
            {isRequired && <span className='ml-1 text-red-ff0000'>*</span>}
          </div>
          <span className='text-12-16 text-gray-400 font-normal'>
            (Chỉ áp dụng tài khoản vai trò Khách hàng)
          </span>
        </Row>
      )}

      {/* Input Group: Space.Compact gắn kết Select + Nút Tra Cứu thành khối đồng nhất */}
      <Space.Compact block className='w-full'>
        <AccountSelectStyled
          mode='multiple'
          showSearch
          value={selectedIds}
          placeholder={placeholder}
          style={{ width: 'calc(100% - 92px)', minHeight: '38px' }}
          className='flex-1'
          filterOption={false}
          onSearch={setSearchKeyword}
          onChange={handleSelectChange}
          disabled={disabled}
          loading={loading}
          notFoundContent={
            loading ? <Spin size='small' className='p-2' /> : 'Không tìm thấy tài khoản phù hợp'
          }
        >
          {allOptions.map((item) => (
            <Select.Option key={item.id} value={item.id}>
              <div className='py-0.5 flex items-center justify-between gap-2'>
                <div className='flex items-center gap-1.5 truncate'>
                  <span className='font-bold text-blue-700'>{item.userName}</span>
                  {item.fullName && (
                    <span className='text-gray-700 truncate font-medium'>({item.fullName})</span>
                  )}
                </div>
                {item.email && (
                  <span className='text-11-14 text-gray-400 truncate'>{item.email}</span>
                )}
              </div>
            </Select.Option>
          ))}
        </AccountSelectStyled>

        {/* Nút Tra Cứu Tài Khoản */}
        <Tooltip title='Mở bảng danh sách tra cứu tài khoản nâng cao'>
          <Button
            type='default'
            icon={<FiSearch size={15} className='text-gray-600' />}
            onClick={() => setModalVisible(true)}
            disabled={disabled}
            style={{ width: '92px', height: '38px' }}
            className='h-[38px] px-3 bg-gray-50 hover:bg-gray-100 border-[#a6a6a7db] text-[#3B3B3B] hover:text-red-700 font-medium flex items-center justify-center gap-1.5 shadow-none'
          >
            <span className='text-13-18'>Tra cứu</span>
          </Button>
        </Tooltip>
      </Space.Compact>

      {/* Danh sách thẻ tài khoản đã liên kết */}
      <LinkedAccountsList
        accounts={selectedAccounts}
        onRemove={disabled ? () => {} : handleRemoveAccount}
        disabled={disabled}
      />

      {/* Modal Tra Cứu Tài Khoản */}
      <AccountSearchModal
        visible={modalVisible}
        onClose={() => setModalVisible(false)}
        onConfirm={handleConfirmModal}
        alreadySelectedIds={selectedIds}
      />
    </div>
  );
};

export default UserAccountSelector;
