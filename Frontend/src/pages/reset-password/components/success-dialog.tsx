import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const SuccessDialog = () => {
  const navigate = useNavigate();
  const [countdown, setCountdown] = useState(3);

  useEffect(() => {
    const timer = setInterval(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    if (countdown === 0) {
      navigate(ROUTE_PATH.LOGIN);
    }

    return () => clearInterval(timer);
  }, [countdown, navigate]);

  return (
    <div className={'flex items-center justify-center h-screen'}>
      <Card className={'w-fit shadow-md p-10'}>
        <div className='text-center'>
          <h2 className={'text-30-40 font-bold'}>
            {_t('Đặt lại mật khẩu thành công!')} <br />
            {_t(`Hệ thống sẽ tự động chuyển hướng về trang đăng nhập sau ${countdown} giây`)}
          </h2>
        </div>
      </Card>
    </div>
  );
};

export default SuccessDialog;
