import { Col, Modal, Row } from 'antd';

import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  title: string;
  visible: boolean;
  handleCancel: () => void;
  classBtnSubmit?: 'blue-bg' | 'cancel';
  width?: number | string;
  children: any;
  handleConfirm: () => void;
  className?: string;
  titleBtnOk?: string;
  isButtonSubmit?: boolean;
  showFooter?: boolean;
  disableSubmitButton?: boolean;
  afterClose?: () => void;
  buttonLoading?: boolean;
};

const DialogCommon = (props: Props) => {
  const {
    title,
    handleCancel,
    visible,
    classBtnSubmit,
    width = 500,
    children,
    handleConfirm,
    className = '',
    titleBtnOk = 'Xác nhận',
    showFooter = true,
    disableSubmitButton = false,
    afterClose = () => {},
    buttonLoading = false,
  } = props;

  return (
    <>
      <Modal
        key={'f-0'}
        centered
        className={`modal-common ${className}`}
        open={visible}
        closable={true}
        footer={false}
        width={width}
        onCancel={handleCancel}
        afterClose={afterClose}
      >
        <div className='modal'>
          {title ? (
            <div className='mb-4 title text-center font-extrabold text-xl'>{title}</div>
          ) : null}
          {children}
          {showFooter ? (
            <Row gutter={20}>
              <Col md={12} sm={24} xs={24}>
                <ButtonCommon
                  title={_t('Huỷ')}
                  colorClass={'firebrick-outline-bg'}
                  onClick={handleCancel}
                  classButton={'w-full'}
                  ghost={true}
                />
              </Col>
              <Col md={12} sm={24} xs={24}>
                <ButtonCommon
                  title={_t(titleBtnOk)}
                  colorClass={classBtnSubmit ? classBtnSubmit : 'firebrick-bg'}
                  onClick={() => handleConfirm()}
                  classButton={'w-full'}
                  disabled={disableSubmitButton}
                  loading={buttonLoading}
                />
              </Col>
            </Row>
          ) : (
            <></>
          )}
        </div>
      </Modal>
    </>
  );
};
export default DialogCommon;
