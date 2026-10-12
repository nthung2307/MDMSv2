import { Collapse } from 'antd';
import React from 'react';

import icArrowUp from '@/assets/icons/ic_arrow_up.svg';

type Props = {
  header?: React.ReactNode;
  children?: React.ReactNode;
};

const CollapseCommon = (props: Props) => {
  const { header, children } = props;
  return (
    <Collapse
      bordered
      expandIconPosition='right'
      accordion
      ghost
      defaultActiveKey={['1']}
      expandIcon={({ isActive }) => (
        <img
          src={icArrowUp}
          alt={''}
          className={`${!isActive ? 'rotate-180' : ''} w-[28px] h-[28px]`}
        />
      )}
    >
      <Collapse.Panel header={header} key='1'>
        {children}
      </Collapse.Panel>
    </Collapse>
  );
};
export default CollapseCommon;
