import { useNavigate } from 'react-router-dom';
import { LuMailCheck } from 'react-icons/lu';

import { LogoLandscape } from '@/assets/icons';
import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

interface EmailSentProps {
  email?: string;
}

const EmailSent = ({ email }: EmailSentProps) => {
  const navigate = useNavigate();

  return (
    <>
      <PageTitle title={'Đã gửi email khôi phục'} />
      <div className='w-screen h-screen flex justify-center items-center px-5 bg-[#f5f7fc]'>
        <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10 flex flex-col items-center text-center'>
          <LogoLandscape width={100} className='mb-6' />

          <div className='w-20 h-20 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center mb-5 shadow-inner'>
            <LuMailCheck size={40} />
          </div>

          <h2 className='text-22-28 font-bold text-gray-800 mb-2'>
            {_t('Kiểm tra hòm thư của bạn')}
          </h2>

          <p className='text-sm text-gray-600 leading-relaxed mb-4'>
            {email ? (
              <>
                {_t('Chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến địa chỉ')} <br />
                <span className='font-semibold text-gray-800'>{email}</span>
              </>
            ) : (
              _t('Liên kết khôi phục mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư đến (hoặc thư mục Spam/Quảng cáo).')
            )}
          </p>

          <div className='w-full bg-amber-50 border border-amber-200 rounded-md p-3 text-xs text-amber-800 mb-6 text-left leading-relaxed'>
            <span className='font-semibold'>Lưu ý:</span> Liên kết khôi phục mật khẩu chỉ có hiệu lực trong vòng <strong>10 phút</strong> vì lý do bảo mật.
          </div>

          <ButtonCommon
            title={_t('Quay lại trang Đăng nhập')}
            colorClass='firebrick-bg'
            classButton='w-full'
            onClick={() => navigate(ROUTE_PATH.LOGIN, { replace: true })}
          />

          <div className='mt-5'>
            <button
              type='button'
              onClick={() => window.location.reload()}
              className='text-xs text-gray-500 hover:text-gray-800 transition-colors underline cursor-pointer'
            >
              {_t('Chưa nhận được email? Nhấp vào đây để gửi lại')}
            </button>
          </div>
        </div>
      </div>
    </>
  );
};

export default EmailSent;
