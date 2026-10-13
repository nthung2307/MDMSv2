import { Card } from 'antd';
import { LuCheck } from 'react-icons/lu';

import { _t } from '@/infrastructure/utils/translation.ts';

const EmailSent = () => {
  return (
    <div className={'flex items-center justify-center h-screen'}>
      <Card className={'w-fit shadow-md p-5'}>
        <div className='text-center flex flex-col justify-center items-center gap-8'>
          <h2 className={'text-30-40 font-extrabold'}>{_t('Quên mật khẩu')}</h2>
          <LuCheck size={120} color={'#5CA52D'} />
          <p className={'text-18-18 text-[#5CA52D]'}>
            {'Link lấy lại mật khẩu đã được gửi đến email của bạn!'}
          </p>
        </div>
      </Card>
    </div>
  );
};

export default EmailSent;
