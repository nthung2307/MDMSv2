import React from 'react';
import { FiCheckCircle, FiTrash2, FiUser, FiX } from 'react-icons/fi';
import { MdOutlineMail } from 'react-icons/md';

interface LinkedAccountsListProps {
  accounts: any[];
  onRemove: (accountId: number | string) => void;
  disabled?: boolean;
}

export const LinkedAccountsList: React.FC<LinkedAccountsListProps> = ({
  accounts = [],
  onRemove,
  disabled = false,
}) => {
  if (!accounts || accounts.length === 0) {
    return (
      <div className='mt-2.5 p-3 bg-gray-50 border border-dashed border-gray-300 rounded-lg text-center text-12-16 text-gray-500'>
        Chưa có tài khoản người dùng nào được liên kết với khách hàng này.
      </div>
    );
  }

  return (
    <div className='mt-2.5 flex flex-col gap-2'>
      <div className='flex items-center justify-between text-12-16 text-gray-600 font-medium'>
        <span>Tài khoản đã liên kết ({accounts.length}):</span>
      </div>

      <div className='grid grid-cols-1 md:grid-cols-2 gap-2'>
        {accounts.map((acc) => {
          const id = acc.id || acc.value || acc.userId;
          const userName = acc.userName || acc.user?.userName || acc.label || `ID: ${id}`;
          const fullName = acc.fullName || acc.user?.fullName || '';
          const email = acc.email || acc.user?.email || '';
          const phone = acc.phoneNumber || acc.user?.phoneNumber || '';

          return (
            <div
              key={id}
              className='p-2.5 bg-white border border-blue-200/80 hover:border-blue-300 rounded-lg shadow-2xs flex items-center justify-between gap-2 transition-all'
            >
              <div className='flex items-center gap-2.5 min-w-0 flex-1'>
                <div className='w-8 h-8 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold flex-shrink-0'>
                  <FiUser size={15} />
                </div>
                <div className='flex flex-col min-w-0'>
                  <div className='flex items-center gap-1.5 truncate'>
                    <span className='font-bold text-13-18 text-blue-700 truncate'>{userName}</span>
                    {fullName && (
                      <span className='text-12-16 text-gray-700 truncate font-medium'>
                        ({fullName})
                      </span>
                    )}
                  </div>
                  {(email || phone) && (
                    <div className='flex items-center gap-2 text-11-14 text-gray-500 truncate'>
                      {email && (
                        <span className='flex items-center gap-0.5 truncate'>
                          <MdOutlineMail size={12} className='flex-shrink-0' />
                          <span className='truncate'>{email}</span>
                        </span>
                      )}
                      {phone && <span className='truncate'>📞 {phone}</span>}
                    </div>
                  )}
                </div>
              </div>

              {!disabled && (
                <button
                  type='button'
                  onClick={() => onRemove(id)}
                  className='p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors flex-shrink-0'
                  title='Gỡ liên kết tài khoản này'
                >
                  <FiX size={16} />
                </button>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default LinkedAccountsList;
