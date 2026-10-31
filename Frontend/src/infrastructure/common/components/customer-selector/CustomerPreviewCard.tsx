import React from 'react';
import { FiExternalLink, FiMapPin, FiPhone, FiUser, FiX } from 'react-icons/fi';
import { MdOutlineMail } from 'react-icons/md';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';

const checkCustomerActive = (status: any) => {
  if (typeof status === 'boolean') return status;
  const s = String(status || '').trim().toUpperCase();
  return s === 'ACTIVE' || s === '1' || s === 'TRUE';
};

interface CustomerPreviewCardProps {
  customer: any;
  onClear?: () => void;
  disabled?: boolean;
}

export const CustomerPreviewCard: React.FC<CustomerPreviewCardProps> = ({
  customer,
  onClear,
  disabled = false,
}) => {
  if (!customer) return null;

  const isActive = checkCustomerActive(customer.status);
  const detailUrl = ROUTE_PATH.CUSTOMER_DETAIL.replace(
    Constants.ReplaceRouterConfig.id,
    customer.id?.toString() || ''
  );

  return (
    <div className='mt-2.5 p-3.5 bg-blue-50/60 border border-blue-200 rounded-lg text-13-18 text-gray-800 shadow-xs transition-all'>
      <div className='flex items-center justify-between pb-2 border-b border-blue-100 mb-2.5'>
        <div className='flex items-center gap-2'>
          <div className='w-7 h-7 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold'>
            <FiUser size={15} />
          </div>
          <div>
            <div className='flex items-center gap-2'>
              <span className='font-bold text-14-20 text-gray-900'>
                {customer.fullName || 'Khách hàng chưa đặt tên'}
              </span>
              <StatusTagCommon
                status={isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
                bgColor={isActive ? '#41A026' : '#B22222'}
                textColor={'#FFFFFF'}
              />
            </div>
          </div>
        </div>

        <div className='flex items-center gap-2'>
          <ActionDetailLink
            href={detailUrl}
            title={`Hồ sơ khách hàng: ${customer.fullName || customer.customerCode}`}
            label='Xem hồ sơ'
            className='text-blue-600 hover:text-blue-800 text-12-16 font-medium hover:underline inline-flex items-center gap-1 cursor-pointer'
            icon={<FiExternalLink size={13} />}
          />
          {!disabled && onClear && (
            <button
              type='button'
              onClick={onClear}
              className='ml-2 text-gray-400 hover:text-red-600 transition-colors p-1 rounded hover:bg-red-50 flex items-center gap-0.5 text-12-16'
              title='Bỏ chọn khách hàng này'
            >
              <FiX size={15} />
              <span>Đổi</span>
            </button>
          )}
        </div>
      </div>

      <div className='grid grid-cols-1 md:grid-cols-3 gap-2 text-gray-700 text-12-16'>
        <div className='flex items-center gap-1.5 truncate'>
          <span className='text-gray-400 font-medium'>Mã KH:</span>
          <span className='font-semibold text-blue-700 bg-blue-100 px-1.5 py-0.5 rounded text-11-14'>
            {customer.customerCode || customer.code || 'N/A'}
          </span>
        </div>

        <div className='flex items-center gap-1.5 truncate'>
          <FiPhone className='text-gray-400 flex-shrink-0' size={13} />
          <span className='text-gray-400 font-medium'>SĐT:</span>
          <span className='font-medium'>{customer.phoneNumber || 'Chưa cập nhật'}</span>
        </div>

        <div className='flex items-center gap-1.5 truncate'>
          <MdOutlineMail className='text-gray-400 flex-shrink-0' size={14} />
          <span className='text-gray-400 font-medium'>Email:</span>
          <span className='font-medium truncate'>{customer.email || 'Chưa cập nhật'}</span>
        </div>
      </div>

      {customer.address && (
        <div className='mt-2 flex items-start gap-1.5 text-12-16 text-gray-600'>
          <FiMapPin className='text-gray-400 flex-shrink-0 mt-0.5' size={13} />
          <span className='text-gray-400 font-medium flex-shrink-0'>Địa chỉ:</span>
          <span className='truncate'>{customer.address}</span>
        </div>
      )}
    </div>
  );
};

export default CustomerPreviewCard;
