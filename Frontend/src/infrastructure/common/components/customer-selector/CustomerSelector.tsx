import { Button, Col, Row, Select, Space, Spin, Tooltip } from 'antd';
import React, { useEffect, useState } from 'react';
import { FiSearch } from 'react-icons/fi';
import styled from 'styled-components';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetCustomerDetailRequest } from '@/core/application/dto/customer/request/GetCustomerDetailRequest';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest';
import { CustomerPreviewCard } from '@/infrastructure/common/components/customer-selector/CustomerPreviewCard';
import { CustomerSearchModal } from '@/infrastructure/common/components/customer-selector/CustomerSearchModal';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import useGetCustomerDetailHook from '@/infrastructure/repository/customer/hooks/useGetCustomerDetail.hook';
import useGetListCustomerHook from '@/infrastructure/repository/customer/hooks/useGetCustomerList.hook';

const CustomerSelectStyled = styled(Select)`
  &.ant-select .ant-select-selector {
    height: 38px !important;
    border-color: #a6a6a7db !important;
    display: flex !important;
    align-items: center !important;
    font-size: 14px !important;
  }
  &.ant-select-focused .ant-select-selector {
    border-color: #a6a6a7db !important;
    box-shadow: none !important;
  }
`;

const checkCustomerActive = (status: any) => {
  if (typeof status === 'boolean') return status;
  const s = String(status || '').trim().toUpperCase();
  return s === 'ACTIVE' || s === '1' || s === 'TRUE';
};

interface CustomerSelectorProps {
  label?: React.ReactNode;
  attribute?: string;
  dataAttribute?: any;
  setData: (data: any) => void;
  isRequired?: boolean;
  disabled?: boolean;
  validate?: any;
  setValidate?: (validate: any) => void;
  submittedTime?: any;
  customerData?: any;
  placeholder?: string;
}

export const CustomerSelector: React.FC<CustomerSelectorProps> = ({
  label = 'Mã khách hàng',
  attribute = 'customerId',
  dataAttribute,
  setData,
  isRequired = false,
  disabled = false,
  validate,
  setValidate,
  customerData,
  placeholder = 'Gõ mã KH, tên hoặc SĐT để tìm kiếm...',
}) => {
  const [getListCustomer] = useGetListCustomerHook();
  const [getCustomerDetail] = useGetCustomerDetailHook();

  const [searchKeyword, setSearchKeyword] = useState('');
  const debouncedKeyword = useDebounce(searchKeyword, 300);

  const [loading, setLoading] = useState(false);
  const [options, setOptions] = useState<any[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<any>(customerData || null);
  const [modalVisible, setModalVisible] = useState(false);

  // Tìm kiếm khách hàng qua API khi người dùng gõ
  const searchCustomers = async (keyword: string) => {
    const searchByFields: Array<{ searchFieldName: string; searchValue: string }> = [];

    if (keyword.trim()) {
      searchByFields.push({
        searchFieldName: 'customerCode',
        searchValue: keyword.trim(),
      });
    }

    const params = new GetListCustomerRequest({
      pageSize: 30,
      searchByFields,
      sortByFields: [{ colName: 'id', sortDirection: 'desc' }],
    });

    await getListCustomer(
      params as any,
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
    searchCustomers(debouncedKeyword);
  }, [debouncedKeyword]);

  // Đồng bộ selectedCustomer khi có dataAttribute và customerData từ ngoài truyền vào
  useEffect(() => {
    if (customerData && Number(customerData.id) === Number(dataAttribute)) {
      setSelectedCustomer(customerData);
    } else if (dataAttribute && (!selectedCustomer || Number(selectedCustomer.id) !== Number(dataAttribute))) {
      // Nếu chưa có thông tin chi tiết khách hàng, gọi API get detail
      const params = new GetCustomerDetailRequest({ id: Number(dataAttribute) });
      getCustomerDetail(
        params,
        (res: DataResponse) => {
          if (res?.data) {
            setSelectedCustomer(res.data);
          }
        },
        () => {}
      );
    } else if (!dataAttribute) {
      setSelectedCustomer(null);
    }
  }, [dataAttribute, customerData]);

  const handleSelectCustomer = (customer: any) => {
    setSelectedCustomer(customer);
    const updateObj = {
      [attribute]: customer.id,
      customer: customer,
    };
    setData((prev: any) => ({
      ...prev,
      ...updateObj,
    }));
    setData(updateObj);

    if (setValidate) {
      setValidate((prev: any) => ({
        ...prev,
        [attribute]: true,
      }));
    }
  };

  const handleClear = () => {
    setSelectedCustomer(null);
    const updateObj = {
      [attribute]: null,
      customer: null,
    };
    setData((prev: any) => ({
      ...prev,
      ...updateObj,
    }));
    setData(updateObj);

    if (setValidate && isRequired) {
      setValidate((prev: any) => ({
        ...prev,
        [attribute]: false,
      }));
    }
  };

  // Ghép option đã chọn vào danh sách nếu chưa có trong options
  const allOptions = [...options];
  if (selectedCustomer && !allOptions.some((o) => o.id === selectedCustomer.id)) {
    allOptions.unshift(selectedCustomer);
  }

  const isInvalid = isRequired && validate && validate[attribute] === false;

  return (
    <div className='flex flex-col w-full'>
      {/* Label chuẩn mực khớp 100% với InputTextCommon */}
      {label && (
        <Row className='mb-1 h-5'>
          <Col className='font-bold text-14-20 text-[#3B3B3B]'>
            <label>{label}</label>
          </Col>
          <Col className='is-required'>
            {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
          </Col>
        </Row>
      )}

      {/* Input Group: Space.Compact gắn kết Select + Nút Tra Cứu thành một khối đồng nhất cao 38px */}
      <Space.Compact block className='w-full'>
        <CustomerSelectStyled
          showSearch
          value={dataAttribute ? Number(dataAttribute) : undefined}
          placeholder={placeholder}
          style={{ width: 'calc(100% - 92px)', height: '38px' }}
          className={`flex-1 ${isInvalid ? 'ant-select-status-error' : ''}`}
          filterOption={false}
          onSearch={setSearchKeyword}
          onChange={(val) => {
            if (!val) {
              handleClear();
            } else {
              const found = allOptions.find((o) => o.id === val);
              if (found) handleSelectCustomer(found);
            }
          }}
          allowClear
          disabled={disabled}
          loading={loading}
          notFoundContent={loading ? <Spin size='small' className='p-2' /> : 'Không tìm thấy khách hàng'}
        >
          {allOptions.map((item) => {
            const isActive = checkCustomerActive(item.status);
            return (
              <Select.Option key={item.id} value={item.id}>
                <div className='py-1 flex flex-col gap-0.5 border-b border-gray-100 last:border-b-0'>
                  <div className='flex items-center justify-between gap-2'>
                    <div className='flex items-center gap-1.5 truncate'>
                      <span className='font-bold text-blue-700 bg-blue-50 px-1 py-0.5 rounded text-11-14 flex-shrink-0'>
                        {item.customerCode || item.code}
                      </span>
                      <span className='font-medium text-gray-900 truncate'>{item.fullName}</span>
                    </div>
                    <span
                      className={`text-10-12 px-1.5 py-0.5 rounded-full flex-shrink-0 ${
                        isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
                    </span>
                  </div>
                  {(item.phoneNumber || item.address) && (
                    <div className='text-11-14 text-gray-500 flex items-center gap-2 truncate'>
                      {item.phoneNumber && <span>📞 {item.phoneNumber}</span>}
                      {item.address && <span className='truncate'>📍 {item.address}</span>}
                    </div>
                  )}
                </div>
              </Select.Option>
            );
          })}
        </CustomerSelectStyled>

        {/* Nút Tra Cứu nâng cao */}
        <Tooltip title='Mở bảng danh sách tra cứu khách hàng nâng cao'>
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

      {isInvalid && (
        <span className='text-red-500 text-12-16 mt-1'>Vui lòng chọn mã khách hàng</span>
      )}

      {/* Thẻ tóm tắt thông tin khách hàng đã chọn (Preview Card) */}
      {selectedCustomer && (
        <CustomerPreviewCard
          customer={selectedCustomer}
          onClear={disabled ? undefined : handleClear}
          disabled={disabled}
        />
      )}

      {/* Modal Tra Cứu Khách Hàng */}
      <CustomerSearchModal
        visible={modalVisible}
        onClose={() => setModalVisible(false)}
        onSelect={handleSelectCustomer}
        selectedCustomerId={dataAttribute}
      />
    </div>
  );
};

export default CustomerSelector;
