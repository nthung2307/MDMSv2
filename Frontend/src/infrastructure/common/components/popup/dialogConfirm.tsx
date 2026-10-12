import { InfoCircleOutlined } from '@ant-design/icons';
import { Modal } from 'antd';

import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
type Props = {
  title: string;
  message: any;
  titleCancel: string;
  titleOk: string;
  visible: boolean;
  handleCancel: () => void;
  handleOk: () => void;
  classBtnSubmit?: 'blue-bg' | 'cancel';
  isShowCancel?: boolean;
  isWarning?: boolean;
};

const DialogConfirmCommon = (props: Props) => {
  const {
    title,
    isWarning,
    message,
    titleCancel,
    titleOk,
    handleOk,
    handleCancel,
    visible,
    isShowCancel = true,
  } = props;

  return (
    <Modal
      key={'f-0'}
      centered
      className='modal-common'
      visible={visible}
      closable={true}
      footer={false}
      onCancel={handleCancel}
    >
      <div className='modal'>
        {isWarning ? (
          <div className={'flex flex-col justify-center items-center gap-2'}>
            <InfoCircleOutlined style={{ color: '#F4C519', fontSize: '50px' }} />
            <div className={`mb-4 title text-center`} style={{ color: '#D1870A' }}>
              {_t('Warning')}
            </div>
          </div>
        ) : (
          <div className={`mb-4 title text-left`}>{title}</div>
        )}
        <div className={`mb-4 message-content ${isWarning ? 'text-center' : 'text-left'}`}>
          {message}
        </div>
        <div className='flex justify-center gap-3'>
          {isShowCancel && (
            <ButtonCommon
              title={titleCancel}
              colorClass={'cancel'}
              onClick={handleCancel}
              classButton={'min-w-[100px]'}
            />
          )}
          <ButtonCommon
            title={titleOk}
            colorClass={'blue-bg'}
            onClick={handleOk}
            classButton={'min-w-[100px]'}
          />
        </div>
      </div>
    </Modal>
  );
};
export default DialogConfirmCommon;
