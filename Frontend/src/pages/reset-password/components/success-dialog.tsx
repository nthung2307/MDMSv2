import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { IoCheckmarkCircleOutline } from 'react-icons/io5';

import { LogoLandscape } from '@/assets/icons';
import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

const SuccessDialog = () => {
  const navigate = useNavigate();
  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    const timer = setInterval(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    if (countdown === 0) {
      navigate(ROUTE_PATH.LOGIN, { replace: true });
    }

    return () => clearInterval(timer);
  }, [countdown, navigate]);

  return (
    <>
      <PageTitle title={'Đổi mật khẩu thành công'} />
      <div className='w-screen h-screen flex justify-center items-center px-5 bg-[#f5f7fc]'>
        <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10 flex flex-col items-center text-center'>
          <LogoLandscape width={100} className='mb-6' />

          <div className='w-20 h-20 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center mb-5 shadow-inner'>
            <IoCheckmarkCircleOutline size={44} />
          </div>

          <h2 className='text-22-28 font-bold text-gray-800 mb-2'>
            {_t('Đặt lại mật khẩu thành công!')}
          </h2>

          <p className='text-sm text-gray-600 leading-relaxed mb-6'>
            {_t('Mật khẩu của bạn đã được cập nhật thành công vào hệ thống. Bạn có thể sử dụng mật khẩu mới để đăng nhập ngay bây giờ.')}
          </p>

          <div className='w-full bg-emerald-50/70 border border-emerald-200/80 rounded-md p-3 text-xs text-emerald-800 mb-6'>
            {_t('Hệ thống sẽ tự động chuyển hướng về trang đăng nhập sau')} <strong className='font-bold text-[#b22222]'>{countdown}s</strong>...
          </div>

          <ButtonCommon
            title={_t('Đăng nhập ngay')}
            colorClass='firebrick-bg'
            classButton='w-full'
            onClick={() => navigate(ROUTE_PATH.LOGIN, { replace: true })}
          />
        </div>
      </div>
    </>
  );
};

export default SuccessDialog;
