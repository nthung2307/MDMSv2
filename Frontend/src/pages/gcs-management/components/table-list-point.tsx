import Column from 'antd/es/table/Column';

import Constants from '@/core/application/common/constants';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  dataSource: any;
};

const TableGCSPoint = (props: Props) => {
  const { dataSource } = props;

  return (
    <>
      <TableCommon
        dataSource={[...dataSource]}
        rowKey={(record: any) => record.idTable}
        pagination={false}
        className='table-common table-point-client'
        scroll={{ x: 'max-content' }}
      >
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Mã điểm đo')}</div>}
          dataIndex='code'
          key='code'
          fixed={'left'}
          width={'200px'}
          render={(val) => (
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Loại điểm đo')}</div>}
          dataIndex='type'
          key='type'
          width={'250px'}
          render={(val) => (
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Địa chỉ')}</div>}
          dataIndex='address'
          key='address'
          width={'300px'}
          render={(val) => (
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Mô tả')}</div>}
          dataIndex='description'
          key='description'
          width={'300px'}
          render={(val) => (
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        />
        <Column
          title={<div className='text-14-20 font-bold text-center'>{_t('Trạng thái')}</div>}
          dataIndex='status'
          key='status'
          width={'200px'}
          render={(val) => {
            const isActive = val === Constants.CustomerStatus.Active;
            const backgroundColor = isActive ? '#41A026' : '#B22222';
            const statusText = isActive ? 'Active' : 'Inactive';
            return (
              <div className='flex justify-center w-full'>
                <StatusTagCommon
                  status={statusText}
                  bgColor={backgroundColor}
                  textColor={'#FFFFFF'}
                />
              </div>
            );
          }}
        />
      </TableCommon>
    </>
  );
};

export default TableGCSPoint;
