import { Link, useNavigate } from 'react-router-dom';
import { IoAlertCircleOutline, IoArrowBackOutline } from 'react-icons/io5';

import { LogoLandscape } from '@/assets/icons';
import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

const FailedDialog = () => {
  const navigate = useNavigate();

  return (
    <>
      <PageTitle title={'Liên kết không khả dụng'} />
      <div className='w-screen h-screen flex justify-center items-center px-5 bg-[#f5f7fc]'>
        <div className='w-[450px] h-fit bg-white rounded-lg shadow-lg p-10 flex flex-col items-center text-center'>
          <LogoLandscape width={100} className='mb-6' />

          <div className='w-20 h-20 rounded-full bg-rose-50 text-rose-600 flex items-center justify-center mb-5 shadow-inner'>
            <IoAlertCircleOutline size={44} />
          </div>

          <h2 className='text-22-28 font-bold text-gray-800 mb-2'>
            {_t('Liên kết không khả dụng')}
          </h2>

          <p className='text-sm text-gray-600 leading-relaxed mb-6'>
            {_t('Yêu cầu đặt lại mật khẩu của bạn đã hết thời hạn hiệu lực (10 phút) hoặc đã được sử dụng trước đó.')}
          </p>

          <ButtonCommon
            title={_t('Gửi lại yêu cầu khôi phục')}
            colorClass='firebrick-bg'
            classButton='w-full mb-4'
            onClick={() => {
              navigate(ROUTE_PATH.FORGOT_PASSWORD, { replace: true });
            }}
          />

          <div className='pt-3 border-t border-gray-100 w-full'>
            <Link
              to={ROUTE_PATH.LOGIN}
              className='text-sm font-semibold text-[#b22222] hover:text-[#8b1a1a] inline-flex items-center gap-1.5 transition-colors'
            >
              <IoArrowBackOutline size={16} />
              {_t('Quay lại trang Đăng nhập')}
            </Link>
          </div>
        </div>
      </div>
    </>
  );
};

export default FailedDialog;
