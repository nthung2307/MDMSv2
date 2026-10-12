import { Col, Modal, Row } from 'antd';
import React from 'react';

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
    titleBtnOk = 'Lưu',
    showFooter = true,
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
        onCancel={() => handleCancel()}
      >
        <div className='modal'>
          {title ? <div className='mb-4 title text-left'>{title}</div> : null}
          {children}
          {showFooter ? (
            <Row gutter={20}>
              <Col md={12} sm={24} xs={24}>
                <ButtonCommon
                  title={_t('Huỷ')}
                  colorClass={'cancel'}
                  onClick={handleCancel}
                  classButton={'w-full'}
                />
              </Col>
              <Col md={12} sm={24} xs={24}>
                <ButtonCommon
                  title={_t(titleBtnOk)}
                  colorClass={classBtnSubmit ? classBtnSubmit : 'blue-bg'}
                  onClick={() => handleConfirm()}
                  classButton={'w-full'}
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
