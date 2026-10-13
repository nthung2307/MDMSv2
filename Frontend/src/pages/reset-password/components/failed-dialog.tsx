import { Card } from 'antd';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

const FailedDialog = () => {
  const navigate = useNavigate();
  return (
    <div className={'flex items-center justify-center h-screen'}>
      <Card className={'w-fit shadow-md p-2'}>
        <div className='text-center'>
          <h2 className={'text-2xl font-bold'}>{_t('Thử đặt lại mật khẩu')}</h2>
          <p className={'mt-10 text-18-18 font-semibold'}>
            {_t('Yêu cầu đặt lại mật khẩu của bạn đã hết hạn hoặc đã được dùng!')}
          </p>
        </div>
        <ButtonCommon
          title={_t('Đồng ý')}
          colorClass='firebrick-bg'
          classButton='w-full mt-10'
          htmlType='submit'
          onClick={() => {
            navigate(ROUTE_PATH.LOGIN, { replace: true });
          }}
        />
      </Card>
    </div>
  );
};

export default FailedDialog;
