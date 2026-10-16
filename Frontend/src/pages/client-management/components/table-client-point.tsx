import Column from 'antd/es/table/Column';

import Constants from '@/core/application/common/constants';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  dataSource: any;
  setData: any;
  data: any;
  disabled: boolean;
  validate: any;
  setValidate: any;
};

const TableClientPoint = (props: Props) => {
  const { dataSource, setData, data, disabled, validate, setValidate } = props;
  const setDataEmail = (dataEmail: any, id: any, attr: string) => {
    let arr = data.measurementPointCustomers;
    const pointTarget = arr.find((item: any) => item.measurementPointId == id);
    pointTarget[attr] = dataEmail;
    setData({
      measurementPointCustomers: arr,
    });
  };

  // const checkEmpty = (value: any) => {
  //   if (!value || value == '') return false;
  //   return true;
  // };

  return (
    <>
      <TableCommon
        dataSource={[...dataSource]}
        rowKey={(record: any) => record.idTable}
        pagination={false}
        className='table-common table-point-client'
        scroll={{ x: 'max-content' }}
        // loading={loadingState.isLoading}
      >
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Mã điểm đo')}</div>}
          dataIndex='code'
          key='code'
          fixed={'left'}
          width={'100px'}
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
          width={'150px'}
          render={(val) => (
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        />
        {/* <Column
          title={<div className='text-14-20 font-bold'>{_t('Địa chỉ')}</div>}
          dataIndex='address'
          key='address'
          width={'300px'}
          render={(val) => (S
            <div className={`attribute-input `}>
              <p>{val}</p>
            </div>
          )}
        /> */}
        <Column
          title={<div className='text-14-20 font-bold'>{_t('Mô tả')}</div>}
          dataIndex='description'
          key='description'
          width={'500px'}
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
            const statusText = isActive ? 'Đang sử dụng' : 'Ngừng sử dụng';
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
        <Column
          title={
            <div className='text-14-20 font-bold text-center'>{_t('Người nhận phiếu báo')}</div>
          }
          dataIndex='value'
          key='value'
          width={'600px'}
          render={(_, record, index) => {
            return (
              <div className={`attribute-input select-custom-client flex gap-3`}>
                <InputTextCommon
                  dataAttribute={record?.email}
                  label={''}
                  attribute={`email-${index}`}
                  setData={() => {}}
                  onCallbackFunction={(dataEmail: any) =>
                    setDataEmail(dataEmail, record.id, 'email')
                  }
                  placeholder={'Nhập email'}
                  isRequired={false}
                  disabled={disabled}
                  validate={validate}
                  setValidate={setValidate}
                  rules={[
                    {
                      message: _t('Không đúng định dạng email'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                />
                <InputTextCommon
                  dataAttribute={record?.email2}
                  label={''}
                  placeholder={'Nhập email'}
                  attribute={`email2-${index}`}
                  setData={() => {}}
                  onCallbackFunction={(dataEmail: any) =>
                    setDataEmail(dataEmail, record.id, 'email2')
                  }
                  disabled={disabled ?? !record?.email}
                  rules={[
                    {
                      message: _t('Không đúng định dạng email'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                  validate={validate}
                  setValidate={setValidate}
                  isRequired={false}
                />
                <InputTextCommon
                  dataAttribute={record?.email3}
                  label={''}
                  placeholder={'Nhập email'}
                  attribute={`email3-${index}`}
                  setData={() => {}}
                  onCallbackFunction={(dataEmail: any) =>
                    setDataEmail(dataEmail, record.id, 'email3')
                  }
                  disabled={disabled}
                  rules={[
                    {
                      message: _t('Không đúng định dạng email'),
                      pattern: Constants.PATTERN_REGEX.Email,
                    },
                  ]}
                  validate={validate}
                  setValidate={setValidate}
                  isRequired={false}
                />
              </div>
            );
          }}
        />
      </TableCommon>
    </>
  );
};

export default TableClientPoint;
