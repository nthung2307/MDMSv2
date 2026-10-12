import { Result } from 'antd';

import { _t } from '@/infrastructure/utils/translation';

export const NotFound = () => {
  return (
    <Result
      icon={<></>}
      style={{ width: '100%' }}
      status='404'
      title='404'
      subTitle={_t('Not Found')}
    />
  );
};
