import Result from 'antd/lib/result';

import { _t } from '@/infrastructure/utils/translation';

const PermissionDeniedPage = () => {
  return (
    <>
      <Result
        icon={<></>}
        style={{ width: '100%' }}
        status='403'
        title='403'
        subTitle={_t("You don't have permission to access this path")}
      />
    </>
    // <div className="permisstions-ui">
    //             <div className='title'>
    //                 403
    //             </div>
    //             <div className='sub-title'>
    //             You don't have permisstion to access this path
    //             </div>

    // </div>
  );
};

export default PermissionDeniedPage;
